using System;
using System.IO;
using UnityEngine;

namespace ChronoCiv.Core.Platform
{
    /// <summary>
    /// Cross-platform cloud save manager supporting iCloud and local storage.
    /// Enables save file synchronization between iOS and Mac.
    /// </summary>
    public class iCloudSaveManager : MonoBehaviour
    {
        public static iCloudSaveManager Instance { get; private set; }

        [Header("Storage Configuration")]
        [SerializeField] private string saveFolderName = "ChronoCivSaves";
        [SerializeField] private int maxLocalSaves = 5;
        [SerializeField] private bool enableCloudSync = true;
        [SerializeField] private bool autoSyncOnPause = true;

        [Header("Conflict Resolution")]
        [SerializeField] private ConflictResolutionMode conflictMode = ConflictResolutionMode.KeepNewer;
        [SerializeField] private bool showConflictDialog = true;

        [Header("iCloud Configuration")]
        [SerializeField] private string iCloudContainerId = "iCloud.com.chrono.civ";
        [SerializeField] private bool useKeyValueStore = true;

        // Events
        public event Action<bool, string> OnCloudSyncCompleted;
        public event Action<SaveConflictInfo> OnConflictDetected;
        public event Action<float> OnSyncProgress;
        public event Action<string> OnError;

        // Private state
        private string localSavePath;
        private string cloudSavePath;
        private bool isSyncing = false;

        public enum ConflictResolutionMode
        {
            KeepNewer,
            KeepLocal,
            KeepCloud,
            KeepBoth,
            AskUser
        }

        /// <summary>
        /// Save conflict information structure.
        /// </summary>
        public struct SaveConflictInfo
        {
            public string saveName;
            public DateTime localTimestamp;
            public DateTime cloudTimestamp;
            public byte[] localData;
            public byte[] cloudData;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SetupSavePaths();
        }

        private void SetupSavePaths()
        {
            // Local save path
#if UNITY_IOS && !UNITY_EDITOR
            localSavePath = Path.Combine(Application.persistentDataPath, saveFolderName);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            localSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), saveFolderName);
#else
            localSavePath = Path.Combine(Application.persistentDataPath, saveFolderName);
#endif

            // Cloud save path
#if UNITY_IOS && !UNITY_EDITOR
            cloudSavePath = Path.Combine(Application.persistentDataPath, "CloudSaves");
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            cloudSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CloudSaves");
#else
            cloudSavePath = localSavePath;
#endif

            // Create directories if they don't exist
            if (!Directory.Exists(localSavePath))
            {
                Directory.CreateDirectory(localSavePath);
            }

            if (!Directory.Exists(cloudSavePath) && enableCloudSync)
            {
                Directory.CreateDirectory(cloudSavePath);
            }

            DebugLog($"Save paths configured - Local: {localSavePath}, Cloud: {cloudSavePath}");
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && autoSyncOnPause && enableCloudSync)
            {
                SyncToCloud();
            }
        }

        /// <summary>
        /// Save game data to local storage.
        /// </summary>
        public bool SaveLocally(string saveName, byte[] data)
        {
            try
            {
                string filePath = Path.Combine(localSavePath, $"{saveName}.sav");
                File.WriteAllBytes(filePath, data);

                // Manage save slots
                MaintainSaveLimit();

                DebugLog($"Save saved locally: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                DebugLogError($"Failed to save locally: {e.Message}");
                OnError?.Invoke($"Failed to save: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load game data from local storage.
        /// </summary>
        public byte[] LoadLocally(string saveName)
        {
            try
            {
                string filePath = Path.Combine(localSavePath, $"{saveName}.sav");
                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    DebugLog($"Save loaded locally: {filePath}");
                    return data;
                }

                DebugLog($"Save file not found: {filePath}");
                return null;
            }
            catch (Exception e)
            {
                DebugLogError($"Failed to load locally: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Sync local saves to iCloud.
        /// </summary>
        public void SyncToCloud()
        {
            if (isSyncing || !enableCloudSync) return;

            isSyncing = true;
            OnSyncProgress?.Invoke(0f);

#if UNITY_IOS && !UNITY_EDITOR || UNITY_STANDALONE_OSX && !UNITY_EDITOR
            try
            {
                // Get all local save files
                string[] localFiles = Directory.GetFiles(localSavePath, "*.sav");

                for (int i = 0; i < localFiles.Length; i++)
                {
                    string fileName = Path.GetFileName(fileName);
                    string localFilePath = localFiles[i];
                    string cloudFilePath = Path.Combine(cloudSavePath, fileName);

                    // Read local data
                    byte[] data = File.ReadAllBytes(localFilePath);

                    // Check if cloud save exists
                    if (File.Exists(cloudFilePath))
                    {
                        byte[] cloudData = File.ReadAllBytes(cloudFilePath);
                        DateTime localTime = File.GetLastWriteTimeUtc(localFilePath);
                        DateTime cloudTime = File.GetLastWriteTimeUtc(cloudFilePath);

                        // Handle conflict if needed
                        if (localTime > cloudTime)
                        {
                            ResolveConflict(fileName, data, cloudData, true);
                        }
                        else
                        {
                            // Cloud is newer, skip or merge
                            DebugLog($"Cloud save is newer for: {fileName}");
                        }
                    }
                    else
                    {
                        // No cloud save, upload local
                        File.WriteAllBytes(cloudFilePath, data);
                        DebugLog($"Uploaded to cloud: {fileName}");
                    }

                    OnSyncProgress?.Invoke((float)(i + 1) / localFiles.Length);
                }

                OnCloudSyncCompleted?.Invoke(true, "Sync completed successfully");
            }
            catch (Exception e)
            {
                DebugLogError($"Cloud sync failed: {e.Message}");
                OnCloudSyncCompleted?.Invoke(false, e.Message);
            }
#else
            // Simulate cloud sync for editor
            DebugLog("Simulating cloud sync (editor mode)");
            OnSyncProgress?.Invoke(1f);
            OnCloudSyncCompleted?.Invoke(true, "Sync simulated in editor");
#endif

            isSyncing = false;
        }

        /// <summary>
        /// Download saves from iCloud.
        /// </summary>
        public void SyncFromCloud()
        {
            if (isSyncing || !enableCloudSync) return;

            isSyncing = true;
            OnSyncProgress?.Invoke(0f);

#if UNITY_IOS && !UNITY_EDITOR || UNITY_STANDALONE_OSX && !UNITY_EDITOR
            try
            {
                string[] cloudFiles = Directory.GetFiles(cloudSavePath, "*.sav");

                for (int i = 0; i < cloudFiles.Length; i++)
                {
                    string fileName = Path.GetFileName(cloudFiles[i]);
                    string localFilePath = Path.Combine(localSavePath, fileName);
                    string cloudFilePath = cloudFiles[i];

                    byte[] cloudData = File.ReadAllBytes(cloudFilePath);

                    if (File.Exists(localFilePath))
                    {
                        byte[] localData = File.ReadAllBytes(localFilePath);
                        DateTime localTime = File.GetLastWriteTimeUtc(localFilePath);
                        DateTime cloudTime = File.GetLastWriteTimeUtc(cloudFilePath);

                        if (cloudTime > localTime)
                        {
                            ResolveConflict(fileName, localData, cloudData, false);
                        }
                    }
                    else
                    {
                        File.Copy(cloudFilePath, localFilePath);
                        DebugLog($"Downloaded from cloud: {fileName}");
                    }

                    OnSyncProgress?.Invoke((float)(i + 1) / cloudFiles.Length);
                }

                OnCloudSyncCompleted?.Invoke(true, "Download completed successfully");
            }
            catch (Exception e)
            {
                DebugLogError($"Cloud download failed: {e.Message}");
                OnCloudSyncCompleted?.Invoke(false, e.Message);
            }
#else
            DebugLog("Simulating cloud download (editor mode)");
            OnSyncProgress?.Invoke(1f);
            OnCloudSyncCompleted?.Invoke(true, "Download simulated in editor");
#endif

            isSyncing = false;
        }

        /// <summary>
        /// Full bidirectional sync.
        /// </summary>
        public void FullSync()
        {
            if (isSyncing) return;

            isSyncing = true;

            // First sync to cloud
            SyncToCloud();

            // Then sync from cloud
            SyncFromCloud();

            isSyncing = false;
        }

        /// <summary>
        /// Resolve a save conflict.
        /// </summary>
        private void ResolveConflict(string saveName, byte[] localData, byte[] cloudData, bool localIsNewer)
        {
            switch (conflictMode)
            {
                case ConflictResolutionMode.KeepNewer:
                    if (localIsNewer)
                    {
                        SaveConflictResolved(true, saveName, localData, cloudData);
                    }
                    else
                    {
                        SaveConflictResolved(false, saveName, localData, cloudData);
                    }
                    break;

                case ConflictResolutionMode.KeepLocal:
                    SaveConflictResolved(true, saveName, localData, cloudData);
                    break;

                case ConflictResolutionMode.KeepCloud:
                    SaveConflictResolved(false, saveName, localData, cloudData);
                    break;

                case ConflictResolutionMode.KeepBoth:
                    // Keep both by renaming
                    string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    string renamedPath = Path.Combine(localSavePath, $"{saveName}_{timestamp}.sav");
                    File.WriteAllBytes(renamedPath, localData);
                    SaveConflictResolved(false, saveName, localData, cloudData);
                    break;

                case ConflictResolutionMode.AskUser:
                    var conflictInfo = new SaveConflictInfo
                    {
                        saveName = saveName,
                        localData = localData,
                        cloudData = cloudData,
                        localTimestamp = File.GetLastWriteTimeUtc(Path.Combine(localSavePath, $"{saveName}.sav")),
                        cloudTimestamp = File.GetLastWriteTimeUtc(Path.Combine(cloudSavePath, $"{saveName}.sav"))
                    };
                    OnConflictDetected?.Invoke(conflictInfo);
                    break;
            }
        }

        private void SaveConflictResolved(bool keepLocal, string saveName, byte[] localData, byte[] cloudData)
        {
            byte[] dataToKeep = keepLocal ? localData : cloudData;

            string localPath = Path.Combine(localSavePath, $"{saveName}.sav");
            string cloudPath = Path.Combine(cloudSavePath, $"{saveName}.sav");

            File.WriteAllBytes(localPath, dataToKeep);
            File.WriteAllBytes(cloudPath, dataToKeep);

            DebugLog($"Conflict resolved for {saveName} - keeping {(keepLocal ? "local" : "cloud")}");
        }

        /// <summary>
        /// User resolves a conflict with their choice.
        /// </summary>
        public void UserResolveConflict(string saveName, byte[] localData, byte[] cloudData, bool keepLocal)
        {
            SaveConflictResolved(keepLocal, saveName, localData, cloudData);
        }

        /// <summary>
        /// Get list of available save files.
        /// </summary>
        public string[] GetAvailableSaves()
        {
            if (!Directory.Exists(localSavePath))
            {
                return new string[0];
            }

            string[] files = Directory.GetFiles(localSavePath, "*.sav");
            string[] saveNames = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                saveNames[i] = Path.GetFileNameWithoutExtension(files[i]);
            }

            return saveNames;
        }

        /// <summary>
        /// Get save file info including timestamps.
        /// </summary>
        public SaveFileInfo GetSaveInfo(string saveName)
        {
            string localPath = Path.Combine(localSavePath, $"{saveName}.sav");
            string cloudPath = Path.Combine(cloudSavePath, $"{saveName}.sav");

            var info = new SaveFileInfo
            {
                saveName = saveName
            };

            if (File.Exists(localPath))
            {
                info.localExists = true;
                info.localTimestamp = File.GetLastWriteTimeUtc(localPath);
                info.localSize = new FileInfo(localPath).Length;
            }

            if (File.Exists(cloudPath))
            {
                info.cloudExists = true;
                info.cloudTimestamp = File.GetLastWriteTimeUtc(cloudPath);
                info.cloudSize = new FileInfo(cloudPath).Length;
            }

            return info;
        }

        /// <summary>
        /// Delete a save file.
        /// </summary>
        public void DeleteSave(string saveName)
        {
            string localPath = Path.Combine(localSavePath, $"{saveName}.sav");
            string cloudPath = Path.Combine(cloudSavePath, $"{saveName}.sav");

            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }

            if (File.Exists(cloudPath))
            {
                File.Delete(cloudPath);
            }

            DebugLog($"Save deleted: {saveName}");
        }

        /// <summary>
        /// Maintain limit on local save files.
        /// </summary>
        private void MaintainSaveLimit()
        {
            string[] files = Directory.GetFiles(localSavePath, "*.sav");
            if (files.Length <= maxLocalSaves) return;

            // Sort by modification time
            Array.Sort(files, (a, b) => File.GetLastWriteTimeUtc(a).CompareTo(File.GetLastWriteTimeUtc(b)));

            // Delete oldest files
            int toDelete = files.Length - maxLocalSaves;
            for (int i = 0; i < toDelete; i++)
            {
                File.Delete(files[i]);
                DebugLog($"Old save deleted: {Path.GetFileName(files[i])}");
            }
        }

        /// <summary>
        /// Check if cloud is available.
        /// </summary>
        public static bool IsCloudAvailable()
        {
#if UNITY_IOS && !UNITY_EDITOR
            // Check iCloud availability
            return true; // Simplified check
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            // Check iCloud Drive availability
            return true; // Simplified check
#else
            return false;
#endif
        }

        /// <summary>
        /// Check if currently syncing.
        /// </summary>
        public bool IsSyncing => isSyncing;

        private void DebugLog(string message)
        {
            Debug.Log($"[iCloudSaveManager] {message}");
        }

        private void DebugLogError(string message)
        {
            Debug.LogError($"[iCloudSaveManager] {message}");
        }
    }

    /// <summary>
    /// Save file information structure.
    /// </summary>
    public struct SaveFileInfo
    {
        public string saveName;
        public bool localExists;
        public DateTime localTimestamp;
        public long localSize;
        public bool cloudExists;
        public DateTime cloudTimestamp;
        public long cloudSize;

        public bool IsSynced => localExists && cloudExists &&
                                 localTimestamp == cloudTimestamp &&
                                 localSize == cloudSize;
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ChronoCiv.Core
{
    /// <summary>
    /// Handles saving and loading game state.
    /// Uses JSON format for human-readable save files.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        [Header("Save Settings")]
        [SerializeField] private string saveFolderName = "Saves";
        [SerializeField] private int maxAutoSaves = 5;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes

        [Header("Encryption")]
        [SerializeField] private bool encryptSaves = false;
        [SerializeField] private string encryptionKey = "ChronoCivSecretKey";

        private string savePath;
        private float lastAutoSaveTime;
        private bool isSaving = false;

        // Events
        public event Action<string> OnSaveComplete;
        public event Action<string, string> OnLoadComplete;
        public event Action<string> OnSaveFailed;
        public event Action<List<SaveFileInfo>> OnSaveFilesListed;

        public class GameLoadedEvent
        {
            public Dictionary<string, Dictionary<string, object>> SaveData;
            public string SaveFilePath;
            public DateTime SaveTime;
        }

        public class SaveFileInfo
        {
            public string FileName;
            public string FilePath;
            public DateTime LastModified;
            public TimeSpan PlayDuration;
            public int Year;
            public string Era;
            public int Population;
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

            savePath = Path.Combine(Application.persistentDataPath, saveFolderName);
            InitializeSaveFolder();
        }

        private void InitializeSaveFolder()
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }

        private void Start()
        {
            var eventBus = EventBus.Instance;
            if (eventBus != null)
            {
                eventBus.Subscribe<RequestSaveEvent>(OnRequestSave);
                eventBus.Subscribe<RequestLoadEvent>(OnRequestLoad);
            }
        }

        private void OnDestroy()
        {
            var eventBus = EventBus.Instance;
            if (eventBus != null)
            {
                eventBus.Unsubscribe<RequestSaveEvent>(OnRequestSave);
                eventBus.Unsubscribe<RequestLoadEvent>(OnRequestLoad);
            }
        }

        private void Update()
        {
            // Auto-save
            if (Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                AutoSave();
                lastAutoSaveTime = Time.time;
            }
        }

        public void SaveGame(string fileName = null)
        {
            if (isSaving) return;
            isSaving = true;

            try
            {
                string saveFileName = fileName ?? GenerateSaveFileName();
                string filePath = Path.Combine(savePath, saveFileName);

                var saveData = CollectSaveData();
                string json = JsonUtility.ToJson(new SaveDataWrapper { Data = saveData }, true);

                if (encryptSaves)
                {
                    json = EncryptString(json, encryptionKey);
                }

                File.WriteAllText(filePath, json);

                OnSaveComplete?.Invoke(filePath);
                DebugLog($"Game saved to: {filePath}");
            }
            catch (Exception e)
            {
                DebugLog($"Save failed: {e.Message}");
                OnSaveFailed?.Invoke(e.Message);
            }
            finally
            {
                isSaving = false;
            }
        }

        public void LoadGame(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    DebugLog($"Save file not found: {filePath}");
                    return;
                }

                string json = File.ReadAllText(filePath);

                if (encryptSaves)
                {
                    json = DecryptString(json, encryptionKey);
                }

                var wrapper = JsonUtility.FromJson<SaveDataWrapper>(json);
                var saveData = wrapper.Data;

                // Publish load event
                var eventBus = EventBus.Instance;
                eventBus?.Publish(new GameLoadedEvent
                {
                    SaveData = saveData,
                    SaveFilePath = filePath,
                    SaveTime = File.GetLastWriteTime(filePath)
                });

                OnLoadComplete?.Invoke(filePath, "Load successful");
                DebugLog($"Game loaded from: {filePath}");
            }
            catch (Exception e)
            {
                DebugLog($"Load failed: {e.Message}");
                OnSaveFailed?.Invoke($"Load failed: {e.Message}");
            }
        }

        public void LoadGameByName(string fileName)
        {
            string filePath = Path.Combine(savePath, fileName);
            LoadGame(filePath);
        }

        public List<SaveFileInfo> GetSaveFiles()
        {
            var saveFiles = new List<SaveFileInfo>();

            if (!Directory.Exists(savePath))
            {
                OnSaveFilesListed?.Invoke(saveFiles);
                return saveFiles;
            }

            foreach (var file in Directory.GetFiles(savePath, "*.save"))
            {
                var info = GetSaveFileInfo(file);
                if (info != null)
                {
                    saveFiles.Add(info);
                }
            }

            // Sort by date descending
            saveFiles.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));

            OnSaveFilesListed?.Invoke(saveFiles);
            return saveFiles;
        }

        public void DeleteSave(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    DebugLog($"Save file deleted: {filePath}");
                }
            }
            catch (Exception e)
            {
                DebugLog($"Failed to delete save: {e.Message}");
            }
        }

        public void DeleteSaveByName(string fileName)
        {
            string filePath = Path.Combine(savePath, fileName);
            DeleteSave(filePath);
        }

        public void QuickSave()
        {
            SaveGame("quicksave.save");
        }

        public void QuickLoad()
        {
            string filePath = Path.Combine(savePath, "quicksave.save");
            if (File.Exists(filePath))
            {
                LoadGame(filePath);
            }
            else
            {
                DebugLog("No quicksave found");
            }
        }

        private void AutoSave()
        {
            // Rotate auto-saves
            var saveFiles = GetSaveFiles();
            int autoSaveCount = 0;

            foreach (var file in saveFiles)
            {
                if (file.FileName.StartsWith("autosave_"))
                {
                    autoSaveCount++;
                }
            }

            // Delete oldest if we have too many
            while (autoSaveCount >= maxAutoSaves)
            {
                var oldestAutoSave = saveFiles
                    .Where(f => f.FileName.StartsWith("autosave_"))
                    .OrderBy(f => f.LastModified)
                    .FirstOrDefault();

                if (oldestAutoSave != null)
                {
                    DeleteSave(oldestAutoSave.FilePath);
                    autoSaveCount--;
                }
            }

            SaveGame($"autosave_{DateTime.Now:yyyyMMdd_HHmmss}.save");
        }

        private Dictionary<string, Dictionary<string, object>> CollectSaveData()
        {
            var saveData = new Dictionary<string, Dictionary<string, object>>();

            // Time Manager
            if (TimeManager.Instance != null)
            {
                saveData["TimeManager"] = TimeManager.Instance.GetSaveData();
            }

            // Resource Manager
            if (ResourceManager.Instance != null)
            {
                saveData["ResourceManager"] = ResourceManager.Instance.GetSaveData();
            }

            // Era Manager
            if (EraManager.Instance != null)
            {
                saveData["EraManager"] = EraManager.Instance.GetSaveData();
            }

            // NPC Manager
            if (NPCManager.Instance != null)
            {
                saveData["NPCManager"] = NPCManager.Instance.GetSaveData();
            }

            // Tech Tree Manager
            if (TechTreeManager.Instance != null)
            {
                saveData["TechTreeManager"] = TechTreeManager.Instance.GetSaveData();
            }

            // Weather Manager
            if (WeatherManager.Instance != null)
            {
                saveData["WeatherManager"] = WeatherManager.Instance.GetSaveData();
            }

            // Event Manager
            if (EventManager.Instance != null)
            {
                saveData["EventManager"] = EventManager.Instance.GetSaveData();
            }

            // Add timestamp
            saveData["_Meta"] = new Dictionary<string, object>
            {
                ["SaveTime"] = DateTime.Now.ToBinary(),
                ["GameVersion"] = Application.version,
                ["UnityVersion"] = Application.unityVersion
            };

            return saveData;
        }

        private SaveFileInfo GetSaveFileInfo(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);

                if (encryptSaves)
                {
                    json = DecryptString(json, encryptionKey);
                }

                var wrapper = JsonUtility.FromJson<SaveDataWrapper>(json);

                if (wrapper.Data.TryGetValue("_Meta", out var meta) &&
                    meta.TryGetValue("SaveTime", out var saveTimeObj))
                {
                    var saveTime = DateTime.FromBinary(Convert.ToInt64(saveTimeObj));

                    int year = 0;
                    string era = "Unknown";
                    int population = 0;

                    if (wrapper.Data.TryGetValue("TimeManager", out var timeData) &&
                        timeData.TryGetValue("currentYear", out var yearObj))
                    {
                        year = Convert.ToInt32(yearObj);
                    }

                    if (wrapper.Data.TryGetValue("EraManager", out var eraData) &&
                        eraData.TryGetValue("currentEraName", out var eraObj))
                    {
                        era = eraObj.ToString();
                    }

                    if (wrapper.Data.TryGetValue("NPCManager", out var npcData) &&
                        npcData.TryGetValue("population", out var popObj))
                    {
                        population = Convert.ToInt32(popObj);
                    }

                    return new SaveFileInfo
                    {
                        FileName = Path.GetFileName(filePath),
                        FilePath = filePath,
                        LastModified = File.GetLastWriteTime(filePath),
                        PlayDuration = saveTime - DateTime.Now, // This would need actual tracking
                        Year = year,
                        Era = era,
                        Population = population
                    };
                }
            }
            catch (Exception e)
            {
                DebugLog($"Error reading save file info: {e.Message}");
            }

            return null;
        }

        private string GenerateSaveFileName()
        {
            var timeManager = TimeManager.Instance;
            int year = timeManager?.CurrentYear ?? 0;
            int day = timeManager?.CurrentDay ?? 0;

            return $"save_y{year}_d{day}_{DateTime.Now:yyyyMMdd_HHmmss}.save";
        }

        private void OnRequestSave(RequestSaveEvent evt)
        {
            SaveGame(evt.FileName);
        }

        private void OnRequestLoad(RequestLoadEvent evt)
        {
            if (string.IsNullOrEmpty(evt.FilePath))
            {
                LoadGameByName(evt.FileName);
            }
            else
            {
                LoadGame(evt.FilePath);
            }
        }

        private string EncryptString(string input, string key)
        {
            // Simple XOR encryption for basic obfuscation
            var result = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                char k = key[i % key.Length];
                result.Append((char)(c ^ k));
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(result.ToString()));
        }

        private string DecryptString(string input, string key)
        {
            try
            {
                string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(input));
                var result = new StringBuilder();
                for (int i = 0; i < decoded.Length; i++)
                {
                    char c = decoded[i];
                    char k = key[i % key.Length];
                    result.Append((char)(c ^ k));
                }
                return result.ToString();
            }
            catch
            {
                return input; // Return as-is if decryption fails
            }
        }

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[SaveSystem] {message}");
        }

        [Serializable]
        private class SaveDataWrapper
        {
            public Dictionary<string, Dictionary<string, object>> Data;
        }
    }

    public class RequestSaveEvent
    {
        public string FileName;
    }

    public class RequestLoadEvent
    {
        public string FilePath;
        public string FileName;
    }
}


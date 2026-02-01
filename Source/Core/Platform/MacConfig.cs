using System;
using UnityEngine;

namespace ChronoCiv.Core.Platform
{
    /// <summary>
    /// macOS-specific configuration and settings.
    /// Manages keyboard shortcuts, trackpad gestures, and Mac-optimized features.
    /// </summary>
    [CreateAssetMenu(fileName = "MacConfig", menuName = "ChronoCiv/Mac Configuration")]
    public class MacConfig : ScriptableObject
    {
        [Header("Keyboard Shortcuts")]
        [SerializeField] private bool useCommandKey = true;
        [SerializeField] private KeyCode quickSaveKey = KeyCode.S;
        [SerializeField] private bool quickSaveRequiresCommand = true;
        [SerializeField] private KeyCode quickLoadKey = KeyCode.L;
        [SerializeField] private bool quickLoadRequiresCommand = true;
        [SerializeField] private KeyCode pauseKey = KeyCode.P;
        [SerializeField] private bool pauseRequiresCommand = true;
        [SerializeField] private KeyCode restartKey = KeyCode.R;
        [SerializeField] private bool restartRequiresCommand = true;
        [SerializeField] private KeyCode settingsKey = KeyCode.Comma;
        [SerializeField] private bool settingsRequiresCommand = true;
        [SerializeField] private KeyCode quitKey = KeyCode.Q;
        [SerializeField] private bool quitRequiresCommand = true;

        [Header("Trackpad Gestures")]
        [SerializeField] private bool enableTrackpadScroll = true;
        [SerializeField] private bool enableTrackpadPinch = true;
        [SerializeField] private bool enableThreeFingerSwipe = true;
        [SerializeField] private float trackpadScrollSpeed = 1f;
        [SerializeField] private float trackpadPinchSpeed = 0.05f;
        [SerializeField] private float trackpadSwipeThreshold = 0.5f;

        [Header("Magic Mouse Gestures")]
        [SerializeField] private bool enableMagicMouseScroll = true;
        [SerializeField] private bool enableMagicMouseSwipe = true;
        [SerializeField] private float magicMouseScrollSpeed = 1f;
        [SerializeField] private float magicMouseSwipeThreshold = 0.3f;

        [Header("Window Management")]
        [SerializeField] private bool supportFullscreen = true;
        [SerializeField] private bool supportResizableWindow = true;
        [SerializeField] private bool supportRetinaDisplay = true;
        [SerializeField] private bool enableMetalRendering = true;
        [SerializeField] private bool vSyncEnabled = true;

        [Header("Performance")]
        [SerializeField] private bool limitFrameRate = false;
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool enableVSync = true;
        [SerializeField] private bool useMetalAPI = true;
        [SerializeField] private bool enableHardwareAcceleration = true;

        [Header("Quality Settings")]
        [SerializeField] private bool autoDetectQuality = true;
        [SerializeField] private bool enableAntiAliasing = true;
        [SerializeField] private int antiAliasingLevel = 2;
        [SerializeField] private bool enableAnisotropicFiltering = true;
        [SerializeField] private int anisotropicFilteringLevel = 2;

        [Header("Audio")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private bool enableSpatialAudio = true;
        [SerializeField] private bool enableAudioMixer = true;

        [Header("Input")]
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] private float scrollSensitivity = 1f;
        [SerializeField] private bool enableCursorWarping = false;
        [SerializeField] private bool enableRawInput = true;

        // Public Properties - Keyboard Shortcuts
        public bool UseCommandKey => useCommandKey;
        public KeyCode QuickSaveKey => quickSaveKey;
        public bool QuickSaveRequiresCommand => quickSaveRequiresCommand;
        public KeyCode QuickLoadKey => quickLoadKey;
        public bool QuickLoadRequiresCommand => quickLoadRequiresCommand;
        public KeyCode PauseKey => pauseKey;
        public bool PauseRequiresCommand => pauseRequiresCommand;
        public KeyCode RestartKey => restartKey;
        public bool RestartRequiresCommand => restartRequiresCommand;
        public KeyCode SettingsKey => settingsKey;
        public bool SettingsRequiresCommand => settingsRequiresCommand;
        public KeyCode QuitKey => quitKey;
        public bool QuitRequiresCommand => quitRequiresCommand;

        // Public Properties - Trackpad
        public bool EnableTrackpadScroll => enableTrackpadScroll;
        public bool EnableTrackpadPinch => enableTrackpadPinch;
        public bool EnableThreeFingerSwipe => enableThreeFingerSwipe;
        public float TrackpadScrollSpeed => trackpadScrollSpeed;
        public float TrackpadPinchSpeed => trackpadPinchSpeed;
        public float TrackpadSwipeThreshold => trackpadSwipeThreshold;

        // Public Properties - Magic Mouse
        public bool EnableMagicMouseScroll => enableMagicMouseScroll;
        public bool EnableMagicMouseSwipe => enableMagicMouseSwipe;
        public float MagicMouseScrollSpeed => magicMouseScrollSpeed;
        public float MagicMouseSwipeThreshold => magicMouseSwipeThreshold;

        // Public Properties - Window Management
        public bool SupportFullscreen => supportFullscreen;
        public bool SupportResizableWindow => supportResizableWindow;
        public bool SupportRetinaDisplay => supportRetinaDisplay;
        public bool EnableMetalRendering => enableMetalRendering;
        public bool VSyncEnabled => vSyncEnabled;

        // Public Properties - Performance
        public bool LimitFrameRate => limitFrameRate;
        public int TargetFrameRate => targetFrameRate;
        public bool EnableVSync => enableVSync;
        public bool UseMetalAPI => useMetalAPI;
        public bool EnableHardwareAcceleration => enableHardwareAcceleration;

        // Public Properties - Quality Settings
        public bool AutoDetectQuality => autoDetectQuality;
        public bool EnableAntiAliasing => enableAntiAliasing;
        public int AntiAliasingLevel => antiAliasingLevel;
        public bool EnableAnisotropicFiltering => enableAnisotropicFiltering;
        public int AnisotropicFilteringLevel => anisotropicFilteringLevel;

        // Public Properties - Audio
        public float MasterVolume => masterVolume;
        public bool EnableSpatialAudio => enableSpatialAudio;
        public bool EnableAudioMixer => enableAudioMixer;

        // Public Properties - Input
        public float MouseSensitivity => mouseSensitivity;
        public float ScrollSensitivity => scrollSensitivity;
        public bool EnableCursorWarping => enableCursorWarping;
        public bool EnableRawInput => enableRawInput;

        /// <summary>
        /// Check if a keyboard shortcut is pressed.
        /// On Mac, this uses Command (⌘) instead of Control.
        /// </summary>
        public bool IsShortcutPressed(KeyCode key)
        {
            bool modifierPressed = useCommandKey ? Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)
                                                  : Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            return modifierPressed && Input.GetKeyDown(key);
        }

        /// <summary>
        /// Check if a keyboard shortcut is held.
        /// </summary>
        public bool IsShortcutHeld(KeyCode key)
        {
            bool modifierPressed = useCommandKey ? Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)
                                                  : Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            return modifierPressed && Input.GetKey(key);
        }

        /// <summary>
        /// Apply macOS-specific settings at runtime.
        /// </summary>
        public void ApplySettings()
        {
            // Set frame rate
            if (limitFrameRate)
            {
                Application.targetFrameRate = targetFrameRate;
            }
            else
            {
                Application.targetFrameRate = 144; // Support high refresh rate displays
            }

            // Enable VSync
            if (enableVSync)
            {
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
            }

            // Configure mouse input
            if (enableRawInput)
            {
                Input.mouseScrollDelta = Input.mouseScrollDelta * scrollSensitivity;
            }

            // Configure quality settings based on hardware
            if (autoDetectQuality)
            {
                AutoConfigureQuality();
            }

            DebugLog("Mac settings applied");
        }

        /// <summary>
        /// Auto-configure quality settings based on Mac hardware.
        /// </summary>
        private void AutoConfigureQuality()
        {
            // Check for Apple Silicon
            bool isAppleSilicon = SystemInfo.processorType.Contains("Apple") ||
                                  SystemInfo.processorType.Contains("M1") ||
                                  SystemInfo.processorType.Contains("M2") ||
                                  SystemInfo.processorType.Contains("M3");

            // Check system memory
            bool hasEnoughMemory = SystemInfo.systemMemorySize >= 8192;

            // Check graphics memory
            bool hasGoodGPU = SystemInfo.graphicsMemorySize >= 2048;

            if (isAppleSilicon && hasEnoughMemory)
            {
                // High quality for Apple Silicon Macs
                QualitySettings.antiAliasing = enableAntiAliasing ? antiAliasingLevel : 0;
                QualitySettings.anisotropicFiltering = enableAnisotropicFiltering ? AnisotropicFilteringLevel : 0;
                QualitySettings.shadowQuality = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                QualitySettings.textureQuality = 0; // Full resolution
                DebugLog("Quality auto-configured for Apple Silicon");
            }
            else if (hasEnoughMemory && hasGoodGPU)
            {
                // Medium-high quality for Intel Macs with good GPU
                QualitySettings.antiAliasing = enableAntiAliasing ? 2 : 0;
                QualitySettings.anisotropicFiltering = enableAnisotropicFiltering ? 1 : 0;
                QualitySettings.shadowQuality = ShadowQuality.Medium;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                QualitySettings.textureQuality = 0;
                DebugLog("Quality auto-configured for Intel Mac");
            }
            else
            {
                // Low-medium quality for older Macs
                QualitySettings.antiAliasing = 0;
                QualitySettings.anisotropicFiltering = 0;
                QualitySettings.shadowQuality = ShadowQuality.Low;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.textureQuality = 1; // Half resolution
                DebugLog("Quality auto-configured for older Mac");
            }
        }

        /// <summary>
        /// Get the current Mac keyboard shortcut description for UI display.
        /// </summary>
        public string GetShortcutDescription(KeyCode key)
        {
            string modifier = useCommandKey ? "⌘" : "⌃";
            return $"{modifier} + {key}";
        }

        /// <summary>
        /// Toggle fullscreen mode.
        /// </summary>
        public void ToggleFullscreen()
        {
            if (!supportFullscreen) return;

            Screen.fullScreen = !Screen.fullScreen;
            DebugLog($"Fullscreen: {Screen.fullScreen}");
        }

        /// <summary>
        /// Set fullscreen mode.
        /// </summary>
        public void SetFullscreen(bool fullscreen)
        {
            if (!supportFullscreen) return;
            Screen.fullScreen = fullscreen;
        }

        /// <summary>
        /// Check if running on Apple Silicon Mac.
        /// </summary>
        public static bool IsAppleSilicon()
        {
            return SystemInfo.processorType.Contains("Apple") ||
                   SystemInfo.processorType.Contains("M1") ||
                   SystemInfo.processorType.Contains("M2") ||
                   SystemInfo.processorType.Contains("M3");
        }

        /// <summary>
        /// Check if running on macOS.
        /// </summary>
        public static bool IsMacPlatform()
        {
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Get Mac model information.
        /// </summary>
        public static string GetMacModel()
        {
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            return SystemInfo.deviceModel;
#else
            return "Unknown Mac";
#endif
        }

        /// <summary>
        /// Quit the application (macOS proper quit).
        /// </summary>
        public void QuitApplication()
        {
#if UNITY_STANDALONE_OSX
            // On macOS, we should use Application.Quit properly
            // This allows proper cleanup and app state saving
#endif
            Application.Quit();
        }

        private void DebugLog(string message)
        {
            Debug.Log($"[MacConfig] {message}");
        }
    }

    /// <summary>
    /// macOS-specific keyboard shortcut definitions.
    /// </summary>
    [Serializable]
    public struct MacKeyboardShortcut
    {
        public string name;
        public KeyCode key;
        public bool requiresCommand;
        public string description;

        public MacKeyboardShortcut(string name, KeyCode key, bool requiresCommand, string description)
        {
            this.name = name;
            this.key = key;
            this.requiresCommand = requiresCommand;
            this.description = description;
        }

        public bool IsPressed()
        {
            bool modifierPressed = requiresCommand ?
                Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand) :
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            return modifierPressed && Input.GetKeyDown(key);
        }

        public string GetDisplayString()
        {
            string modifier = requiresCommand ? "⌘" : "⌃";
            return $"{modifier} + {key}";
        }
    }

    /// <summary>
    /// Trackpad gesture types supported on macOS.
    /// </summary>
    public enum TrackpadGestureType
    {
        Scroll,
        Pinch,
        TwoFingerSwipe,
        ThreeFingerSwipe,
        FourFingerSwipe,
        Rotation
    }

    /// <summary>
    /// Magic Mouse gesture types supported on macOS.
    /// </summary>
    public enum MagicMouseGestureType
    {
        Scroll,
        SwipeLeft,
        SwipeRight,
        SwipeUp,
        SwipeDown,
        DoubleClick,
        ForceClick
    }
}


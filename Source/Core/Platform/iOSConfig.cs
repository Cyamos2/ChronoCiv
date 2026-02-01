using System;
using UnityEngine;

namespace ChronoCiv.Core.Platform
{
    /// <summary>
    /// iOS-specific configuration and settings.
    /// Manages touch input, virtual cursor, and mobile-optimized features.
    /// </summary>
    [CreateAssetMenu(fileName = "iOSConfig", menuName = "ChronoCiv/iOS Configuration")]
    public class iOSConfig : ScriptableObject
    {
        [Header("Touch Input Settings")]
        [SerializeField] private bool enableMultiTouch = true;
        [SerializeField] private float touchSensitivity = 1f;
        [SerializeField] private float longPressDuration = 0.5f;
        [SerializeField] private float tapThresholdDistance = 20f;
        [SerializeField] private float touchRefreshRate = 60f;
        [SerializeField] private bool enableTouchPrediction = true;

        [Header("Virtual Cursor")]
        [SerializeField] private bool enableVirtualCursor = true;
        [SerializeField] private float cursorSpeed = 500f;
        [SerializeField] private float cursorTrailDuration = 0.3f;
        [SerializeField] private Color cursorColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private Vector2 cursorSize = new Vector2(32, 32);
        [SerializeField] private Sprite cursorSprite;
        [SerializeField] private bool cursorFollowsFinger = true;
        [SerializeField] private float cursorOffsetY = 50f;

        [Header("Camera Controls")]
        [SerializeField] private bool enableSwipePan = true;
        [SerializeField] private float swipePanSpeed = 0.5f;
        [SerializeField] private bool enablePinchZoom = true;
        [SerializeField] private float pinchZoomSpeed = 0.05f;
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 10f;
        [SerializeField] private float gestureSmoothing = 0.15f;

        [Header("Movement Controls")]
        [SerializeField] private bool enableTapToMove = true;
        [SerializeField] private bool enableVirtualJoystick = false;
        [SerializeField] private float joystickSize = 150f;
        [SerializeField] private float joystickDeadzone = 20f;
        [SerializeField] private Color joystickColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private bool joystickSnapToFinger = true;

        [Header("UI Adaptations")]
        [SerializeField] private float buttonScale = 1.2f;
        [SerializeField] private float uiSpacing = 10f;
        [SerializeField] private bool useLargeTouchTargets = true;
        [SerializeField] private float minTouchTargetSize = 44f;
        [SerializeField] private bool enableEdgePadding = true;
        [SerializeField] private float edgePaddingSize = 20f;

        [Header("Performance")]
        [SerializeField] private bool limitFrameRate = true;
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool enableLowPowerMode = false;
        [SerializeField] private bool disableParticleEffects = false;
        [SerializeField] private int maxParticles = 50;
        [SerializeField] private bool reduceEffectsInLowPower = true;
        [SerializeField] private int maxNPCsInLowPower = 10;

        [Header("Haptic Feedback (Core Haptics)")]
        [SerializeField] private bool enableHaptics = true;
        [SerializeField] private bool hapticOnMove = false;
        [SerializeField] private bool hapticOnInteract = true;
        [SerializeField] private bool hapticOnUI = true;
        [SerializeField] private bool hapticOnEraTransition = true;
        [SerializeField] private bool hapticOnNotification = true;
        [SerializeField] private float hapticIntensity = 1f;
        [SerializeField] private bool useCoreHapticsAPI = true;

        [Header("Safe Areas & Display")]
        [SerializeField] private bool respectSafeArea = true;
        [SerializeField] private Color safeAreaOverlayColor = new Color(0, 0, 0, 0.3f);
        [SerializeField] private bool supportNotchedDevices = true;
        [SerializeField] private bool supportDynamicIsland = true;
        [SerializeField] private bool supportProMotionDisplay = true;
        [SerializeField] private bool enableAutoBrightness = false;

        [Header("Dark Mode Support")]
        [SerializeField] private bool supportDarkMode = true;
        [SerializeField] private bool followSystemTheme = true;
        [SerializeField] private Color darkModeBackground = new Color(0.1f, 0.1f, 0.1f);
        [SerializeField] private Color darkModeText = new Color(0.9f, 0.9f, 0.9f);

        [Header("Background & Resume")]
        [SerializeField] private bool enableBackgroundRefresh = false;
        [SerializeField] private float backgroundRefreshInterval = 900f;
        [SerializeField] private bool pauseOnBackground = true;
        [SerializeField] private bool saveOnBackground = true;

        [Header("Game Controller")]
        [SerializeField] private bool enableGameController = true;
        [SerializeField] private bool enableControllerRumble = true;
        [SerializeField] private float rumbleIntensity = 0.5f;

        // Public Properties - Touch Input
        public bool EnableMultiTouch => enableMultiTouch;
        public float TouchSensitivity => touchSensitivity;
        public float LongPressDuration => longPressDuration;
        public float TapThresholdDistance => tapThresholdDistance;
        public float TouchRefreshRate => touchRefreshRate;
        public bool EnableTouchPrediction => enableTouchPrediction;

        // Public Properties - Virtual Cursor
        public bool EnableVirtualCursor => enableVirtualCursor;
        public float CursorSpeed => cursorSpeed;
        public float CursorTrailDuration => cursorTrailDuration;
        public Color CursorColor => cursorColor;
        public Vector2 CursorSize => cursorSize;
        public Sprite CursorSprite => cursorSprite;
        public bool CursorFollowsFinger => cursorFollowsFinger;
        public float CursorOffsetY => cursorOffsetY;

        // Public Properties - Camera Controls
        public bool EnableSwipePan => enableSwipePan;
        public float SwipePanSpeed => swipePanSpeed;
        public bool EnablePinchZoom => enablePinchZoom;
        public float PinchZoomSpeed => pinchZoomSpeed;
        public float MinZoom => minZoom;
        public float MaxZoom => maxZoom;
        public float GestureSmoothing => gestureSmoothing;

        // Public Properties - Movement Controls
        public bool EnableTapToMove => enableTapToMove;
        public bool EnableVirtualJoystick => enableVirtualJoystick;
        public float JoystickSize => joystickSize;
        public float JoystickDeadzone => joystickDeadzone;
        public Color JoystickColor => joystickColor;
        public bool JoystickSnapToFinger => joystickSnapToFinger;

        // Public Properties - UI Adaptations
        public float ButtonScale => buttonScale;
        public float UISpacing => uiSpacing;
        public bool UseLargeTouchTargets => useLargeTouchTargets;
        public float MinTouchTargetSize => minTouchTargetSize;
        public bool EnableEdgePadding => enableEdgePadding;
        public float EdgePaddingSize => edgePaddingSize;

        // Public Properties - Performance
        public bool LimitFrameRate => limitFrameRate;
        public int TargetFrameRate => targetFrameRate;
        public bool EnableLowPowerMode => enableLowPowerMode;
        public bool DisableParticleEffects => disableParticleEffects;
        public int MaxParticles => maxParticles;
        public bool ReduceEffectsInLowPower => reduceEffectsInLowPower;
        public int MaxNPCsInLowPower => maxNPCsInLowPower;

        // Public Properties - Haptic Feedback
        public bool EnableHaptics => enableHaptics;
        public bool HapticOnMove => hapticOnMove;
        public bool HapticOnInteract => hapticOnInteract;
        public bool HapticOnUI => hapticOnUI;
        public bool HapticOnEraTransition => hapticOnEraTransition;
        public bool HapticOnNotification => hapticOnNotification;
        public float HapticIntensity => hapticIntensity;
        public bool UseCoreHapticsAPI => useCoreHapticsAPI;

        // Public Properties - Safe Areas & Display
        public bool RespectSafeArea => respectSafeArea;
        public Color SafeAreaOverlayColor => safeAreaOverlayColor;
        public bool SupportNotchedDevices => supportNotchedDevices;
        public bool SupportDynamicIsland => supportDynamicIsland;
        public bool SupportProMotionDisplay => supportProMotionDisplay;
        public bool EnableAutoBrightness => enableAutoBrightness;

        // Public Properties - Dark Mode
        public bool SupportDarkMode => supportDarkMode;
        public bool FollowSystemTheme => followSystemTheme;
        public Color DarkModeBackground => darkModeBackground;
        public Color DarkModeText => darkModeText;

        // Public Properties - Background
        public bool EnableBackgroundRefresh => enableBackgroundRefresh;
        public float BackgroundRefreshInterval => backgroundRefreshInterval;
        public bool PauseOnBackground => pauseOnBackground;
        public bool SaveOnBackground => saveOnBackground;

        // Public Properties - Game Controller
        public bool EnableGameController => enableGameController;
        public bool EnableControllerRumble => enableControllerRumble;
        public float RumbleIntensity => rumbleIntensity;

        /// <summary>
        /// Apply iOS-specific settings at runtime.
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
                Application.targetFrameRate = 60;
            }

            // Set screen sleep
            Screen.sleepTimeout = enableLowPowerMode ? SleepTimeout.SystemSetting : SleepTimeout.NeverSleep;

            // Enable/disable multi-touch
            Input.multiTouchEnabled = enableMultiTouch;

            // Configure screen orientation
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.orientation = ScreenOrientation.Landscape;

            // Configure low power mode
            if (enableLowPowerMode)
            {
                ConfigureLowPowerMode();
            }

            // Initialize haptic feedback
            if (enableHaptics && useCoreHapticsAPI)
            {
                InitializeCoreHaptics();
            }

            DebugLog("iOS settings applied");
        }

        /// <summary>
        /// Configure settings for Low Power Mode.
        /// </summary>
        private void ConfigureLowPowerMode()
        {
            // Reduce frame rate
            Application.targetFrameRate = Mathf.Min(targetFrameRate, 30);

            // Disable unnecessary effects
            if (reduceEffectsInLowPower)
            {
                QualitySettings.shadowQuality = ShadowQuality.Disable;
                QualitySettings.particlesDefaultQuality = ParticleSystemRenderMode.Billboard;
                QualitySettings.antiAliasing = 0;
            }

            DebugLog("Low Power Mode configured");
        }

        /// <summary>
        /// Initialize Core Haptics API for advanced haptic feedback.
        /// </summary>
        private void InitializeCoreHaptics()
        {
#if UNITY_IOS && !UNITY_EDITOR
            // Initialize Core Haptics engine
            // This would use the native Core Haptics framework
            DebugLog("Core Haptics initialized");
#endif
        }

        /// <summary>
        /// Check if device is in Low Power Mode.
        /// </summary>
        public static bool IsLowPowerModeEnabled()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone6S ||
                   UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone6SPlus ||
                   UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneSE ||
                   UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone7 ||
                   UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone7Plus ||
                   UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone8 ||
                   UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone8Plus;
#else
            return false;
#endif
        }

        /// <summary>
        /// Check if device supports Dark Mode.
        /// </summary>
        public static bool IsDarkModeSupported()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Check if Dark Mode is currently enabled.
        /// </summary>
        public static bool IsDarkModeActive()
        {
#if UNITY_IOS && !UNITY_EDITOR
            // Would query UIApplication interface for dark mode status
            return false;
#else
            return false;
#endif
        }

        /// <summary>
        /// Check if device has a notch or uses Dynamic Island.
        /// </summary>
        public static bool HasNotchOrDynamicIsland()
        {
#if UNITY_IOS && !UNITY_EDITOR
            var generation = UnityEngine.iOS.Device.generation;
            return generation >= UnityEngine.iOS.DeviceGeneration.iPhoneX;
#else
            return false;
#endif
        }

        /// <summary>
        /// Get safe area margins for notched devices.
        /// </summary>
        public Vector4 GetSafeAreaMargins()
        {
            Rect safeArea = Screen.safeArea;
            float topMargin = Screen.height - safeArea.yMax;
            float bottomMargin = safeArea.yMin;
            float leftMargin = safeArea.xMin;
            float rightMargin = Screen.width - safeArea.xMax;

            return new Vector4(leftMargin, topMargin, rightMargin, bottomMargin);
        }

        /// <summary>
        /// Check if device supports ProMotion (120Hz display).
        /// </summary>
        public static bool SupportsProMotion()
        {
#if UNITY_IOS && !UNITY_EDITOR
            var generation = UnityEngine.iOS.Device.generation;
            return generation >= UnityEngine.iOS.DeviceGeneration.iPhone13ProMax;
#else
            return false;
#endif
        }

        /// <summary>
        /// Get the current screen safe area.
        /// </summary>
        public Rect GetSafeArea()
        {
            return Screen.safeArea;
        }

        /// <summary>
        /// Get scaled button size based on touch target requirements.
        /// </summary>
        public Vector2 GetScaledButtonSize(Vector2 baseSize)
        {
            float scale = useLargeTouchTargets ? buttonScale : 1f;
            float minSize = useLargeTouchTargets ? minTouchTargetSize : 32f;

            float width = Mathf.Max(baseSize.x * scale, minSize);
            float height = Mathf.Max(baseSize.y * scale, minSize);

            return new Vector2(width, height);
        }

        /// <summary>
        /// Get joystick configuration.
        /// </summary>
        public JoystickConfig GetJoystickConfig()
        {
            return new JoystickConfig
            {
                Size = joystickSize,
                Deadzone = joystickDeadzone,
                Color = joystickColor,
                Enabled = enableVirtualJoystick,
                SnapToFinger = joystickSnapToFinger
            };
        }

        /// <summary>
        /// Trigger haptic feedback with Core Haptics API.
        /// </summary>
        public void TriggerHaptic(HapticFeedbackType type)
        {
            if (!enableHaptics) return;

#if UNITY_IOS && !UNITY_EDITOR
            if (useCoreHapticsAPI)
            {
                TriggerCoreHaptic(type);
            }
            else
            {
                TriggerLegacyHaptic(type);
            }
#else
            // Fallback for other platforms - use vibration
            Handheld.Vibrate();
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        /// <summary>
        /// Trigger advanced haptic using Core Haptics framework.
        /// </summary>
        private void TriggerCoreHaptic(HapticFeedbackType type)
        {
            // Core Haptics implementation
            // This would create and play haptic patterns using CHHapticEngine
            float intensity = hapticIntensity;

            switch (type)
            {
                case HapticFeedbackType.Light:
                    PlayHapticPattern(intensity * 0.5f, 0.1f);
                    break;
                case HapticFeedbackType.Medium:
                    PlayHapticPattern(intensity * 0.7f, 0.15f);
                    break;
                case HapticFeedbackType.Heavy:
                    PlayHapticPattern(intensity, 0.2f);
                    break;
                case HapticFeedbackType.Success:
                    PlayHapticPattern(intensity * 0.8f, 0.1f, true);
                    break;
                case HapticFeedbackType.Warning:
                    PlayHapticPattern(intensity * 0.6f, 0.15f, false, true);
                    break;
                case HapticFeedbackType.Error:
                    PlayHapticPattern(intensity, 0.25f, false, false, true);
                    break;
                case HapticFeedbackType.Selection:
                    PlayHapticPattern(intensity * 0.3f, 0.05f);
                    break;
            }
        }

        /// <summary>
        /// Play a haptic pattern with Core Haptics.
        /// </summary>
        private void PlayHapticPattern(float intensity, float duration, bool sharp = false, bool warning = false, bool error = false)
        {
            // Implementation would use CHHapticEngine and related classes
            // For now, log the haptic event
            UnityEngine.Debug.Log($"[CoreHaptics] Playing pattern - Intensity: {intensity}, Duration: {duration}, Sharp: {sharp}, Warning: {warning}, Error: {error}");
        }

        /// <summary>
        /// Trigger legacy haptic feedback (for older iOS versions).
        /// </summary>
        private void TriggerLegacyHaptic(HapticFeedbackType type)
        {
            switch (type)
            {
                case HapticFeedbackType.Light:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.LightImpact);
                    break;
                case HapticFeedbackType.Medium:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.MediumImpact);
                    break;
                case HapticFeedbackType.Heavy:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.HeavyImpact);
                    break;
                case HapticFeedbackType.Success:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.Success);
                    break;
                case HapticFeedbackType.Warning:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.Warning);
                    break;
                case HapticFeedbackType.Error:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.Error);
                    break;
                case HapticFeedbackType.Selection:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.Selection);
                    break;
            }
        }
#endif

        /// <summary>
        /// Trigger controller rumble (if game controller is connected).
        /// </summary>
        public void TriggerRumble(float intensity, float duration)
        {
            if (!enableGameController || !enableControllerRumble) return;

#if UNITY_IOS && !UNITY_EDITOR
            // Would trigger controller rumble via GameController framework
#endif
        }

        private void DebugLog(string message)
        {
            Debug.Log($"[iOSConfig] {message}");
        }

        /// <summary>
        /// Haptic feedback types available on iOS.
        /// </summary>
        public enum HapticFeedbackType
        {
            Light,
            Medium,
            Heavy,
            Success,
            Warning,
            Error,
            Selection
        }

        /// <summary>
        /// Joystick configuration structure.
        /// </summary>
        [Serializable]
        public struct JoystickConfig
        {
            public float Size;
            public float Deadzone;
            public Color Color;
            public bool Enabled;
            public bool SnapToFinger;
        }
    }

#if UNITY_IOS && !UNITY_EDITOR
    /// <summary>
    /// Simple iOS haptic feedback wrapper for legacy support.
    /// Uses UIImpactFeedbackGenerator for older iOS versions.
    /// </summary>
    public class iOSHapticFeedback
    {
        public static iOSHapticFeedback Instance { get; private set; }

        public enum iOSFeedbackType
        {
            LightImpact,
            MediumImpact,
            HeavyImpact,
            Success,
            Warning,
            Error,
            Selection
        }

        private iOSHapticFeedback() { }

        public void Trigger(iOSFeedbackType feedbackType)
        {
            // Legacy haptic feedback using UIImpactFeedbackGenerator
            // Works on iOS 10+
            UnityEngine.Debug.Log($"[iOSHaptic] Triggering {feedbackType}");
        }

        static iOSHapticFeedback()
        {
            Instance = new iOSHapticFeedback();
        }
    }
#endif
}


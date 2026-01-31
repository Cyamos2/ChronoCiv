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

        [Header("Virtual Cursor")]
        [SerializeField] private bool enableVirtualCursor = true;
        [SerializeField] private float cursorSpeed = 500f;
        [SerializeField] private float cursorTrailDuration = 0.3f;
        [SerializeField] private Color cursorColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private Vector2 cursorSize = new Vector2(32, 32);
        [SerializeField] private Sprite cursorSprite;

        [Header("Camera Controls")]
        [SerializeField] private bool enableSwipePan = true;
        [SerializeField] private float swipePanSpeed = 0.5f;
        [SerializeField] private bool enablePinchZoom = true;
        [SerializeField] private float pinchZoomSpeed = 0.05f;
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 10f;

        [Header("Movement Controls")]
        [SerializeField] private bool enableTapToMove = true;
        [SerializeField] private bool enableVirtualJoystick = false;
        [SerializeField] private float joystickSize = 150f;
        [SerializeField] private float joystickDeadzone = 20f;
        [SerializeField] private Color joystickColor = new Color(1f, 1f, 1f, 0.5f);

        [Header("UI Adaptations")]
        [SerializeField] private float buttonScale = 1.2f;
        [SerializeField] private float uiSpacing = 10f;
        [SerializeField] private bool useLargeTouchTargets = true;
        [SerializeField] private float minTouchTargetSize = 44f;

        [Header("Performance")]
        [SerializeField] private bool limitFrameRate = true;
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool enableLowPowerMode = false;
        [SerializeField] private bool disableParticleEffects = false;
        [SerializeField] private int maxParticles = 50;

        [Header("Haptic Feedback")]
        [SerializeField] private bool enableHaptics = true;
        [SerializeField] private bool hapticOnMove = false;
        [SerializeField] private bool hapticOnInteract = true;
        [SerializeField] private bool hapticOnUI = true;

        [Header("Safe Areas")]
        [SerializeField] private bool respectSafeArea = true;
        [SerializeField] private Color safeAreaOverlayColor = new Color(0, 0, 0, 0.3f);

        // Public Properties - Touch Input
        public bool EnableMultiTouch => enableMultiTouch;
        public float TouchSensitivity => touchSensitivity;
        public float LongPressDuration => longPressDuration;
        public float TapThresholdDistance => tapThresholdDistance;

        // Public Properties - Virtual Cursor
        public bool EnableVirtualCursor => enableVirtualCursor;
        public float CursorSpeed => cursorSpeed;
        public float CursorTrailDuration => cursorTrailDuration;
        public Color CursorColor => cursorColor;
        public Vector2 CursorSize => cursorSize;
        public Sprite CursorSprite => cursorSprite;

        // Public Properties - Camera Controls
        public bool EnableSwipePan => enableSwipePan;
        public float SwipePanSpeed => swipePanSpeed;
        public bool EnablePinchZoom => enablePinchZoom;
        public float PinchZoomSpeed => pinchZoomSpeed;
        public float MinZoom => minZoom;
        public float MaxZoom => maxZoom;

        // Public Properties - Movement Controls
        public bool EnableTapToMove => enableTapToMove;
        public bool EnableVirtualJoystick => enableVirtualJoystick;
        public float JoystickSize => joystickSize;
        public float JoystickDeadzone => joystickDeadzone;
        public Color JoystickColor => joystickColor;

        // Public Properties - UI Adaptations
        public float ButtonScale => buttonScale;
        public float UISpacing => uiSpacing;
        public bool UseLargeTouchTargets => useLargeTouchTargets;
        public float MinTouchTargetSize => minTouchTargetSize;

        // Public Properties - Performance
        public bool LimitFrameRate => limitFrameRate;
        public int TargetFrameRate => targetFrameRate;
        public bool EnableLowPowerMode => enableLowPowerMode;
        public bool DisableParticleEffects => disableParticleEffects;
        public int MaxParticles => maxParticles;

        // Public Properties - Haptic Feedback
        public bool EnableHaptics => enableHaptics;
        public bool HapticOnMove => hapticOnMove;
        public bool HapticOnInteract => hapticOnInteract;
        public bool HapticOnUI => hapticOnUI;

        // Public Properties - Safe Areas
        public bool RespectSafeArea => respectSafeArea;
        public Color SafeAreaOverlayColor => safeAreaOverlayColor;

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

            DebugLog("iOS settings applied");
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
                Enabled = enableVirtualJoystick
            };
        }

        /// <summary>
        /// Trigger haptic feedback.
        /// </summary>
        public void TriggerHaptic(HapticFeedbackType type)
        {
            if (!enableHaptics) return;

#if UNITY_IOS && !UNITY_EDITOR
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
            }
#else
            // Fallback for other platforms - use vibration
            Handheld.Vibrate();
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
            Error
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
        }
    }

#if UNITY_IOS && !UNITY_EDITOR
    /// <summary>
    /// Simple iOS haptic feedback wrapper.
    /// Requires the iOSHapticFeedback Unity package or equivalent.
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
            // This would use native iOS haptic feedback
            // Implementation depends on available Unity packages
            // For now, this is a placeholder
            UnityEngine.Debug.Log($"[iOSHaptic] Triggering {feedbackType}");
        }

        static iOSHapticFeedback()
        {
            Instance = new iOSHapticFeedback();
        }
    }
#endif
}


using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChronoCiv.Core.Platform
{
    /// <summary>
    /// Unified input manager supporting mouse, touch, and cursor-based control.
    /// Automatically detects platform and adjusts input handling accordingly.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Platform Detection")]
        [SerializeField] private bool isMobilePlatform = false;
        [SerializeField] private InputPlatform currentPlatform = InputPlatform.Auto;

        [Header("Mouse Input (Desktop)")]
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] private bool mouseVisible = true;

        [Header("Touch Input (iOS)")]
        [SerializeField] private bool enableTouch = true;
        [SerializeField] private float touchSensitivity = 1f;
        [SerializeField] private float minPinchDistance = 50f;
        [SerializeField] private float pinchZoomSpeed = 0.1f;

        [Header("Virtual Cursor")]
        [SerializeField] private bool showVirtualCursor = false;
        [SerializeField] private GameObject virtualCursorPrefab;
        [SerializeField] private float cursorSpeed = 10f;
        [SerializeField] private float cursorTrailDuration = 0.5f;

        [Header("Movement")]
        [SerializeField] private float clickMoveThreshold = 5f;

        // Public Properties
        public bool IsMobilePlatform => isMobilePlatform;
        public InputPlatform CurrentPlatform => currentPlatform;
        public Vector2 CursorPosition { get; private set; }
        public bool IsPointerDown { get; private set; }
        public bool IsPointerJustPressed { get; private set; }
        public bool IsPointerJustReleased { get; private set; }
        public float ScrollDelta { get; private set; }
        public Vector2 PointerDelta { get; private set; }
        public int PointerId { get; private set; } = -1;

        // Touch Gesture Properties
        public int TouchCount => Input.touchCount;
        public bool IsMultiTouch => Input.touchCount > 1;
        public float PinchDelta { get; private set; }
        public Vector2 TouchDelta { get; private set; }

        // macOS-specific Properties
        public bool IsMacPlatform => currentPlatform == InputPlatform.macOS;
        public bool IsCommandKeyDown => Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
        public float ScrollWheelDelta => Input.mouseScrollDelta.y;

        // Events
        public event Action OnPointerDown;
        public event Action OnPointerUp;
        public event Action OnPointerClick;
        public event Action<float> OnScroll;
        public event Action<Vector2> OnPointerMove;
        public event Action<float> OnPinchZoom;
        public event Action<Vector2> OnTouchMove;
        public event Action<int> OnPinchStarted;
        public event Action OnPinchEnded;

        // Private State
        private Vector2 lastPointerPosition;
        private Vector2 virtualCursorPosition;
        private PlayerCursor cursorVisual;
        private List<Vector2> cursorTrail = new();
        private bool wasPointerDown = false;
        private bool wasMultiTouch = false;
        private float lastPinchDistance = 0f;

        private enum InputPlatform
        {
            Auto,
            Desktop,
            iOS,
            Android,
            WebGL,
            macOS
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

            DetectPlatform();
        }

        private void Start()
        {
            InitializeVirtualCursor();
        }

        private void DetectPlatform()
        {
#if UNITY_IOS && !UNITY_EDITOR
            isMobilePlatform = true;
            currentPlatform = InputPlatform.iOS;
#elif UNITY_ANDROID && !UNITY_EDITOR
            isMobilePlatform = true;
            currentPlatform = InputPlatform.Android;
#elif UNITY_WEBGL && !UNITY_EDITOR
            isMobilePlatform = IsMobileBrowser();
            currentPlatform = isMobilePlatform ? InputPlatform.WebGL : InputPlatform.Desktop;
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            isMobilePlatform = false;
            currentPlatform = InputPlatform.macOS;
#else
            isMobilePlatform = false;
            currentPlatform = InputPlatform.Desktop;
#endif

            DebugLog($"Platform detected: {currentPlatform} (Mobile: {isMobilePlatform})");
        }

        private bool IsMobileBrowser()
        {
            // Simple mobile browser detection
            return Application.platform == RuntimePlatform.WebGLPlayer &&
                   (SystemInfo.deviceType == DeviceType.Handheld ||
                    UnityEngine.SystemInfo.operatingSystem.Contains("iOS") ||
                    UnityEngine.SystemInfo.operatingSystem.Contains("Android"));
        }

        private void InitializeVirtualCursor()
        {
            if (isMobilePlatform && showVirtualCursor)
            {
                if (virtualCursorPrefab != null)
                {
                    var cursorObj = Instantiate(virtualCursorPrefab, Vector3.zero, Quaternion.identity);
                    cursorVisual = cursorObj.GetComponent<PlayerCursor>();
                    if (cursorVisual != null)
                    {
                        cursorVisual.Initialize(cursorTrailDuration);
                    }
                }
            }
        }

        private void Update()
        {
            UpdateInput();
        }

        private void UpdateInput()
        {
            // Reset frame-specific states
            IsPointerJustPressed = false;
            IsPointerJustReleased = false;
            ScrollDelta = 0f;
            PinchDelta = 0f;
            PointerDelta = Vector2.zero;
            TouchDelta = Vector2.zero;

            if (isMobilePlatform)
            {
                UpdateTouchInput();
            }
            else
            {
                UpdateMouseInput();
            }
        }

        private void UpdateMouseInput()
        {
            // Get current cursor position
            CursorPosition = Input.mousePosition;
            PointerDelta = CursorPosition - lastPointerPosition;

            // Handle scroll
            ScrollDelta = Input.GetAxis("Mouse ScrollWheel");

            // Handle pointer states
            bool currentPointerDown = Input.GetMouseButton(0);

            if (currentPointerDown && !wasPointerDown)
            {
                IsPointerDown = true;
                IsPointerJustPressed = true;
                OnPointerDown?.Invoke();
            }
            else if (!currentPointerDown && wasPointerDown)
            {
                IsPointerDown = false;
                IsPointerJustReleased = true;
                OnPointerUp?.Invoke();

                // Click detected
                if (Vector2.Distance(CursorPosition, lastPointerPosition) < clickMoveThreshold)
                {
                    OnPointerClick?.Invoke();
                }
            }
            else
            {
                IsPointerDown = currentPointerDown;
            }

            // Update pointer move event
            if (PointerDelta.magnitude > 0.1f)
            {
                OnPointerMove?.Invoke(CursorPosition);
            }

            // Update scroll event
            if (Mathf.Abs(ScrollDelta) > 0.001f)
            {
                OnScroll?.Invoke(ScrollDelta);
            }

            lastPointerPosition = CursorPosition;
            wasPointerDown = currentPointerDown;
        }

        private void UpdateTouchInput()
        {
            if (!enableTouch || Input.touchCount == 0)
            {
                IsPointerDown = false;
                if (wasPointerDown)
                {
                    IsPointerJustReleased = true;
                    OnPointerUp?.Invoke();
                }
                wasPointerDown = false;
                return;
            }

            // Handle single touch
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                // Update pointer position
                Vector2 newPosition = touch.position;
                PointerDelta = newPosition - lastPointerPosition;
                CursorPosition = newPosition;

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        IsPointerDown = true;
                        IsPointerJustPressed = true;
                        PointerId = touch.fingerId;
                        OnPointerDown?.Invoke();
                        wasPointerDown = true;
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (wasPointerDown)
                        {
                            OnPointerMove?.Invoke(CursorPosition);
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        IsPointerDown = false;
                        IsPointerJustReleased = true;
                        OnPointerUp?.Invoke();

                        // Check for tap (short duration, minimal movement)
                        if (touch.deltaTime < 0.3f && touch.deltaPosition.magnitude < clickMoveThreshold)
                        {
                            OnPointerClick?.Invoke();
                        }
                        wasPointerDown = false;
                        break;
                }

                lastPointerPosition = newPosition;
            }
            // Handle multi-touch (pinch zoom)
            else if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                // Calculate pinch distance
                float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);

                if (!wasMultiTouch)
                {
                    // Pinch just started
                    OnPinchStarted?.Invoke(1);
                    lastPinchDistance = currentPinchDistance;
                }

                // Calculate pinch delta
                PinchDelta = (currentPinchDistance - lastPinchDistance) * touchSensitivity;

                if (Mathf.Abs(PinchDelta) > 0.1f)
                {
                    OnPinchZoom?.Invoke(PinchDelta);
                }

                // Handle touch movement for virtual cursor
                if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    TouchDelta = (touch0.deltaPosition + touch1.deltaPosition) / 2f;
                    OnTouchMove?.Invoke(TouchDelta);
                }

                lastPinchDistance = currentPinchDistance;
                wasMultiTouch = true;
            }
            else
            {
                wasMultiTouch = false;
            }
        }

        /// <summary>
        /// Get a screen position for world interaction.
        /// </summary>
        public Vector3 GetWorldPosition(Camera camera = null, Plane plane = default)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            Ray ray;
            if (isMobilePlatform)
            {
                // For mobile, use virtual cursor or center of screen
                ray = camera.ScreenPointToRay(CursorPosition);
            }
            else
            {
                ray = camera.ScreenPointToRay(CursorPosition);
            }

            if (plane == default)
            {
                plane = new Plane(Vector3.up, Vector3.zero);
            }

            if (plane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Get world position with custom plane.
        /// </summary>
        public bool TryGetWorldPosition(out Vector3 worldPosition, Camera camera = null, Plane plane = default)
        {
            worldPosition = GetWorldPosition(camera, plane);
            return worldPosition != Vector3.zero;
        }

        /// <summary>
        /// Check if a screen position is over UI.
        /// </summary>
        public bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null &&
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(PointerId);
        }

        /// <summary>
        /// Show or hide the virtual cursor.
        /// </summary>
        public void SetVirtualCursorVisible(bool visible)
        {
            showVirtualCursor = visible;
            if (cursorVisual != null)
            {
                cursorVisual.SetVisible(visible);
            }
        }

        /// <summary>
        /// Set the virtual cursor position.
        /// </summary>
        public void SetVirtualCursorPosition(Vector2 position)
        {
            if (isMobilePlatform)
            {
                virtualCursorPosition = position;
                CursorPosition = position;

                if (cursorVisual != null)
                {
                    cursorVisual.SetPosition(position);
                    cursorTrail.Add(position);
                    if (cursorTrail.Count > 10)
                    {
                        cursorTrail.RemoveAt(0);
                    }
                }
            }
        }

        /// <summary>
        /// Force platform detection (useful for WebGL).
        /// </summary>
        public void ForcePlatformDetection()
        {
            DetectPlatform();
        }

        /// <summary>
        /// Get input mode description for debugging.
        /// </summary>
        public string GetInputModeDescription()
        {
            return $"Platform: {currentPlatform}, Mobile: {isMobilePlatform}, " +
                   $"Touch: {enableTouch}, Pointer: {(IsPointerDown ? "Down" : "Up")}, " +
                   $"Touch Count: {Input.touchCount}";
        }

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[InputManager] {message}");
        }

        /// <summary>
        /// Set platform mode manually (for testing).
        /// </summary>
        public void SetPlatformMode(InputPlatform mode)
        {
            currentPlatform = mode;
            isMobilePlatform = mode == InputPlatform.iOS || mode == InputPlatform.Android || mode == InputPlatform.WebGL;
            DetectPlatform();
        }

        /// <summary>
        /// Check if a macOS-style shortcut is pressed (Command + Key).
        /// On macOS, uses Command key. On other platforms, uses Control key.
        /// </summary>
        public bool IsMacShortcutPressed(KeyCode key)
        {
            if (isMobilePlatform) return false;

#if UNITY_STANDALONE_OSX
            return (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) && Input.GetKeyDown(key);
#else
            return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(key);
#endif
        }

        /// <summary>
        /// Check if a macOS-style shortcut is held (Command + Key).
        /// </summary>
        public bool IsMacShortcutHeld(KeyCode key)
        {
            if (isMobilePlatform) return false;

#if UNITY_STANDALONE_OSX
            return (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) && Input.GetKey(key);
#else
            return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(key);
#endif
        }

        /// <summary>
        /// Get the modifier key name for display (⌘ on Mac, Ctrl otherwise).
        /// </summary>
        public string GetModifierKeyName()
        {
#if UNITY_STANDALONE_OSX
            return "⌘";
#else
            return "Ctrl";
#endif
        }
    }
}


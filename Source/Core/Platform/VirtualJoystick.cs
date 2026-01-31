using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChronoCiv.Core.Platform
{
    /// <summary>
    /// Virtual joystick controller for mobile touch input.
    /// Provides on-screen joystick for player movement on iOS/Android.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        public static VirtualJoystick Instance { get; private set; }

        [Header("Joystick Configuration")]
        [SerializeField] private RectTransform joystickArea;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float joystickSize = 150f;
        [SerializeField] private float handleRange = 1f;
        [SerializeField] private float deadzone = 0.1f;
        [SerializeField] private bool snapToFinger = true;

        [Header("Visual")]
        [SerializeField] private Color baseColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color handleColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private bool useCustomSprites = false;
        [SerializeField] private Sprite baseSprite;
        [SerializeField] private Sprite handleSprite;

        [Header("Output")]
        [SerializeField] private bool normalizeOutput = true;
        [SerializeField] private JoystickOutputMode outputMode = JoystickOutputMode.Both;

        // Public Properties
        public Vector2 InputVector { get; private set; }
        public Vector2 RawInput { get; private set; }
        public float Horizontal => InputVector.x;
        public float Vertical => InputVector.y;
        public bool IsActive { get; private set; }
        public bool IsDragging { get; private set; }

        // Events
        public event Action<VirtualJoystick> OnJoystickDown;
        public event Action<VirtualJoystick> OnJoystickUp;
        public event Action<VirtualJoystick, Vector2> OnJoystickMove;

        // Private State
        private Canvas canvas;
        private Rect joystickRect;
        private Vector2 handleOriginalPosition;
        private Camera mainCamera;
        private int dragFingerId = -1;

        private enum JoystickOutputMode
        {
            HorizontalOnly,
            VerticalOnly,
            Both
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SetupCanvas();
        }

        private void SetupCanvas()
        {
            // Ensure this is on a canvas
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("JoystickCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                transform.SetParent(canvasObj.transform, false);
            }

            mainCamera = Camera.main;
        }

        private void Start()
        {
            SetupJoystick();
        }

        private void SetupJoystick()
        {
            // Set up joystick area if not assigned
            if (joystickArea == null)
            {
                joystickArea = GetComponent<RectTransform>();
            }

            if (joystickArea == null)
            {
                DebugLogError("Joystick area not found!");
                return;
            }

            // Set joystick size
            joystickArea.sizeDelta = Vector2.one * joystickSize;

            // Set up handle if not assigned
            if (joystickHandle == null)
            {
                var handleObj = new GameObject("JoystickHandle");
                handleObj.transform.SetParent(joystickArea, false);
                joystickHandle = handleObj.AddComponent<RectTransform>();
                joystickHandle.anchorMin = Vector2.zero;
                joystickHandle.anchorMax = Vector2.one;
                joystickHandle.sizeDelta = Vector2.zero;
                joystickHandle.anchoredPosition = Vector2.zero;

                var handleImage = handleObj.AddComponent<UnityEngine.UI.Image>();
                handleImage.color = handleColor;
                handleImage.sprite = useCustomSprites ? handleSprite : null;
            }

            handleOriginalPosition = joystickHandle.anchoredPosition;

            // Set up background
            var bgImage = joystickArea.gameObject.GetComponent<UnityEngine.UI.Image>();
            if (bgImage == null)
            {
                bgImage = joystickArea.gameObject.AddComponent<UnityEngine.UI.Image>();
            }
            bgImage.color = baseColor;
            bgImage.sprite = useCustomSprites ? baseSprite : null;

            // Calculate joystick rect in screen space
            UpdateJoystickRect();

            // Initially hide joystick if not configured to show
            if (!IsActive)
            {
                joystickArea.gameObject.SetActive(false);
            }
        }

        private void UpdateJoystickRect()
        {
            if (canvas == null || joystickArea == null) return;

            joystickRect = new Rect(
                joystickArea.position.x - joystickArea.sizeDelta.x / 2f,
                joystickArea.position.y - joystickArea.sizeDelta.y / 2f,
                joystickArea.sizeDelta.x,
                joystickArea.sizeDelta.y
            );
        }

        private void Update()
        {
            if (IsDragging && Input.touchCount > 0)
            {
                // Check if dragging finger is still valid
                bool fingerFound = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).fingerId == dragFingerId)
                    {
                        fingerFound = true;
                        break;
                    }
                }

                if (!fingerFound)
                {
                    ResetJoystick();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsDragging) return;

            // Check if touch should activate joystick
            Vector2 touchPosition = eventData.position;

            if (snapToFinger)
            {
                // Snap joystick to finger position
                joystickArea.position = touchPosition;
                UpdateJoystickRect();
            }

            dragFingerId = eventData.pointerId;
            IsDragging = true;
            IsActive = true;
            joystickArea.gameObject.SetActive(true);

            HandleDrag(eventData);
            OnJoystickDown?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging || eventData.pointerId != dragFingerId) return;

            HandleDrag(eventData);
            OnJoystickMove?.Invoke(this, InputVector);
        }

        private void HandleDrag(PointerEventData eventData)
        {
            Vector2 pointerPosition = eventData.position;
            Vector2 joystickCenter = joystickArea.position;

            Vector2 direction = pointerPosition - joystickCenter;
            float magnitude = direction.magnitude;

            // Apply deadzone
            if (magnitude < deadzone * joystickSize)
            {
                RawInput = Vector2.zero;
                InputVector = Vector2.zero;
                joystickHandle.anchoredPosition = handleOriginalPosition;
                return;
            }

            // Normalize direction
            direction.Normalize();

            // Clamp handle position
            float handleDistance = Mathf.Min(magnitude, joystickSize * 0.5f * handleRange);
            Vector2 handlePosition = direction * handleDistance;

            // Update handle visual
            joystickHandle.anchoredPosition = handlePosition;

            // Calculate output
            float normalizedMagnitude = handleDistance / (joystickSize * 0.5f * handleRange);

            switch (outputMode)
            {
                case JoystickOutputMode.HorizontalOnly:
                    RawInput = new Vector2(direction.x, 0);
                    InputVector = normalizeOutput ? new Vector2(direction.x * normalizedMagnitude, 0) : RawInput;
                    break;
                case JoystickOutputMode.VerticalOnly:
                    RawInput = new Vector2(0, direction.y);
                    InputVector = normalizeOutput ? new Vector2(0, direction.y * normalizedMagnitude) : RawInput;
                    break;
                case JoystickOutputMode.Both:
                default:
                    RawInput = direction;
                    InputVector = normalizeOutput ? direction * normalizedMagnitude : RawInput;
                    break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != dragFingerId && dragFingerId != -1) return;

            ResetJoystick();
            OnJoystickUp?.Invoke(this);
        }

        private void ResetJoystick()
        {
            IsDragging = false;
            dragFingerId = -1;
            RawInput = Vector2.zero;
            InputVector = Vector2.zero;
            joystickHandle.anchoredPosition = handleOriginalPosition;

            if (snapToFinger)
            {
                joystickArea.gameObject.SetActive(false);
                IsActive = false;
            }
        }

        /// <summary>
        /// Show the joystick.
        /// </summary>
        public void Show()
        {
            IsActive = true;
            if (!IsDragging)
            {
                joystickArea.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the joystick.
        /// </summary>
        public void Hide()
        {
            IsActive = false;
            ResetJoystick();
        }

        /// <summary>
        /// Set joystick position on screen.
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            if (joystickArea != null)
            {
                joystickArea.position = position;
                UpdateJoystickRect();
            }
        }

        /// <summary>
        /// Set joystick size.
        /// </summary>
        public void SetSize(float size)
        {
            joystickSize = size;
            if (joystickArea != null)
            {
                joystickArea.sizeDelta = Vector2.one * joystickSize;
                UpdateJoystickRect();
            }
        }

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[VirtualJoystick] {message}");
        }

        private void DebugLogError(string message)
        {
            UnityEngine.Debug.LogError($"[VirtualJoystick] {message}");
        }
    }
}


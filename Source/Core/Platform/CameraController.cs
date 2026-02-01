using System;
using UnityEngine;

namespace ChronoCiv.Core.Platform
{
    /// <summary>
    /// Camera controller supporting both mouse and touch input.
    /// Handles pan, zoom, and rotation for both desktop and mobile platforms.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = Vector3.zero;

        [Header("Position")]
        [SerializeField] private float distance = 10f;
        [SerializeField] private float minDistance = 3f;
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float zoomSmoothTime = 0.1f;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float minVerticalAngle = 20f;
        [SerializeField] private float maxVerticalAngle = 80f;
        [SerializeField] private float currentVerticalAngle = 45f;
        [SerializeField] private float currentHorizontalAngle = 0f;

        [Header("Movement")]
        [SerializeField] private float panSpeed = 0.5f;
        [SerializeField] private float panSmoothTime = 0.1f;
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Vector3 currentVelocity;

        [Header("Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 minWorldX = new Vector2(-50, -50);
        [SerializeField] private Vector2 maxWorldX = new Vector2(50, 50);

        [Header("Input (Desktop)")]
        [SerializeField] private bool edgePan = true;
        [SerializeField] private float edgeThreshold = 20f;

        [Header("Input (Touch)")]
        [SerializeField] private bool twoFingerPan = true;
        [SerializeField] private float pinchZoomThreshold = 0.1f;
        [SerializeField] private float gestureSmoothing = 0.15f;
        [SerializeField] private bool enableHapticOnZoom = true;

        [Header("Input (Mac/Trackpad)")]
        [SerializeField] private bool enableTrackpadScroll = true;
        [SerializeField] private float trackpadScrollMultiplier = 1f;
        [SerializeField] private bool enableTrackpadPinch = true;
        [SerializeField] private float trackpadPinchMultiplier = 1f;
        [SerializeField] private bool enableMagicMouseScroll = true;
        [SerializeField] private float magicMouseScrollMultiplier = 1f;

        // Public Properties
        public float CurrentDistance => distance;
        public Vector3 CurrentPosition => transform.position;
        public float CurrentZoom => distance;

        // Events
        public event Action<float> OnZoomChanged;

        // Private State
        private Camera mainCamera;
        private float targetDistance;
        private float distanceVelocity;
        private Vector3 lastMousePosition;
        private bool isDragging = false;
        private int lastTouchCount = 0;

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

            mainCamera = GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = gameObject.AddComponent<Camera>();
            }
        }

        private void Start()
        {
            targetDistance = distance;

            if (target != null)
            {
                targetPosition = target.position + targetOffset;
            }
            else
            {
                targetPosition = Vector3.zero;
            }

            UpdateCameraPosition();
        }

        private void LateUpdate()
        {
            if (mainCamera == null) return;

            HandleInput();
            UpdateZoom();
            UpdatePosition();
            UpdateRotation();
        }

        #region Input Handling

        private void HandleInput()
        {
            var inputManager = InputManager.Instance;

            if (inputManager != null && inputManager.IsMobilePlatform)
            {
                HandleTouchInput(inputManager);
            }
            else if (inputManager != null && inputManager.IsMacPlatform)
            {
                HandleMacInput(inputManager);
            }
            else
            {
                HandleMouseInput();
            }
        }

        private void HandleMacInput(InputManager inputManager)
        {
            // Handle trackpad scroll for panning
            if (enableTrackpadScroll)
            {
                float scrollDelta = inputManager.ScrollWheelDelta;
                if (Mathf.Abs(scrollDelta) > 0.001f)
                {
                    // Trackpad scroll typically pans camera
                    Vector3 move = new Vector3(-scrollDelta * trackpadScrollMultiplier, 0, -scrollDelta * trackpadScrollMultiplier);
                    targetPosition += transform.TransformDirection(move) * distance * 0.5f;
                }
            }

            // Handle mouse scroll for zoom (or panning if preferred)
            float mouseScroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(mouseScroll) > 0.001f)
            {
                Zoom(-mouseScroll * zoomSpeed);
            }

            // Handle Option+Click for panning (Mac convention)
            if (Input.GetMouseButtonDown(0) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                Vector3 move = new Vector3(-delta.x, 0, -delta.y) * panSpeed * distance * 0.01f;
                targetPosition += transform.TransformDirection(move);
                lastMousePosition = Input.mousePosition;
            }
        }

        private void HandleMouseInput()
        {
            // Pan with middle mouse button or Alt+LeftClick
            if (Input.GetMouseButtonDown(2) || (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftAlt)))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(2) || (Input.GetMouseButtonUp(0) && Input.GetKey(KeyCode.LeftAlt)))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                Vector3 move = new Vector3(-delta.x, 0, -delta.y) * panSpeed * distance * 0.01f;
                targetPosition += transform.TransformDirection(move);
                lastMousePosition = Input.mousePosition;
            }

            // Edge panning
            if (edgePan && !isDragging)
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 move = Vector3.zero;

                if (mousePos.x < edgeThreshold) move.x = -1;
                else if (mousePos.x > Screen.width - edgeThreshold) move.x = 1;
                if (mousePos.y < edgeThreshold) move.z = -1;
                else if (mousePos.y > Screen.height - edgeThreshold) move.z = 1;

                if (move != Vector3.zero)
                {
                    targetPosition += move * panSpeed * distance * Time.deltaTime;
                }
            }

            // Zoom with scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                Zoom(-scroll * zoomSpeed);
            }
        }

        private void HandleTouchInput(InputManager inputManager)
        {
            int touchCount = inputManager.TouchCount;

            // Single touch - pan
            if (touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 delta = touch.deltaPosition;
                    Vector3 move = new Vector3(-delta.x, 0, -delta.y) * panSpeed * distance * 0.01f;
                    targetPosition += transform.TransformDirection(move);
                }
            }
            // Two finger - pinch zoom and pan
            else if (touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                // Pinch zoom
                float prevMagnitude = (touch0.position - touch0.deltaPosition - (touch1.position - touch1.deltaPosition)).magnitude;
                float currentMagnitude = (touch0.position - touch1.position).magnitude;
                float difference = currentMagnitude - prevMagnitude;

                if (Mathf.Abs(difference) > pinchZoomThreshold)
                {
                    Zoom(-difference * pinchZoomThreshold * 0.01f);
                }

                // Two finger pan
                if (twoFingerPan)
                {
                    Vector2 prevCenter = (touch0.position - touch0.deltaPosition + touch1.position - touch1.deltaPosition) / 2f;
                    Vector2 currentCenter = (touch0.position + touch1.position) / 2f;
                    Vector2 delta = currentCenter - prevCenter;

                    Vector3 move = new Vector3(-delta.x, 0, -delta.y) * panSpeed * distance * 0.01f;
                    targetPosition += transform.TransformDirection(move);
                }
            }

            lastTouchCount = touchCount;
        }

        #endregion

        #region Zoom

        /// <summary>
        /// Zoom the camera by the given amount.
        /// </summary>
        public void Zoom(float amount)
        {
            targetDistance = Mathf.Clamp(targetDistance + amount, minDistance, maxDistance);
        }

        /// <summary>
        /// Set the camera zoom level directly.
        /// </summary>
        public void SetZoom(float zoom)
        {
            targetDistance = Mathf.Clamp(zoom, minDistance, maxDistance);
            distance = targetDistance;
        }

        private void UpdateZoom()
        {
            distance = Mathf.SmoothDamp(distance, targetDistance, ref distanceVelocity, zoomSmoothTime);

            if (Mathf.Abs(distance - targetDistance) > 0.01f)
            {
                OnZoomChanged?.Invoke(distance);
            }
        }

        #endregion

        #region Position

        private void UpdatePosition()
        {
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minWorldX.x, maxWorldX.x);
                targetPosition.z = Mathf.Clamp(targetPosition.z, minWorldX.y, maxWorldX.y);
            }

            transform.position = Vector3.SmoothDamp(transform.position, GetCameraPosition(), ref currentVelocity, panSmoothTime);
        }

        private Vector3 GetCameraPosition()
        {
            float horizontalAngle = currentHorizontalAngle * Mathf.Deg2Rad;
            float verticalAngle = currentVerticalAngle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Sin(horizontalAngle) * Mathf.Cos(verticalAngle),
                Mathf.Sin(verticalAngle),
                Mathf.Cos(horizontalAngle) * Mathf.Cos(verticalAngle)
            ) * distance;

            return targetPosition + targetOffset + offset;
        }

        #endregion

        #region Rotation

        private void UpdateRotation()
        {
            // Follow target rotation if available
            if (target != null)
            {
                Vector3 direction = target.position - transform.position;
                if (direction != Vector3.zero)
                {
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    currentHorizontalAngle = Mathf.LerpAngle(currentHorizontalAngle, targetAngle, rotationSpeed * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Rotate the camera horizontally.
        /// </summary>
        public void Rotate(float angle)
        {
            currentHorizontalAngle += angle;
        }

        /// <summary>
        /// Set the camera to look at a specific point.
        /// </summary>
        public void LookAt(Vector3 point)
        {
            targetPosition = point - targetOffset;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Focus on a specific position in the world.
        /// </summary>
        public void FocusOn(Vector3 position, float zoom = -1)
        {
            targetPosition = position;

            if (zoom > 0)
            {
                targetDistance = Mathf.Clamp(zoom, minDistance, maxDistance);
            }
        }

        /// <summary>
        /// Focus on a game object.
        /// </summary>
        public void FocusOn(GameObject obj, float zoom = -1)
        {
            if (obj != null)
            {
                FocusOn(obj.transform.position, zoom);
            }
        }

        /// <summary>
        /// Reset camera to default position.
        /// </summary>
        public void ResetCamera()
        {
            targetPosition = Vector3.zero;
            targetDistance = 10f;
            currentVerticalAngle = 45f;
            currentHorizontalAngle = 0f;
        }

        /// <summary>
        /// Set the world bounds for camera movement.
        /// </summary>
        public void SetBounds(Vector2 min, Vector2 max)
        {
            useBounds = true;
            minWorldX = min;
            maxWorldX = max;
        }

        /// <summary>
        /// Disable world bounds.
        /// </summary>
        public void DisableBounds()
        {
            useBounds = false;
        }

        #endregion

        #region Screen Conversion

        /// <summary>
        /// Convert a screen position to a world position at ground level.
        /// </summary>
        public Vector3 ScreenToWorldPoint(Vector2 screenPosition, float groundY = 0f)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, groundY, 0));

            if (groundPlane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Convert a world position to screen position.
        /// </summary>
        public Vector2 WorldToScreenPoint(Vector3 worldPosition)
        {
            return mainCamera.WorldToScreenPoint(worldPosition);
        }

        #endregion
    }
}


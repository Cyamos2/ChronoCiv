using System.Collections.Generic;
using UnityEngine;

namespace ChronoCiv.GamePlay.Player
{
    /// <summary>
    /// Visual cursor indicator for iOS/mobile touch input.
    /// Displays a cursor that follows touch input with optional trail effects.
    /// </summary>
    public class PlayerCursor : MonoBehaviour
    {
        [Header("Cursor Appearance")]
        [SerializeField] private Sprite cursorSprite;
        [SerializeField] private Color cursorColor = new Color(1f, 1f, 1f, 0.9f);
        [SerializeField] private Vector2 cursorSize = new Vector2(32, 32);
        [SerializeField] private float cursorScale = 1f;

        [Header("Trail Effect")]
        [SerializeField] private bool showTrail = true;
        [SerializeField] private int trailLength = 5;
        [SerializeField] private float trailDuration = 0.3f;
        [SerializeField] private float trailFadeSpeed = 2f;
        [SerializeField] private Color trailColor = new Color(1f, 1f, 1f, 0.5f);

        [Header("Click Feedback")]
        [SerializeField] private bool showClickFeedback = true;
        [SerializeField] private float clickScaleMultiplier = 1.3f;
        [SerializeField] private float clickFeedbackDuration = 0.15f;
        [SerializeField] private Color clickColor = new Color(1f, 1f, 0f, 1f);

        [Header("Movement Indicator")]
        [SerializeField] private bool showMovementIndicator = true;
        [SerializeField] private Sprite movementMarkerSprite;
        [SerializeField] private Color markerColor = new Color(0f, 1f, 0f, 0.7f);
        [SerializeField] private float markerScale = 0.5f;

        [Header("Animations")]
        [SerializeField] private bool animateCursor = true;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.1f;

        // Public Properties
        public bool IsVisible { get; private set; } = true;
        public Vector2 CurrentPosition { get; private set; }

        // Private State
        private SpriteRenderer mainRenderer;
        private List<CursorTrailPoint> trailPoints = new();
        private List<GameObject> trailObjects = new();
        private GameObject movementMarker;
        private bool isClicking = false;
        private float clickTimer = 0f;
        private float currentScale = 1f;
        private float pulseTime = 0f;
        private Camera mainCamera;
        private RectTransform rectTransform;

        private class CursorTrailPoint
        {
            public Vector2 position;
            public float alpha;
            public float creationTime;
        }

        private void Awake()
        {
            mainRenderer = GetComponent<SpriteRenderer>();
            rectTransform = GetComponent<RectTransform>();

            if (mainRenderer == null)
            {
                mainRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;
            InitializeCursor();
        }

        private void InitializeCursor()
        {
            // Set up main cursor
            if (cursorSprite != null)
            {
                mainRenderer.sprite = cursorSprite;
            }
            else
            {
                // Create a simple arrow cursor if no sprite provided
                CreateDefaultCursorSprite();
            }

            mainRenderer.color = cursorColor;
            transform.localScale = Vector3.one * cursorScale;

            // Create movement marker
            if (showMovementIndicator)
            {
                CreateMovementMarker();
            }

            // Initialize trail pool
            if (showTrail)
            {
                InitializeTrailPool();
            }

            IsVisible = true;
            DebugLog("Cursor initialized");
        }

        private void CreateDefaultCursorSprite()
        {
            // Create a simple triangle cursor texture programmatically
            int size = 32;
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f));
                    if (dist < size / 2f)
                    {
                        // Create arrow shape
                        if (y < size * 0.7f)
                        {
                            colors[y * size + x] = Color.white;
                        }
                    }
                }
            }

            texture.SetPixels(colors);
            texture.Apply();
            cursorSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private void CreateMovementMarker()
        {
            movementMarker = new GameObject("MovementMarker");
            movementMarker.transform.SetParent(transform, false);

            var markerRenderer = movementMarker.AddComponent<SpriteRenderer>();
            markerRenderer.sprite = movementMarkerSprite ?? CreateDefaultMarkerSprite();
            markerRenderer.color = markerColor;
            movementMarker.transform.localScale = Vector3.one * markerScale;
            movementMarker.SetActive(false);
        }

        private Sprite CreateDefaultMarkerSprite()
        {
            // Create a ring/marker sprite
            int size = 16;
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f));
                    if (dist > size * 0.2f && dist < size * 0.45f)
                    {
                        colors[y * size + x] = Color.white;
                    }
                }
            }

            texture.SetPixels(colors);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private void InitializeTrailPool()
        {
            for (int i = 0; i < trailLength; i++)
            {
                GameObject trailObj = new GameObject($"Trail_{i}");
                trailObj.transform.SetParent(transform, false);
                var trailRenderer = trailObj.AddComponent<SpriteRenderer>();
                trailRenderer.sprite = mainRenderer.sprite;
                trailRenderer.color = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
                trailObj.transform.localScale = transform.localScale * 0.8f;
                trailObj.SetActive(false);
                trailObjects.Add(trailObj);
            }
        }

        private void Update()
        {
            UpdatePosition();
            UpdateTrail();
            UpdateClickFeedback();
            UpdateAnimations();
        }

        private void UpdatePosition()
        {
            var inputManager = InputManager.Instance;
            if (inputManager != null)
            {
                Vector2 targetPos = inputManager.CursorPosition;
                CurrentPosition = targetPos;

                // Smooth movement
                Vector3 worldPos = ConvertScreenToWorld(targetPos);
                Vector3 currentWorldPos = transform.position;
                transform.position = Vector3.Lerp(currentWorldPos, worldPos, Time.deltaTime * 20f);
            }
        }

        private Vector3 ConvertScreenToWorld(Vector2 screenPos)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                return Vector3.zero;
            }

            // For UI overlay cursor
            if (rectTransform != null)
            {
                Vector3 worldPos;
                if (RectTransformUtility.ScreenPointToWorldSpaceInRectangle(
                    rectTransform, screenPos, mainCamera, out worldPos))
                {
                    return worldPos;
                }
            }

            return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        }

        private void UpdateTrail()
        {
            if (!showTrail) return;

            var inputManager = InputManager.Instance;
            if (inputManager == null || !inputManager.IsMoving) return;

            // Add new trail point
            if (trailPoints.Count == 0 ||
                Vector2.Distance(trailPoints[trailPoints.Count - 1].position, CurrentPosition) > 10f)
            {
                trailPoints.Add(new CursorTrailPoint
                {
                    position = CurrentPosition,
                    alpha = 1f,
                    creationTime = Time.time
                });

                // Remove old points
                while (trailPoints.Count > trailLength)
                {
                    trailPoints.RemoveAt(0);
                }
            }

            // Update trail visuals
            for (int i = 0; i < trailObjects.Count && i < trailPoints.Count; i++)
            {
                var trailPoint = trailPoints[trailPoints.Count - 1 - i];
                var trailObj = trailObjects[i];

                if (!trailObj.activeSelf)
                {
                    trailObj.SetActive(true);
                }

                // Fade trail
                float age = Time.time - trailPoint.creationTime;
                float alpha = Mathf.Clamp01(1f - (age / trailDuration));
                trailPoint.alpha = alpha;

                // Update position and alpha
                Vector3 worldPos = ConvertScreenToWorld(trailPoint.position);
                trailObj.transform.position = worldPos;

                var renderer = trailObj.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = new Color(trailColor.r, trailColor.g, trailColor.b, alpha * trailColor.a);
                }

                // Scale down older trail points
                float scale = (1f - (float)i / trailLength) * cursorScale;
                trailObj.transform.localScale = Vector3.one * scale;
            }
        }

        private void UpdateClickFeedback()
        {
            if (!showClickFeedback) return;

            var inputManager = InputManager.Instance;
            if (inputManager == null) return;

            if (inputManager.IsPointerJustPressed)
            {
                isClicking = true;
                clickTimer = clickFeedbackDuration;
                mainRenderer.color = clickColor;
                currentScale = clickScaleMultiplier;

                // Trigger haptic feedback if enabled
                var iOSConfig = Resources.Load<iOSConfig>("iOSConfig");
                if (iOSConfig != null)
                {
                    iOSConfig.TriggerHaptic(iOSConfig.HapticOnInteract ?
                        iOSConfig.HapticFeedbackType.Light :
                        iOSConfig.HapticFeedbackType.Success);
                }
            }

            if (isClicking)
            {
                clickTimer -= Time.deltaTime;
                if (clickTimer <= 0)
                {
                    isClicking = false;
                    currentScale = 1f;
                    mainRenderer.color = cursorColor;
                }
            }
        }

        private void UpdateAnimations()
        {
            if (!animateCursor) return;

            pulseTime += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(pulseTime) * pulseAmount;

            transform.localScale = Vector3.one * cursorScale * currentScale * pulse;
        }

        /// <summary>
        /// Show or hide the cursor.
        /// </summary>
        public void SetVisible(bool visible)
        {
            IsVisible = visible;
            gameObject.SetActive(visible);

            // Also toggle trail
            foreach (var trailObj in trailObjects)
            {
                trailObj.SetActive(false);
            }
        }

        /// <summary>
        /// Set cursor position directly.
        /// </summary>
        public void SetPosition(Vector2 screenPosition)
        {
            CurrentPosition = screenPosition;
        }

        /// <summary>
        /// Show movement indicator at target position.
        /// </summary>
        public void ShowMovementIndicator(Vector3 worldPosition)
        {
            if (!showMovementIndicator || movementMarker == null) return;

            movementMarker.transform.position = worldPosition;
            movementMarker.SetActive(true);

            // Animate marker
            StartCoroutine(AnimateMarker(movementMarker));
        }

        private System.Collections.IEnumerator AnimateMarker(GameObject marker)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Vector3 startScale = marker.transform.localScale;
            Color startColor = marker.GetComponent<SpriteRenderer>().color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                marker.transform.localScale = Vector3.Lerp(startScale, startScale * 0.1f, t);

                var renderer = marker.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0), t);
                }

                yield return null;
            }

            marker.SetActive(false);
            marker.transform.localScale = startScale;
        }

        /// <summary>
        /// Initialize cursor with custom settings.
        /// </summary>
        public void Initialize(float trailDuration = 0.3f)
        {
            this.trailDuration = trailDuration;
            InitializeCursor();
        }

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[PlayerCursor] {message}");
        }
    }
}


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ChronoCiv.GamePlay.Player
{
    /// <summary>
    /// Player character controller with cursor-based movement and interaction.
    /// Supports both mouse (desktop) and touch (iOS) input methods.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Player Data")]
        [SerializeField] private string playerId;
        [SerializeField] private string displayName = "Leader";
        [SerializeField] private PlayerProfile profile;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float stoppingDistance = 0.5f;
        [SerializeField] private float pathUpdateInterval = 0.1f;

        [Header("Cursor Following")]
        [SerializeField] private bool followCursor = true;
        [SerializeField] private float cursorFollowDelay = 0.05f;
        [SerializeField] private bool showMovementIndicator = true;
        [SerializeField] private GameObject movementIndicatorPrefab;

        [Header("State")]
        [SerializeField] private PlayerState currentState = PlayerState.Idle;
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private bool isMoving = false;
        [SerializeField] private Direction facingDirection = Direction.Down;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer bodySprite;
        [SerializeField] private SpriteRenderer clothingSprite;
        [SerializeField] private SpriteAnimator spriteAnimator;

        [Header("Interaction")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask groundLayer;

        // Public Properties
        public string PlayerId => playerId;
        public string DisplayName => displayName;
        public PlayerProfile Profile => profile;
        public PlayerState CurrentState => currentState;
        public bool IsMoving => isMoving;
        public Direction FacingDirection => facingDirection;
        public Vector3 TargetPosition => targetPosition;
        public Vector3 CurrentPosition => transform.position;

        // Events
        public event Action<PlayerController, PlayerState> OnStateChanged;
        public event Action<PlayerController, Vector3> OnMoveStarted;
        public event Action<PlayerController, Vector3> OnTargetReached;
        public event Action<PlayerController, Vector3> OnMoveCancelled;
        public event Action<PlayerController, Vector3> OnInteract;
        public event Action<PlayerController, string> OnDialogueStarted;

        // Components
        private NavMeshAgent navAgent;
        private Rigidbody2D rb;
        private Collider2D col;
        private Camera mainCamera;

        // Pathfinding
        private Vector3 lastTargetPosition;
        private float lastPathUpdateTime;
        private Queue<Vector3> pathQueue = new();

        // Input
        private bool isInitialized = false;

        // Cursor visual
        private GameObject movementIndicator;

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

            navAgent = GetComponent<NavMeshAgent>();
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            spriteAnimator = GetComponent<SpriteAnimator>();

            if (navAgent != null)
            {
                navAgent.speed = moveSpeed;
                navAgent.stoppingDistance = stoppingDistance;
                navAgent.updateRotation = false;
                navAgent.updateUpAxis = false;
            }

            mainCamera = Camera.main;
        }

        protected virtual void Start()
        {
            InitializePlayer();
            SetupInputHandlers();
            CreateMovementIndicator();

            // Subscribe to era changes for visual updates
            var eraManager = EraManager.Instance;
            if (eraManager != null)
            {
                eraManager.OnEraChanged += OnEraChanged;
            }
        }

        protected virtual void OnDestroy()
        {
            var eraManager = EraManager.Instance;
            if (eraManager != null)
            {
                eraManager.OnEraChanged -= OnEraChanged;
            }

            UnsubscribeInputHandlers();
        }

        protected virtual void Update()
        {
            if (!isInitialized) return;

            UpdateMovement();
            UpdateFacingDirection();
            UpdateAnimationState();
        }

        private void InitializePlayer()
        {
            playerId = Guid.NewGuid().ToString();

            if (profile != null)
            {
                displayName = profile.DisplayName;
                moveSpeed = profile.MoveSpeed;
                runSpeed = profile.RunSpeed;
            }

            isInitialized = true;
            SetState(PlayerState.Idle);

            DebugLog($"Player initialized: {displayName}");
        }

        private void SetupInputHandlers()
        {
            var inputManager = InputManager.Instance;
            if (inputManager != null)
            {
                inputManager.OnPointerClick += OnPointerClick;
                inputManager.OnPointerMove += OnPointerMove;
                inputManager.OnPointerDown += OnPointerDown;
                inputManager.OnPointerUp += OnPointerUp;
                inputManager.OnPinchZoom += OnPinchZoom;
            }
        }

        private void UnsubscribeInputHandlers()
        {
            var inputManager = InputManager.Instance;
            if (inputManager != null)
            {
                inputManager.OnPointerClick -= OnPointerClick;
                inputManager.OnPointerMove -= OnPointerMove;
                inputManager.OnPointerDown -= OnPointerDown;
                inputManager.OnPointerUp -= OnPointerUp;
                inputManager.OnPinchZoom -= OnPinchZoom;
            }
        }

        private void CreateMovementIndicator()
        {
            if (showMovementIndicator && movementIndicatorPrefab != null)
            {
                movementIndicator = Instantiate(movementIndicatorPrefab, transform.position, Quaternion.identity);
                movementIndicator.SetActive(false);
            }
        }

        #region Input Handling

        private void OnPointerDown(Vector2 screenPosition)
        {
            // Store initial touch/click position
            lastTargetPosition = screenPosition;
        }

        private void OnPointerUp()
        {
            // Could handle drag release
        }

        private void OnPointerMove(Vector2 screenPosition)
        {
            if (InputManager.Instance.IsPointerOverUI()) return;

            // Update virtual cursor position on mobile
            if (InputManager.Instance.IsMobilePlatform)
            {
                InputManager.Instance.SetVirtualCursorPosition(screenPosition);
            }
        }

        private void OnPointerClick(Vector2 screenPosition)
        {
            if (InputManager.Instance.IsPointerOverUI())
            {
                DebugLog("Click ignored - over UI");
                return;
            }

            HandleClick(screenPosition);
        }

        private void HandleClick(Vector2 screenPosition)
        {
            // Convert screen position to world position
            Vector3 worldPos = GetWorldPosition(screenPosition);

            if (worldPos == Vector3.zero) return;

            // Check for interaction with NPCs or objects first
            if (TryGetInteractableAt(worldPos, out var interactable))
            {
                InteractWith(interactable);
                return;
            }

            // Otherwise, move to position
            MoveTo(worldPos);
        }

        private void OnPinchZoom(float delta)
        {
            // Handle camera zoom - could be delegated to camera controller
            var cameraController = Camera.main?.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.Zoom(delta * 0.1f);
            }
        }

        #endregion

        #region Movement

        /// <summary>
        /// Move player to a target position using NavMesh pathfinding.
        /// </summary>
        public void MoveTo(Vector3 position)
        {
            targetPosition = position;
            isMoving = true;

            if (navAgent != null && navAgent.enabled)
            {
                navAgent.SetDestination(position);
                navAgent.speed = moveSpeed;
            }

            ShowMovementIndicator(position);
            SetState(PlayerState.Moving);
            OnMoveStarted?.Invoke(this, position);

            DebugLog($"Moving to: {position}");
        }

        /// <summary>
        /// Move to a position with running speed.
        /// </summary>
        public void RunTo(Vector3 position)
        {
            targetPosition = position;
            isMoving = true;

            if (navAgent != null && navAgent.enabled)
            {
                navAgent.SetDestination(position);
                navAgent.speed = runSpeed;
            }

            ShowMovementIndicator(position);
            SetState(PlayerState.Moving);
            OnMoveStarted?.Invoke(this, position);
        }

        /// <summary>
        /// Stop moving immediately.
        /// </summary>
        public void StopMoving()
        {
            if (navAgent != null && navAgent.enabled)
            {
                navAgent.ResetPath();
            }
            isMoving = false;
            HideMovementIndicator();
            SetState(PlayerState.Idle);
        }

        /// <summary>
        /// Queue a movement path (for touch drag).
        /// </summary>
        public void QueueMovement(Vector3 position)
        {
            if (pathQueue.Count < 5) // Limit queue size
            {
                pathQueue.Enqueue(position);
            }
        }

        private void UpdateMovement()
        {
            if (!isMoving || navAgent == null || !navAgent.enabled) return;

            // Check if destination reached
            float remainingDistance = navAgent.remainingDistance;
            if (remainingDistance <= navAgent.stoppingDistance && navAgent.hasPath)
            {
                StopMoving();
                OnTargetReached?.Invoke(this, transform.position);
                DebugLog($"Reached target: {transform.position}");
                return;
            }

            // Update path periodically
            if (Time.time - lastPathUpdateTime > pathUpdateInterval)
            {
                lastPathUpdateTime = Time.time;
                // Path is automatically updated by NavMeshAgent
            }
        }

        private void UpdateFacingDirection()
        {
            if (!isMoving) return;

            Vector2 velocity = Vector2.zero;
            if (navAgent != null)
            {
                velocity = navAgent.velocity;
            }

            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                facingDirection = velocity.x > 0 ? Direction.Right : Direction.Left;
            }
            else if (velocity.y != 0)
            {
                facingDirection = velocity.y > 0 ? Direction.Up : Direction.Down;
            }
        }

        #endregion

        #region Interaction

        /// <summary>
        /// Attempt to interact with an object or NPC at the given position.
        /// </summary>
        public void InteractWith(string targetId)
        {
            // Look for target by ID
            // Could search NPCs, buildings, etc.
            DebugLog($"Interacting with: {targetId}");
            OnInteract?.Invoke(this, transform.position);
        }

        /// <summary>
        /// Interact with a game object.
        /// </summary>
        private void InteractWith(GameObject target)
        {
            // Try to get interactable component
            var interactable = target.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.OnInteract(this);
            }

            // Check for NPC
            var npc = target.GetComponent<NPCs.NPC>();
            if (npc != null)
            {
                npc.StartDialogue("player_greeting");
                OnDialogueStarted?.Invoke(this, npc.NPCId);
            }

            DebugLog($"Interacting with: {target.name}");
            OnInteract?.Invoke(this, target.transform.position);
        }

        /// <summary>
        /// Check if there's an interactable object at the given world position.
        /// </summary>
        private bool TryGetInteractableAt(Vector3 worldPos, out GameObject interactable)
        {
            interactable = null;

            // Check for NPCs
            var npcManager = NPCs.NPCManager.Instance;
            if (npcManager != null)
            {
                var npc = npcManager.GetClosestNPC(worldPos, interactionRange);
                if (npc != null && Vector3.Distance(worldPos, npc.transform.position) < interactionRange)
                {
                    interactable = npc.gameObject;
                    return true;
                }
            }

            // Check for buildings or other interactables
            Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, interactionRange, interactableLayer);
            if (colliders.Length > 0)
            {
                interactable = colliders[0].gameObject;
                return true;
            }

            return false;
        }

        #endregion

        #region Visuals

        private void UpdateAnimationState()
        {
            if (spriteAnimator == null) return;

            switch (currentState)
            {
                case PlayerState.Idle:
                    spriteAnimator.PlayAnimation("idle");
                    break;
                case PlayerState.Moving:
                    spriteAnimator.PlayAnimation("walk");
                    break;
                case PlayerState.Running:
                    spriteAnimator.PlayAnimation("run");
                    break;
                case PlayerState.Interacting:
                    spriteAnimator.PlayAnimation("interact");
                    break;
            }

            // Update facing direction
            spriteAnimator.SetDirection(facingDirection);
        }

        private void OnEraChanged(Era oldEra, Era newEra)
        {
            if (profile != null)
            {
                UpdateAppearance(newEra);
            }
        }

        /// <summary>
        /// Update player appearance based on current era.
        /// </summary>
        public void UpdateAppearance(Era era)
        {
            if (era == null || clothingSprite == null) return;

            // Would load appropriate era-specific clothing sprite
            // clothingSprite.sprite = GetEraClothingSprite(era.Id);
        }

        private void ShowMovementIndicator(Vector3 target)
        {
            if (movementIndicator != null && showMovementIndicator)
            {
                movementIndicator.transform.position = target;
                movementIndicator.SetActive(true);
            }
        }

        private void HideMovementIndicator()
        {
            if (movementIndicator != null)
            {
                movementIndicator.SetActive(false);
            }
        }

        #endregion

        #region Helpers

        private Vector3 GetWorldPosition(Vector2 screenPosition)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null) return Vector3.zero;

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);

            if (groundPlane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return Vector3.zero;
        }

        private void SetState(PlayerState newState)
        {
            if (currentState == newState) return;

            var previousState = currentState;
            currentState = newState;
            OnStateChanged?.Invoke(this, newState);
        }

        #endregion

        #region Save/Load

        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
            {
                ["playerId"] = playerId,
                ["displayName"] = displayName,
                ["profileId"] = profile?.Id ?? "",
                ["currentState"] = currentState.ToString(),
                ["position"] = new Dictionary<string, float>
                {
                    ["x"] = transform.position.x,
                    ["y"] = transform.position.y,
                    ["z"] = transform.position.z
                },
                ["facingDirection"] = facingDirection.ToString()
            };
        }

        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data.TryGetValue("position", out var posObj) && posObj is Dictionary<string, object> pos)
            {
                float x = Convert.ToSingle(pos["x"]);
                float y = Convert.ToSingle(pos["y"]);
                float z = Convert.ToSingle(pos["z"]);
                transform.position = new Vector3(x, y, z);
            }

            if (data.TryGetValue("facingDirection", out var dirObj))
            {
                if (Enum.TryParse<Direction>(dirObj.ToString(), out var dir))
                {
                    facingDirection = dir;
                }
            }
        }

        #endregion

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[Player:{displayName}] {message}");
        }
    }

    /// <summary>
    /// Player state enumeration.
    /// </summary>
    public enum PlayerState
    {
        Idle,
        Moving,
        Running,
        Interacting,
        Dialoguing,
        Menu
    }

    /// <summary>
    /// Direction enumeration for sprite facing.
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// Interface for interactable objects.
    /// </summary>
    public interface IInteractable
    {
        void OnInteract(PlayerController player);
    }
}


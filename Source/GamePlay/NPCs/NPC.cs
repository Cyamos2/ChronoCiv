using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ChronoCiv.GamePlay.NPCs
{
    /// <summary>
    /// Core NPC class representing a character in the game world.
    /// NPCs can move freely, perform tasks, and interact with the world.
    /// </summary>
    public class NPC : MonoBehaviour
    {
        [Header("NPC Data")]
        [SerializeField] private string npcId;
        [SerializeField] private string profileId;
        [SerializeField] private string displayName;

        [Header("State")]
        [SerializeField] private NPCState currentState = NPCState.Idle;
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Task currentTask;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private int currentEnergy = 100;
        [SerializeField] private float currentHappiness = 50f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float minMoveDistance = 0.5f;
        [SerializeField] private bool isMoving = false;

        [Header("AI")]
        [SerializeField] private float decisionInterval = 2f;
        [SerializeField] private float wanderRadius = 10f;
        [SerializeField] private float wanderTimer = 0f;
        [SerializeField] private Vector3 wanderCenter;

        [Header("Stats")]
        [SerializeField] private List<string> skills = new();
        [SerializeField] private int age = 18;
        [SerializeField] private int maxAge = 60;
        [SerializeField] private int experience = 0;
        [SerializeField] private int level = 1;

        [Header("Relationships")]
        [SerializeField] private float relationshipPlayer = 50f;
        [SerializeField] private Dictionary<string, float> relationshipNPCs = new();

        [Header("Visual")]
        [SerializeField] private SpriteRenderer bodySprite;
        [SerializeField] private SpriteRenderer clothingSprite;
        [SerializeField] private SpriteRenderer accessorySprite;
        [SerializeField] private Direction facingDirection = Direction.Down;

        [Header("Animation")]
        [SerializeField] private SpriteAnimator spriteAnimator;

        // Public Properties
        public string NPCId => npcId;
        public string ProfileId => profileId;
        public string DisplayName => displayName;
        public NPCState CurrentState => currentState;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => 100;
        public int CurrentEnergy => currentEnergy;
        public int MaxEnergy => 100;
        public float CurrentHappiness => currentHappiness;
        public float RelationshipPlayer => relationshipPlayer;
        public int Experience => experience;
        public int Level => level;
        public int Age => age;
        public bool IsAlive => currentHealth > 0;
        public bool IsMoving => isMoving;
        public Direction FacingDirection => facingDirection;
        public Task CurrentTask => currentTask;

        // Events
        public event Action<NPC, NPCState> OnStateChanged;
        public event Action<NPC, Vector3> OnTargetReached;
        public event Action<NPC, Task> OnTaskStarted;
        public event Action<NPC, Task> OnTaskCompleted;
        public event Action<NPC, Task> OnTaskCancelled;
        public event Action<NPC, int> OnHealthChanged;
        public event Action<NPC, float> OnHappinessChanged;
        public event Action<NPC> OnDied;
        public event Action<NPC, string> OnDialogueStarted;
        public event Action<NPC> OnDialogueEnded;

        // Components
        private NavMeshAgent navAgent;
        private Rigidbody2D rb;
        private Collider2D col;

        private float decisionTimer;
        private bool isInitialized = false;

        protected virtual void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            spriteAnimator = GetComponent<SpriteAnimator>();

            if (navAgent != null)
            {
                navAgent.speed = moveSpeed;
                navAgent.stoppingDistance = minMoveDistance;
            }
        }

        protected virtual void Start()
        {
            wanderCenter = transform.position;
            decisionTimer = UnityEngine.Random.Range(0, decisionInterval);
        }

        protected virtual void Update()
        {
            if (!isAlive) return;

            UpdateAI();
            UpdateMovement();
            UpdateState();
        }

        protected virtual void UpdateAI()
        {
            decisionTimer -= Time.deltaTime;

            if (decisionTimer <= 0)
            {
                MakeDecision();
                decisionTimer = decisionInterval;
            }

            // Update wander timer
            if (currentState == NPCState.Wandering)
            {
                wanderTimer -= Time.deltaTime;
                if (wanderTimer <= 0)
                {
                    SetRandomWanderTarget();
                    wanderTimer = UnityEngine.Random.Range(5f, 15f);
                }
            }
        }

        protected virtual void UpdateMovement()
        {
            if (navAgent != null && navAgent.enabled && navAgent.hasPath)
            {
                isMoving = navAgent.remainingDistance > navAgent.stoppingDistance;

                if (!isMoving && currentState == NPCState.Moving)
                {
                    OnTargetReached?.Invoke(this, transform.position);
                    SetState(NPCState.Idle);
                }
            }
            else if (isMoving && currentState == NPCState.Moving)
            {
                isMoving = false;
            }

            // Update facing direction based on movement
            if (isMoving)
            {
                Vector2 velocity = navAgent != null ? navAgent.velocity : Vector2.zero;
                UpdateFacingDirection(velocity);
            }
        }

        protected virtual void UpdateState()
        {
            // Update energy over time
            if (currentState == NPCState.Sleeping)
            {
                currentEnergy = Mathf.Min(currentEnergy + 10 * Time.deltaTime, 100);
                if (currentEnergy >= 100)
                {
                    SetState(NPCState.Idle);
                }
            }

            // Update happiness based on state
            if (currentState == NPCState.Working)
            {
                currentHappiness = Mathf.Max(0, currentHappiness - 2 * Time.deltaTime);
            }
            else if (currentState == NPCState.Socializing)
            {
                currentHappiness = Mathf.Min(100, currentHappiness + 5 * Time.deltaTime);
            }
        }

        protected virtual void MakeDecision()
        {
            if (currentTask != null)
            {
                return; // Busy with task
            }

            // Decision making based on needs
            if (currentEnergy < 20)
            {
                SetState(NPCState.Sleeping);
            }
            else if (currentHappiness < 30)
            {
                SetState(NPCState.Socializing);
                SetRandomWanderTarget();
            }
            else if (currentState == NPCState.Idle)
            {
                // Random chance to start working or wandering
                float roll = UnityEngine.Random.value;
                if (roll < 0.3f)
                {
                    SetState(NPCState.Wandering);
                    SetRandomWanderTarget();
                }
                else if (roll < 0.6f)
                {
                    // Look for work
                    var taskManager = TaskManager.Instance;
                    if (taskManager != null)
                    {
                        var task = taskManager.GetAvailableTaskForNPC(this);
                        if (task != null)
                        {
                            AssignTask(task);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialize the NPC with profile data.
        /// </summary>
        public void Initialize(NPCProfile profile)
        {
            profileId = profile.Id;
            displayName = profile.Name;
            npcId = Guid.NewGuid().ToString();

            // Set up skills
            skills = new List<string>(profile.Skills);

            // Set up age and lifespan
            age = UnityEngine.Random.Range(18, 30);
            maxAge = profile.Lifespan;

            // Set up visual appearance
            UpdateAppearance(profile);

            isInitialized = true;
        }

        /// <summary>
        /// Update the NPC's appearance based on era and profile.
        /// </summary>
        public void UpdateAppearance(NPCProfile profile)
        {
            if (profile == null) return;

            var eraManager = EraManager.Instance;
            string currentEraId = eraManager?.CurrentEra?.Id ?? "stone_age";

            if (profile.Clothing.TryGetValue(currentEraId, out var clothing))
            {
                // Update clothing sprite
                if (clothingSprite != null)
                {
                    // Would load sprite from resources
                }
            }
        }

        /// <summary>
        /// Set a target position to move to.
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

            SetState(NPCState.Moving);
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

            SetState(NPCState.Moving);
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
            SetState(NPCState.Idle);
        }

        /// <summary>
        /// Assign a task to this NPC.
        /// </summary>
        public void AssignTask(Task task)
        {
            if (currentTask == task) return;

            if (currentTask != null)
            {
                CancelCurrentTask();
            }

            currentTask = task;
            SetState(NPCState.Working);

            // Move to task location
            if (task.TargetPosition.HasValue)
            {
                MoveTo(task.TargetPosition.Value);
            }

            OnTaskStarted?.Invoke(this, task);
        }

        /// <summary>
        /// Cancel the current task.
        /// </summary>
        public void CancelCurrentTask()
        {
            if (currentTask != null)
            {
                var task = currentTask;
                currentTask = null;
                SetState(NPCState.Idle);
                StopMoving();
                OnTaskCancelled?.Invoke(this, task);
            }
        }

        /// <summary>
        /// Complete the current task.
        /// </summary>
        public void CompleteTask()
        {
            if (currentTask != null)
            {
                // Grant experience
                AddExperience(currentTask.ExperienceReward);

                var task = currentTask;
                currentTask = null;
                SetState(NPCState.Idle);
                OnTaskCompleted?.Invoke(this, task);
            }
        }

        /// <summary>
        /// Start a dialogue with this NPC.
        /// </summary>
        public void StartDialogue(string dialogueSetId)
        {
            SetState(NPCState.Socializing);
            OnDialogueStarted?.Invoke(this, dialogueSetId);
        }

        /// <summary>
        /// End the current dialogue.
        /// </summary>
        public void EndDialogue()
        {
            SetState(NPCState.Idle);
            OnDialogueEnded?.Invoke(this);
        }

        /// <summary>
        /// Get a dialogue line from this NPC.
        /// </summary>
        public DialogueLine GetDialogue(string dialogueSet, string trigger)
        {
            var dialogueManager = DialogueManager.Instance;
            if (dialogueManager != null)
            {
                return dialogueManager.GetDialogueForNPC(this, dialogueSet, trigger);
            }
            return null;
        }

        /// <summary>
        /// Modify the NPC's relationship with the player.
        /// </summary>
        public void ModifyPlayerRelationship(float amount)
        {
            relationshipPlayer = Mathf.Clamp(relationshipPlayer + amount, 0, 100);
            OnHappinessChanged?.Invoke(this, relationshipPlayer);
        }

        /// <summary>
        /// Modify health.
        /// </summary>
        public void ModifyHealth(int amount)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, MaxHealth);
            OnHealthChanged?.Invoke(this, currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Modify energy.
        /// </summary>
        public void ModifyEnergy(int amount)
        {
            currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, MaxEnergy);
        }

        /// <summary>
        /// Modify happiness.
        /// </summary>
        public void ModifyHappiness(float amount)
        {
            currentHappiness = Mathf.Clamp(currentHappiness + amount, 0, 100);
            OnHappinessChanged?.Invoke(this, currentHappiness);
        }

        /// <summary>
        /// Add experience and check for level up.
        /// </summary>
        public void AddExperience(int amount)
        {
            experience += amount;

            int newLevel = Mathf.FloorToInt(Mathf.Sqrt(experience / 100f)) + 1;
            if (newLevel > level)
            {
                LevelUp(newLevel - level);
            }
        }

        /// <summary>
        /// Level up the NPC.
        /// </summary>
        protected virtual void LevelUp(int levels)
        {
            level += levels;
            currentHealth = MaxHealth;
            currentEnergy = MaxEnergy;

            // Add random skill point
            if (skills.Count > 0)
            {
                var randomSkill = skills[UnityEngine.Random.Range(0, skills.Count)];
                // Would increase skill level
            }

            DebugLog($"{displayName} leveled up to {level}!");
        }

        /// <summary>
        /// Die and handle death effects.
        /// </summary>
        protected virtual void Die()
        {
            SetState(NPCState.Dead);
            isMoving = false;

            if (navAgent != null)
            {
                navAgent.enabled = false;
            }

            if (col != null)
            {
                col.enabled = false;
            }

            OnDied?.Invoke(this);

            DebugLog($"{displayName} has died at age {age}");
        }

        private void SetState(NPCState newState)
        {
            if (currentState == newState) return;

            var previousState = currentState;
            currentState = newState;
            OnStateChanged?.Invoke(this, newState);

            UpdateAnimationState();
        }

        private void SetRandomWanderTarget()
        {
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * wanderRadius;
            Vector3 target = wanderCenter + new Vector3(randomPoint.x, 0, randomPoint.y);

            // Clamp to world bounds
            target = ClampToWorldBounds(target);

            MoveTo(target);
        }

        private Vector3 ClampToWorldBounds(Vector3 position)
        {
            // Would use WorldManager to get bounds
            return position;
        }

        private void UpdateFacingDirection(Vector2 velocity)
        {
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                facingDirection = velocity.x > 0 ? Direction.Right : Direction.Left;
            }
            else if (velocity.y != 0)
            {
                facingDirection = velocity.y > 0 ? Direction.Up : Direction.Down;
            }

            UpdateAnimationDirection();
        }

        private void UpdateAnimationDirection()
        {
            if (spriteAnimator != null)
            {
                spriteAnimator.SetDirection(facingDirection);
            }
        }

        private void UpdateAnimationState()
        {
            if (spriteAnimator != null)
            {
                switch (currentState)
                {
                    case NPCState.Idle:
                        spriteAnimator.PlayAnimation("idle");
                        break;
                    case NPCState.Moving:
                        spriteAnimator.PlayAnimation("walk");
                        break;
                    case NPCState.Working:
                        spriteAnimator.PlayAnimation("work");
                        break;
                    case NPCState.Socializing:
                        spriteAnimator.PlayAnimation("talk");
                        break;
                    case NPCState.Sleeping:
                        spriteAnimator.PlayAnimation("sleep");
                        break;
                }
            }
        }

        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
            {
                ["npcId"] = npcId,
                ["profileId"] = profileId,
                ["displayName"] = displayName,
                ["currentState"] = currentState.ToString(),
                ["currentHealth"] = currentHealth,
                ["currentEnergy"] = currentEnergy,
                ["currentHappiness"] = currentHappiness,
                ["position"] = new Dictionary<string, float>
                {
                    ["x"] = transform.position.x,
                    ["y"] = transform.position.y,
                    ["z"] = transform.position.z
                },
                ["age"] = age,
                ["experience"] = experience,
                ["level"] = level,
                ["relationshipPlayer"] = relationshipPlayer,
                ["hasTask"] = currentTask != null,
                ["taskId"] = currentTask?.Id ?? ""
            };
        }

        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data.TryGetValue("currentHealth", out var health))
                currentHealth = Convert.ToInt32(health);
            if (data.TryGetValue("currentEnergy", out var energy))
                currentEnergy = Convert.ToInt32(energy);
            if (data.TryGetValue("currentHappiness", out var happiness))
                currentHappiness = Convert.ToSingle(happiness);
            if (data.TryGetValue("age", out var ageObj))
                age = Convert.ToInt32(ageObj);
            if (data.TryGetValue("experience", out var exp))
                experience = Convert.ToInt32(exp);
            if (data.TryGetValue("level", out var lvl))
                level = Convert.ToInt32(lvl);
            if (data.TryGetValue("relationshipPlayer", out var rel))
                relationshipPlayer = Convert.ToSingle(rel);
            if (data.TryGetValue("position", out var posObj) && posObj is Dictionary<string, object> pos)
            {
                float x = Convert.ToSingle(pos["x"]);
                float y = Convert.ToSingle(pos["y"]);
                float z = Convert.ToSingle(pos["z"]);
                transform.position = new Vector3(x, y, z);
            }
        }

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[NPC:{displayName}] {message}");
        }
    }

    public enum NPCState
    {
        Idle,
        Moving,
        Working,
        Socializing,
        Sleeping,
        Dead
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    [Serializable]
    public class NPCProfile
    {
        public string Id;
        public string Name;
        public string DisplayName;
        public string Gender;
        public string Era;
        public string Archetype;
        public string SpriteBase;
        public List<string> Animations;
        public List<int> SpriteSize;
        public string DefaultTask;
        public List<string> Skills;
        public string DialogueSet;
        public Dictionary<string, string> Clothing;
        public List<string> Equipment;
        public int SpawnWeight;
        public bool CanMigrate;
        public bool CanReproduce;
        public int Lifespan;
        public string Description;
    }
}


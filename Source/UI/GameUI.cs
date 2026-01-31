using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChronoCiv.UI
{
    /// <summary>
    /// Main game UI controller for the gameplay screen.
    /// Manages all in-game UI elements including HUD, panels, and notifications.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        [Header("Top HUD")]
        [SerializeField] private Text dateText;
        [SerializeField] private Text yearText;
        [SerializeField] private Text eraText;
        [SerializeField] private Text populationText;
        [SerializeField] private Text timeSpeedText;

        [Header("Resource Panel")]
        [SerializeField] private Transform resourcePanel;
        [SerializeField] private GameObject resourceItemPrefab;
        [SerializeField] private List<ResourceDisplay> resourceDisplays = new();

        [Header("Bottom Panel")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button speedButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;

        [Header("Action Panel")]
        [SerializeField] private Transform actionPanel;
        [SerializeField] private Button buildButton;
        [SerializeField] private Button researchButton;
        [SerializeField] private Button diplomacyButton;
        [SerializeField] private Button tasksButton;

        [Header("Notifications")]
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private float notificationDuration = 5f;

        [Header("Dialogue")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Text dialogueText;
        [SerializeField] private Button dialogueCloseButton;

        // Private state
        private List<GameObject> activeNotifications = new();
        private bool isPaused = false;
        private int timeSpeed = 1;
        private float notificationTimer;

        // Events
        public event Action OnPauseClicked;
        public event Action OnSpeedClicked;
        public event Action OnMenuClicked;
        public event Action OnSaveClicked;
        public event Action OnLoadClicked;
        public event Action OnBuildClicked;
        public event Action OnResearchClicked;
        public event Action OnDiplomacyClicked;
        public event Action OnTasksClicked;

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
        }

        private void Start()
        {
            InitializeUI();
            SetupButtonListeners();
            SubscribeToEvents();
            UpdateAllUI();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            UpdateTimeUI();
            UpdateResourceDisplays();
            UpdateNotificationTimer();
        }

        private void InitializeUI()
        {
            // Initialize resource displays
            InitializeResourceDisplays();

            // Set up dialogue panel
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        private void InitializeResourceDisplays()
        {
            var resourceManager = ResourceManager.Instance;
            if (resourceManager == null) return;

            // Create resource displays for common resources
            string[] commonResources = { "food", "wood", "stone", "gold", "iron" };

            foreach (var resourceId in commonResources)
            {
                CreateResourceDisplay(resourceId);
            }
        }

        private void CreateResourceDisplay(string resourceId)
        {
            if (resourceItemPrefab == null || resourcePanel == null) return;

            var resourceManager = ResourceManager.Instance;
            if (resourceManager == null) return;

            GameObject displayObj = Instantiate(resourceItemPrefab, resourcePanel);
            displayObj.name = $"ResourceDisplay_{resourceId}";

            var display = displayObj.GetComponent<ResourceDisplay>();
            if (display != null)
            {
                display.Initialize(resourceId);
                resourceDisplays.Add(display);
            }
        }

        private void SetupButtonListeners()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
            if (speedButton != null)
                speedButton.onClick.AddListener(OnSpeedButtonClicked);
            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenuButtonClicked);
            if (saveButton != null)
                saveButton.onClick.AddListener(OnSaveButtonClicked);
            if (loadButton != null)
                loadButton.onClick.AddListener(OnLoadButtonClicked);
            if (buildButton != null)
                buildButton.onClick.AddListener(OnBuildButtonClicked);
            if (researchButton != null)
                researchButton.onClick.AddListener(OnResearchButtonClicked);
            if (diplomacyButton != null)
                diplomacyButton.onClick.AddListener(OnDiplomacyButtonClicked);
            if (tasksButton != null)
                tasksButton.onClick.AddListener(OnTasksButtonClicked);
            if (dialogueCloseButton != null)
                dialogueCloseButton.onClick.AddListener(CloseDialogue);
        }

        private void SubscribeToEvents()
        {
            var eventBus = EventBus.Instance;
            if (eventBus != null)
            {
                eventBus.Subscribe<TimeManager.EraTransitionEvent>(OnEraTransition);
                eventBus.Subscribe<ResourceManager.ResourceChangedEvent>(OnResourceChanged);
                eventBus.Subscribe<NPCManager.NPCEvent>(OnNPCEvent);
                eventBus.Subscribe<EventManager.EventTriggeredEvent>(OnGameEvent);
            }
        }

        private void UnsubscribeFromEvents()
        {
            var eventBus = EventBus.Instance;
            if (eventBus != null)
            {
                eventBus.Unsubscribe<TimeManager.EraTransitionEvent>(OnEraTransition);
                eventBus.Unsubscribe<ResourceManager.ResourceChangedEvent>(OnResourceChanged);
                eventBus.Unsubscribe<NPCManager.NPCEvent>(OnNPCEvent);
                eventBus.Unsubscribe<EventManager.EventTriggeredEvent>(OnGameEvent);
            }
        }

        private void UpdateAllUI()
        {
            UpdateTimeUI();
            UpdateResourceDisplays();
        }

        private void UpdateTimeUI()
        {
            var timeManager = TimeManager.Instance;
            if (timeManager == null) return;

            // Update date display
            if (dateText != null)
                dateText.text = timeManager.GetFormattedDate();

            // Update year
            if (yearText != null)
                yearText.text = $"{timeManager.CurrentYear}";

            // Update era
            var eraManager = EraManager.Instance;
            if (eraText != null && eraManager != null)
                eraText.text = eraManager.CurrentEra?.DisplayName ?? "";

            // Update population
            var npcManager = NPCManager.Instance;
            if (populationText != null && npcManager != null)
                populationText.text = $"Pop: {npcManager.Population}";

            // Update time speed
            if (timeSpeedText != null)
                timeSpeedText.text = $"{timeSpeed}x";
        }

        private void UpdateResourceDisplays()
        {
            var resourceManager = ResourceManager.Instance;
            if (resourceManager == null) return;

            foreach (var display in resourceDisplays)
            {
                if (display != null)
                {
                    display.UpdateDisplay(resourceManager.GetResource(display.ResourceId));
                }
            }
        }

        private void UpdateNotificationTimer()
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0 && activeNotifications.Count > 0)
            {
                RemoveOldestNotification();
                notificationTimer = notificationDuration / activeNotifications.Count;
            }
        }

        private void OnPauseButtonClicked()
        {
            var timeManager = TimeManager.Instance;
            if (timeManager != null)
            {
                timeManager.TogglePause();
            }
            OnPauseClicked?.Invoke();
        }

        private void OnSpeedButtonClicked()
        {
            timeSpeed = timeSpeed >= 4 ? 1 : timeSpeed * 2;
            Time.timeScale = timeSpeed;
            UpdateTimeUI();
            OnSpeedClicked?.Invoke();
        }

        private void OnMenuButtonClicked()
        {
            OnMenuClicked?.Invoke();
            // Would open pause menu
        }

        private void OnSaveButtonClicked()
        {
            var saveSystem = SaveSystem.Instance;
            if (saveSystem != null)
            {
                saveSystem.QuickSave();
                ShowNotification("Game Saved!", NotificationType.Success);
            }
            OnSaveClicked?.Invoke();
        }

        private void OnLoadButtonClicked()
        {
            var saveSystem = SaveSystem.Instance;
            if (saveSystem != null)
            {
                saveSystem.QuickLoad();
            }
            OnLoadClicked?.Invoke();
        }

        private void OnBuildButtonClicked()
        {
            OnBuildClicked?.Invoke();
        }

        private void OnResearchButtonClicked()
        {
            OnResearchClicked?.Invoke();
        }

        private void OnDiplomacyButtonClicked()
        {
            OnDiplomacyClicked?.Invoke();
        }

        private void OnTasksButtonClicked()
        {
            OnTasksClicked?.Invoke();
        }

        private void OnEraTransition(TimeManager.EraTransitionEvent evt)
        {
            string message = $"A new era begins: {evt.ToEra?.DisplayName}!";
            ShowNotification(message, NotificationType.EraChange);
            UpdateTimeUI();
        }

        private void OnResourceChanged(ResourceManager.ResourceChangedEvent evt)
        {
            // Could show floating text for resource changes
        }

        private void OnNPCEvent(NPCManager.NPCEvent evt)
        {
            if (evt.Type == NPCManager.NPCEventType.Spawned)
            {
                ShowNotification($"{evt.NPC.DisplayName} joined the settlement", NotificationType.Info);
            }
        }

        private void OnGameEvent(EventManager.EventTriggeredEvent evt)
        {
            ShowNotification(evt.Event.DisplayName, NotificationType.Event);
        }

        /// <summary>
        /// Show a notification to the player.
        /// </summary>
        public void ShowNotification(string message, NotificationType type)
        {
            if (notificationContainer == null) return;

            GameObject notification = new GameObject("Notification");
            notification.transform.SetParent(notificationContainer, false);

            // Add visual components
            var rect = notification.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20 - (activeNotifications.Count * 50));
            rect.sizeDelta = new Vector2(300, 40);

            var image = notification.AddComponent<Image>();
            image.color = GetNotificationColor(type);

            var text = notification.AddComponent<Text>();
            text.text = message;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.rectTransform.offsetMin = new Vector2(10, 5);
            text.rectTransform.offsetMax = new Vector2(-10, -5);

            activeNotifications.Add(notification);

            // Auto-remove after duration
            Destroy(notification, notificationDuration * 2);
        }

        /// <summary>
        /// Show a dialogue with an NPC.
        /// </summary>
        public void ShowDialogue(string speakerName, string dialogue)
        {
            if (dialoguePanel == null) return;

            dialoguePanel.SetActive(true);

            if (dialogueText != null)
            {
                dialogueText.text = $"{speakerName}: {dialogue}";
            }
        }

        /// <summary>
        /// Close the current dialogue.
        /// </summary>
        public void CloseDialogue()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        private void RemoveOldestNotification()
        {
            if (activeNotifications.Count > 0)
            {
                var oldest = activeNotifications[0];
                activeNotifications.RemoveAt(0);
                Destroy(oldest);

                // Reposition remaining notifications
                RepositionNotifications();
            }
        }

        private void RepositionNotifications()
        {
            for (int i = 0; i < activeNotifications.Count; i++)
            {
                var rect = activeNotifications[i].GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(20, -20 - (i * 50));
                }
            }
        }

        private Color GetNotificationColor(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => new Color(0, 0.5f, 0, 0.8f),
                NotificationType.Warning => new Color(0.5f, 0.5f, 0, 0.8f),
                NotificationType.Error => new Color(0.5f, 0, 0, 0.8f),
                NotificationType.Info => new Color(0, 0, 0.5f, 0.8f),
                NotificationType.Event => new Color(0.5f, 0, 0.5f, 0.8f),
                NotificationType.EraChange => new Color(0.5f, 0.25f, 0, 0.8f),
                _ => new Color(0, 0, 0, 0.8f)
            };
        }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Event,
        EraChange
    }

    /// <summary>
    /// Component for displaying a resource in the UI.
    /// </summary>
    public class ResourceDisplay : MonoBehaviour
    {
        [SerializeField] private string resourceId;
        [SerializeField] private Text amountText;
        [SerializeField] private Image iconImage;

        public string ResourceId => resourceId;

        public void Initialize(string id)
        {
            resourceId = id;

            // Would load icon from resources
            // iconImage.sprite = Resources.Load<Sprite>($"Icons/{id}");
        }

        public void UpdateDisplay(float amount)
        {
            if (amountText != null)
            {
                amountText.text = amount >= 1000 
                    ? $"{amount / 1000:F1}K" 
                    : $"{amount:F0}";
            }
        }
    }
}


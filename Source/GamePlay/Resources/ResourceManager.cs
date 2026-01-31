using System.Collections.Generic;
using UnityEngine;

namespace ChronoCiv.GamePlay.Resources
{
    /// <summary>
    /// Represents a type of resource in the game.
    /// </summary>
    [System.Serializable]
    public class ResourceType
    {
        public string Id;
        public string Name;
        public string DisplayName;
        public string Icon;
        public string Category;
        public string Description;
        public bool Renewable;
        public float RenewalRate;
        public float DepletionRate;
        public float BaseValue;
        public float Weight;
        public string Color;
    }

    /// <summary>
    /// Manages all resources in the game including storage, production, and consumption.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        [Header("Resource Data")]
        [SerializeField] private TextAsset resourcesJsonFile;
        [SerializeField] private Dictionary<string, ResourceType> resourceTypes = new();

        [Header("Resource Storage")]
        [SerializeField] private Dictionary<string, float> resources = new();
        [SerializeField] private Dictionary<string, float> maxResources = new();
        [SerializeField] private Dictionary<string, float> resourceRates = new();

        [Header("Production/Consumption")]
        [SerializeField] private float productionMultiplier = 1f;
        [SerializeField] private float consumptionMultiplier = 1f;
        [SerializeField] private float updateInterval = 1f;

        [Header("Food Settings")]
        [SerializeField] private float foodPerPerson = 1f;
        [SerializeField] private float foodSurplusGrowth = 0.1f;

        // Events
        public event System.Action<ResourceChangedEvent> OnResourceChanged;
        public event System.Action<ResourceStorageFullEvent> OnResourceStorageFull;
        public event System.Action<ResourceDepletedEvent> OnResourceDepleted;
        public event System.Action<float> OnFoodChanged;

        public int ResourceCount => resources.Count;

        private float updateTimer;
        private bool isInitialized = false;

        public class ResourceChangedEvent
        {
            public string ResourceId;
            public float PreviousAmount;
            public float NewAmount;
            public float ChangeAmount;
            public string Source;
        }

        public class ResourceStorageFullEvent
        {
            public string ResourceId;
            public float Amount;
            public float Capacity;
        }

        public class ResourceDepletedEvent
        {
            public string ResourceId;
            public float Amount;
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
        }

        private void Start()
        {
            LoadResourceTypes();
            InitializeDefaultResources();

            var eventBus = EventBus.Instance;
            if (eventBus != null)
            {
                eventBus.Subscribe<SaveSystem.GameLoadedEvent>(OnGameLoaded);
            }
        }

        private void OnDestroy()
        {
            var eventBus = EventBus.Instance;
            if (eventBus != null)
            {
                eventBus.Unsubscribe<SaveSystem.GameLoadedEvent>(OnGameLoaded);
            }
        }

        private void Update()
        {
            if (!isInitialized) return;

            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0;
                UpdateResourceRates();
                ApplyResourceProduction();
                ApplyResourceConsumption();
            }
        }

        private void LoadResourceTypes()
        {
            if (resourcesJsonFile == null)
            {
                DebugLog("No resources JSON file assigned");
                return;
            }

            try
            {
                var wrapper = JsonUtility.FromJson<ResourceDataWrapper>(resourcesJsonFile.text);
                if (wrapper.resources != null)
                {
                    foreach (var type in wrapper.resources)
                    {
                        resourceTypes[type.Id] = type;
                    }
                }
                DebugLog($"Loaded {resourceTypes.Count} resource types");
            }
            catch (System.Exception e)
            {
                DebugLog($"Error loading resource types: {e.Message}");
            }
        }

        private void InitializeDefaultResources()
        {
            // Initialize with starting resources
            SetResource("food", 100);
            SetResource("wood", 50);
            SetResource("stone", 25);

            // Set max storage
            SetMaxResource("food", 1000);
            SetMaxResource("wood", 500);
            SetMaxResource("stone", 500);
            SetMaxResource("gold", 100);

            isInitialized = true;
        }

        private void UpdateResourceRates()
        {
            // Calculate production/consumption rates
            var npcManager = NPCManager.Instance;
            int population = npcManager?.Population ?? 0;

            // Food consumption
            float foodConsumption = population * foodPerPerson * consumptionMultiplier;
            resourceRates["food"] = -foodConsumption;

            // Other rates (would be calculated from buildings and tasks)
            resourceRates["wood"] = 0;
            resourceRates["stone"] = 0;
            resourceRates["gold"] = 0;
        }

        private void ApplyResourceProduction()
        {
            var weatherManager = WeatherManager.Instance;
            float temperature = weatherManager?.CurrentTemperature ?? 20f;

            foreach (var rate in resourceRates)
            {
                if (rate.Value > 0)
                {
                    float production = rate.Value * productionMultiplier * Time.deltaTime;
                    AddResource(rate.Key, production, "production");
                }
            }
        }

        private void ApplyResourceConsumption()
        {
            foreach (var rate in resourceRates)
            {
                if (rate.Value < 0)
                {
                    float consumption = Mathf.Abs(rate.Value) * consumptionMultiplier * Time.deltaTime;
                    ConsumeResource(rate.Key, consumption);
                }
            }
        }

        /// <summary>
        /// Get the current amount of a resource.
        /// </summary>
        public float GetResource(string resourceId)
        {
            return resources.TryGetValue(resourceId, out var amount) ? amount : 0;
        }

        /// <summary>
        /// Set the amount of a resource directly.
        /// </summary>
        public void SetResource(string resourceId, float amount)
        {
            float previous = GetResource(resourceId);
            resources[resourceId] = Mathf.Max(0, amount);

            OnResourceChanged?.Invoke(new ResourceChangedEvent
            {
                ResourceId = resourceId,
                PreviousAmount = previous,
                NewAmount = amount,
                ChangeAmount = amount - previous,
                Source = "set"
            });
        }

        /// <summary>
        /// Add resources to storage.
        /// </summary>
        public float AddResource(string resourceId, float amount, string source = "unknown")
        {
            if (amount <= 0) return 0;

            float previous = GetResource(resourceId);
            float max = GetMaxResource(resourceId);
            float overflow = 0;

            float newAmount = previous + amount;
            if (newAmount > max)
            {
                overflow = newAmount - max;
                newAmount = max;

                OnResourceStorageFull?.Invoke(new ResourceStorageFullEvent
                {
                    ResourceId = resourceId,
                    Amount = amount - overflow,
                    Capacity = max
                });
            }

            resources[resourceId] = newAmount;

            OnResourceChanged?.Invoke(new ResourceChangedEvent
            {
                ResourceId = resourceId,
                PreviousAmount = previous,
                NewAmount = newAmount,
                ChangeAmount = amount - overflow,
                Source = source
            });

            return overflow;
        }

        /// <summary>
        /// Consume resources from storage.
        /// </summary>
        public float ConsumeResource(string resourceId, float amount)
        {
            if (amount <= 0) return 0;

            float previous = GetResource(resourceId);
            float consumed = Mathf.Min(amount, previous);
            float newAmount = previous - consumed;

            resources[resourceId] = newAmount;

            if (consumed < amount)
            {
                OnResourceDepleted?.Invoke(new ResourceDepletedEvent
                {
                    ResourceId = resourceId,
                    Amount = amount - consumed
                });
            }

            OnResourceChanged?.Invoke(new ResourceChangedEvent
            {
                ResourceId = resourceId,
                PreviousAmount = previous,
                NewAmount = newAmount,
                ChangeAmount = -consumed,
                Source = "consumption"
            });

            return consumed;
        }

        /// <summary>
        /// Check if we have enough of a resource.
        /// </summary>
        public bool HasResource(string resourceId, float amount)
        {
            return GetResource(resourceId) >= amount;
        }

        /// <summary>
        /// Try to consume resources, returns true if successful.
        /// </summary>
        public bool TryConsumeResource(string resourceId, float amount)
        {
            if (HasResource(resourceId, amount))
            {
                ConsumeResource(resourceId, amount);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the maximum storage for a resource.
        /// </summary>
        public float GetMaxResource(string resourceId)
        {
            return maxResources.TryGetValue(resourceId, out var max) ? max : 0;
        }

        /// <summary>
        /// Set the maximum storage for a resource.
        /// </summary>
        public void SetMaxResource(string resourceId, float max)
        {
            maxResources[resourceId] = max;
        }

        /// <summary>
        /// Get a resource type by ID.
        /// </summary>
        public ResourceType GetResourceType(string resourceId)
        {
            return resourceTypes.TryGetValue(resourceId, out var type) ? type : null;
        }

        /// <summary>
        /// Get all resources in a category.
        /// </summary>
        public List<string> GetResourcesByCategory(string category)
        {
            var result = new List<string>();
            foreach (var type in resourceTypes.Values)
            {
                if (type.Category == category)
                {
                    result.Add(type.Id);
                }
            }
            return result;
        }

        /// <summary>
        /// Get total value of all resources.
        /// </summary>
        public float GetTotalResourceValue()
        {
            float total = 0;
            foreach (var resource in resources)
            {
                float value = GetResourceValue(resource.Key);
                total += resource.Value * value;
            }
            return total;
        }

        /// <summary>
        /// Get the base value of a resource.
        /// </summary>
        public float GetResourceValue(string resourceId)
        {
            var type = GetResourceType(resourceId);
            return type?.BaseValue ?? 1f;
        }

        /// <summary>
        /// Get food surplus or deficit.
        /// </summary>
        public float GetFoodBalance()
        {
            return resourceRates.TryGetValue("food", out var rate) ? rate : 0;
        }

        /// <summary>
        /// Check if food is running low.
        /// </summary>
        public bool IsFoodLow()
        {
            float food = GetResource("food");
            var npcManager = NPCManager.Instance;
            int population = npcManager?.Population ?? 1;
            return food < population * 3;
        }

        /// <summary>
        /// Get resource data for saving.
        /// </summary>
        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
            {
                ["resources"] = new Dictionary<string, float>(resources),
                ["maxResources"] = new Dictionary<string, float>(maxResources)
            };
        }

        private void OnGameLoaded(SaveSystem.GameLoadedEvent evt)
        {
            if (evt.SaveData.TryGetValue("ResourceManager", out var data))
            {
                if (data.TryGetValue("resources", out var resObj) && resObj is Dictionary<string, float> res)
                {
                    foreach (var kvp in res)
                    {
                        resources[kvp.Key] = kvp.Value;
                    }
                }

                if (data.TryGetValue("maxResources", out var maxObj) && maxObj is Dictionary<string, float> max)
                {
                    foreach (var kvp in max)
                    {
                        maxResources[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[ResourceManager] {message}");
        }

        [System.Serializable]
        private class ResourceDataWrapper
        {
            public System.Collections.Generic.List<ResourceType> resources;
        }
    }
}


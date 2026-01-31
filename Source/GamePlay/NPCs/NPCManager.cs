using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChronoCiv.GamePlay.NPCs
{
    /// <summary>
    /// Manages all NPCs in the game world including spawning, updating, and cleanup.
    /// </summary>
    public class NPCManager : MonoBehaviour
    {
        public static NPCManager Instance { get; private set; }

        [Header("NPC Data")]
        [SerializeField] private TextAsset npcProfilesJsonFile;
        [SerializeField] private GameObject npcPrefab;
        [SerializeField] private int initialPopulation = 10;
        [SerializeField] private int maxPopulation = 1000;
        [SerializeField] private float populationGrowthRate = 0.01f;

        [Header("Spawning")]
        [SerializeField] private Transform spawnArea;
        [SerializeField] private Vector2 spawnBounds = new Vector2(50, 50);
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private float populationCheckInterval = 10f;

        [Header("Current State")]
        [SerializeField] private List<NPC> allNPCs = new();
        [SerializeField] private List<NPC> livingNPCs = new();
        [SerializeField] private Dictionary<string, List<NPC>> npcsByArchetype = new();
        [SerializeField] private int population = 0;

        [Header("NPC Limit")]
        [SerializeField] private bool limitPopulation = true;

        // Events
        public event Action<NPC> OnNPCSpawned;
        public event Action<NPC> OnNPCDied;
        public event Action<int> OnPopulationChanged;
        public event Action<NPC, NPCState> OnNPCStateChanged;

        public int Population => population;
        public int MaxPopulation => maxPopulation;
        public int LivingNPCCount => livingNPCs.Count;

        private float lastSpawnTime;
        private float lastPopulationCheckTime;
        private Dictionary<string, NPCProfile> npcProfiles = new();
        private bool isInitialized = false;

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
            LoadNPCProfiles();
            SpawnInitialPopulation();

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
            UpdateNPCs();
            HandleSpawning();
            HandlePopulationGrowth();
        }

        private void UpdateNPCs()
        {
            // Update all living NPCs
            for (int i = livingNPCs.Count - 1; i >= 0; i--)
            {
                var npc = livingNPCs[i];
                if (npc != null && npc.IsAlive)
                {
                    // Would handle individual NPC updates here
                }
                else
                {
                    // NPC is dead or destroyed
                    livingNPCs.RemoveAt(i);
                }
            }
        }

        private void HandleSpawning()
        {
            if (Time.time - lastSpawnTime < spawnInterval) return;
            lastSpawnTime = Time.time;

            // Check if we should spawn a new NPC
            if (limitPopulation && population >= maxPopulation) return;

            // Random chance to spawn based on available resources
            var resourceManager = ResourceManager.Instance;
            if (resourceManager != null)
            {
                float food = resourceManager.GetResource("food");
                if (food < population * 2) return; // Not enough food to support new NPC
            }

            // Spawn chance based on population growth rate
            if (UnityEngine.Random.value < populationGrowthRate)
            {
                SpawnRandomNPC();
            }
        }

        private void HandlePopulationGrowth()
        {
            if (Time.time - lastPopulationCheckTime < populationCheckInterval) return;
            lastPopulationCheckTime = Time.time;

            // Handle population growth based on resources
            var resourceManager = ResourceManager.Instance;
            if (resourceManager == null) return;

            float food = resourceManager.GetResource("food");
            float foodPerPerson = population > 0 ? food / population : 0;

            // Population grows if there's excess food, shrinks if starving
            if (foodPerPerson > 3)
            {
                population = Mathf.Min(population + 1, maxPopulation);
                OnPopulationChanged?.Invoke(population);
            }
            else if (foodPerPerson < 1 && population > 0)
            {
                // Find an NPC to die from starvation
                var starvingNPC = FindStarvingNPC();
                if (starvingNPC != null)
                {
                    starvingNPC.ModifyHealth(-50);
                }
            }
        }

        private NPC FindStarvingNPC()
        {
            foreach (var npc in livingNPCs)
            {
                if (npc.CurrentHealth < 30)
                {
                    return npc;
                }
            }
            return livingNPCs.Count > 0 ? livingNPCs[0] : null;
        }

        private void LoadNPCProfiles()
        {
            if (npcProfilesJsonFile == null)
            {
                DebugLog("No NPC profiles JSON file assigned");
                return;
            }

            try
            {
                var wrapper = JsonUtility.FromJson<NPCProfilesWrapper>(npcProfilesJsonFile.text);
                if (wrapper.profiles != null)
                {
                    foreach (var profile in wrapper.profiles)
                    {
                        npcProfiles[profile.Id] = profile;
                    }
                }
                DebugLog($"Loaded {npcProfiles.Count} NPC profiles");
            }
            catch (Exception e)
            {
                DebugLog($"Error loading NPC profiles: {e.Message}");
            }
        }

        private void SpawnInitialPopulation()
        {
            for (int i = 0; i < initialPopulation; i++)
            {
                SpawnRandomNPC();
            }
            population = livingNPCs.Count;
            DebugLog($"Spawned initial population of {population}");
        }

        /// <summary>
        /// Spawn a random NPC based on weighted profiles.
        /// </summary>
        public NPC SpawnRandomNPC()
        {
            if (npcProfiles.Count == 0) return null;

            // Select profile based on weights
            var eraManager = EraManager.Instance;
            string currentEraId = eraManager?.CurrentEra?.Id ?? "stone_age";

            // Filter profiles for current era
            var validProfiles = new List<NPCProfile>();
            foreach (var profile in npcProfiles.Values)
            {
                if (profile.Era == currentEraId || IsEraUnlocked(profile.Era))
                {
                    validProfiles.Add(profile);
                }
            }

            if (validProfiles.Count == 0) return null;

            // Weighted random selection
            int totalWeight = 0;
            foreach (var profile in validProfiles)
            {
                totalWeight += profile.SpawnWeight;
            }

            int randomWeight = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;
            NPCProfile selectedProfile = validProfiles[0];

            foreach (var profile in validProfiles)
            {
                currentWeight += profile.SpawnWeight;
                if (randomWeight < currentWeight)
                {
                    selectedProfile = profile;
                    break;
                }
            }

            return SpawnNPC(selectedProfile);
        }

        /// <summary>
        /// Spawn an NPC from a specific profile.
        /// </summary>
        public NPC SpawnNPC(NPCProfile profile)
        {
            if (profile == null) return null;
            if (limitPopulation && population >= maxPopulation) return null;

            // Create NPC instance
            GameObject npcObj;
            if (npcPrefab != null)
            {
                npcObj = Instantiate(npcPrefab, GetRandomSpawnPosition(), Quaternion.identity);
            }
            else
            {
                npcObj = new GameObject($"NPC_{profile.Name}");
                npcObj.transform.position = GetRandomSpawnPosition();
            }

            var npc = npcObj.GetComponent<NPC>();
            if (npc == null)
            {
                npc = npcObj.AddComponent<NPC>();
            }

            // Initialize NPC
            npc.Initialize(profile);
            npc.OnStateChanged += OnNPCStateChangedHandler;
            npc.OnDied += OnNPCDiedHandler;

            // Track NPC
            allNPCs.Add(npc);
            livingNPCs.Add(npc);

            // Track by archetype
            if (!npcsByArchetype.ContainsKey(profile.Archetype))
            {
                npcsByArchetype[profile.Archetype] = new List<NPC>();
            }
            npcsByArchetype[profile.Archetype].Add(npc);

            population = livingNPCs.Count;
            OnPopulationChanged?.Invoke(population);
            OnNPCSpawned?.Invoke(npc);

            DebugLog($"Spawned NPC: {npc.DisplayName} ({profile.Archetype})");
            return npc;
        }

        /// <summary>
        /// Get a random spawn position within bounds.
        /// </summary>
        public Vector3 GetRandomSpawnPosition()
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnBounds;
            Vector3 spawnPos = spawnArea != null 
                ? spawnArea.position + new Vector3(offset.x, 0, offset.y)
                : new Vector3(offset.x, 0, offset.y);
            return spawnPos;
        }

        /// <summary>
        /// Get all NPCs of a specific archetype.
        /// </summary>
        public List<NPC> GetNPCsByArchetype(string archetype)
        {
            return npcsByArchetype.TryGetValue(archetype, out var npcs) 
                ? new List<NPC>(npcs) 
                : new List<NPC>();
        }

        /// <summary>
        /// Get the closest NPC to a position.
        /// </summary>
        public NPC GetClosestNPC(Vector3 position, float maxDistance = float.MaxValue)
        {
            NPC closest = null;
            float closestDistance = maxDistance;

            foreach (var npc in livingNPCs)
            {
                float distance = Vector3.Distance(position, npc.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = npc;
                }
            }

            return closest;
        }

        /// <summary>
        /// Get all NPCs within a radius of a position.
        /// </summary>
        public List<NPC> GetNPCsInRadius(Vector3 position, float radius)
        {
            var npcsInRadius = new List<NPC>();
            float radiusSquared = radius * radius;

            foreach (var npc in livingNPCs)
            {
                if ((npc.transform.position - position).sqrMagnitude <= radiusSquared)
                {
                    npcsInRadius.Add(npc);
                }
            }

            return npcsInRadius;
        }

        /// <summary>
        /// Get an NPC by ID.
        /// </summary>
        public NPC GetNPCById(string npcId)
        {
            foreach (var npc in allNPCs)
            {
                if (npc.NPCId == npcId)
                {
                    return npc;
                }
            }
            return null;
        }

        /// <summary>
        /// Remove an NPC from tracking (for dead NPCs after cleanup).
        /// </summary>
        public void RemoveNPC(NPC npc)
        {
            if (npc == null) return;

            allNPCs.Remove(npc);
            livingNPCs.Remove(npc);

            foreach (var archetypeList in npcsByArchetype.Values)
            {
                archetypeList.Remove(npc);
            }

            population = livingNPCs.Count;
            OnPopulationChanged?.Invoke(population);
        }

        private bool IsEraUnlocked(string eraId)
        {
            var eraManager = EraManager.Instance;
            if (eraManager == null) return false;

            int currentEraIndex = eraManager.CurrentEraIndex;
            var currentEra = eraManager.GetCurrentEra();
            if (currentEra == null) return false;

            int eraIndex = GetEraIndex(eraId);
            return eraIndex <= currentEraIndex;
        }

        private int GetEraIndex(string eraId)
        {
            // Map era IDs to indices
            var eraOrder = new Dictionary<string, int>
            {
                ["stone_age"] = 0,
                ["ancient"] = 1,
                ["classical"] = 2,
                ["medieval"] = 3,
                ["renaissance"] = 4,
                ["industrial"] = 5,
                ["modern"] = 6,
                ["future"] = 7
            };
            return eraOrder.TryGetValue(eraId, out var index) ? index : 0;
        }

        private void OnNPCStateChangedHandler(NPC npc, NPCState state)
        {
            OnNPCStateChanged?.Invoke(npc, state);
        }

        private void OnNPCDiedHandler(NPC npc)
        {
            OnNPCDied?.Invoke(npc);
            // Remove after a delay for death effects
            StartCoroutine(RemoveNPCAfterDelay(npc, 5f));
        }

        private System.Collections.IEnumerator RemoveNPCAfterDelay(NPC npc, float delay)
        {
            yield return new WaitForSeconds(delay);
            RemoveNPC(npc);
        }

        private void OnGameLoaded(SaveSystem.GameLoadedEvent evt)
        {
            if (evt.SaveData.TryGetValue("NPCManager", out var npcData))
            {
                if (npcData.TryGetValue("population", out var popObj))
                {
                    population = Convert.ToInt32(popObj);
                }
            }
        }

        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
            {
                ["population"] = population,
                ["livingNPCCount"] = livingNPCs.Count,
                ["maxPopulation"] = maxPopulation
            };
        }

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[NPCManager] {message}");
        }

        [Serializable]
        private class NPCProfilesWrapper
        {
            public List<NPCProfile> profiles;
        }
    }
}


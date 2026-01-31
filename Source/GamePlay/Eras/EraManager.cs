using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChronoCiv.GamePlay.Eras
{
    /// <summary>
    /// Represents a single era in the game's timeline.
    /// </summary>
    [Serializable]
    public class Era
    {
        public string Id;
        public string Name;
        public string DisplayName;
        public int StartYear;
        public int EndYear;
        public string Color;
        public string Icon;
        public string MusicTheme;
        public List<string> Buildings;
        public string ClothingStyle;
        public int TechnologyLevel;
        public string Description;

        /// <summary>
        /// Get the era duration in years.
        /// </summary>
        public int Duration => EndYear - StartYear;

        /// <summary>
        /// Check if a year falls within this era.
        /// </summary>
        public bool ContainsYear(int year)
        {
            return year >= StartYear && year < EndYear;
        }

        /// <summary>
        /// Get progress through this era (0-1).
        /// </summary>
        public float GetProgress(int currentYear)
        {
            if (currentYear < StartYear || currentYear >= EndYear)
                return currentYear < StartYear ? 0f : 1f;

            return (float)(currentYear - StartYear) / Duration;
        }

        /// <summary>
        /// Get the Color for this era.
        /// </summary>
        public Color GetColor()
        {
            if (ColorUtility.TryParseHtmlString(Color, out var color))
            {
                return color;
            }
            return Color.white;
        }
    }

    /// <summary>
    /// Era transition information.
    /// </summary>
    [Serializable]
    public class EraTransition
    {
        public string TriggerYear;
        public string VisualEffect;
        public string SoundEffect;
        public string Description;
    }

    /// <summary>
    /// Manages era progression and transitions.
    /// </summary>
    public class EraManager : MonoBehaviour
    {
        public static EraManager Instance { get; private set; }

        [Header("Era Data")]
        [SerializeField] private TextAsset erasJsonFile;
        [SerializeField] private List<Era> eras = new();
        [SerializeField] private Dictionary<string, Era> erasById = new();
        [SerializeField] private Dictionary<string, EraTransition> eraTransitions = new();

        [Header("Current State")]
        [SerializeField] private int currentEraIndex = 0;
        [SerializeField] private Era currentEra;

        [Header("Settings")]
        [SerializeField] private bool autoTransitions = true;

        // Events
        public event Action<Era> OnEraChanged;
        public event Action<Era, Era> OnEraTransition;
        public event Action<float> OnEraProgressChanged;

        public Era CurrentEra => currentEra;
        public int CurrentEraIndex => currentEraIndex;
        public int EraCount => eras.Count;

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
            LoadEraData();
            InitializeEraLookup();

            var timeManager = TimeManager.Instance;
            if (timeManager != null)
            {
                timeManager.OnEraChanged += OnEraChangedHandler;
                UpdateEraForYear(timeManager.CurrentYear);
            }
        }

        private void OnDestroy()
        {
            var timeManager = TimeManager.Instance;
            if (timeManager != null)
            {
                timeManager.OnEraChanged -= OnEraChangedHandler;
            }
        }

        private void LoadEraData()
        {
            if (erasJsonFile == null)
            {
                DebugLog("No era JSON file assigned");
                return;
            }

            try
            {
                var wrapper = JsonUtility.FromJson<EraDataWrapper>(erasJsonFile.text);
                eras = wrapper.eras ?? new List<Era>();

                // Load transitions
                if (wrapper.eraTransitions != null)
                {
                    foreach (var transition in wrapper.eraTransitions)
                    {
                        var key = transition.Key;
                        var value = transition.Value;
                        eraTransitions[key] = new EraTransition
                        {
                            TriggerYear = value.triggerYear.ToString(),
                            VisualEffect = value.visualEffect,
                            SoundEffect = value.soundEffect,
                            Description = value.description
                        };
                    }
                }

                DebugLog($"Loaded {eras.Count} eras");
            }
            catch (Exception e)
            {
                DebugLog($"Error loading era data: {e.Message}");
            }
        }

        private void InitializeEraLookup()
        {
            erasById.Clear();
            foreach (var era in eras)
            {
                erasById[era.Id] = era;
            }
        }

        private void OnEraChangedHandler(int newEraIndex)
        {
            var previousEra = currentEra;
            currentEraIndex = newEraIndex;
            currentEra = eras.Count > currentEraIndex ? eras[currentEraIndex] : null;

            OnEraChanged?.Invoke(currentEra);

            if (previousEra != null && currentEra != null)
            {
                OnEraTransition?.Invoke(previousEra, currentEra);
            }
        }

        private void UpdateEraForYear(int year)
        {
            int eraIndex = GetEraIndexForYear(year);
            if (eraIndex != currentEraIndex)
            {
                OnEraChangedHandler(eraIndex);
            }
        }

        /// <summary>
        /// Get the era index for a given year.
        /// </summary>
        public int GetEraIndexForYear(int year)
        {
            for (int i = 0; i < eras.Count; i++)
            {
                if (eras[i].ContainsYear(year))
                {
                    return i;
                }
            }
            // If year is beyond all eras, return last era
            return Mathf.Max(0, eras.Count - 1);
        }

        /// <summary>
        /// Get the era for a given year.
        /// </summary>
        public Era GetEraForYear(int year)
        {
            int index = GetEraIndexForYear(year);
            return eras.Count > index ? eras[index] : null;
        }

        /// <summary>
        /// Get an era by its ID.
        /// </summary>
        public Era GetEra(string eraId)
        {
            return erasById.TryGetValue(eraId, out var era) ? era : null;
        }

        /// <summary>
        /// Get the current era.
        /// </summary>
        public Era GetCurrentEra()
        {
            return currentEra;
        }

        /// <summary>
        /// Get the next era, if any.
        /// </summary>
        public Era GetNextEra()
        {
            if (currentEraIndex < eras.Count - 1)
            {
                return eras[currentEraIndex + 1];
            }
            return null;
        }

        /// <summary>
        /// Get progress through the current era (0-1).
        /// </summary>
        public float GetEraProgress()
        {
            if (currentEra == null) return 0f;

            var timeManager = TimeManager.Instance;
            int year = timeManager?.CurrentYear ?? 0;

            return currentEra.GetProgress(year);
        }

        /// <summary>
        /// Get the transition info for the next era change.
        /// </summary>
        public EraTransition GetNextTransition()
        {
            if (currentEraIndex >= eras.Count - 1) return null;

            string transitionKey = $"{currentEra.Id}_to_{eras[currentEraIndex + 1].Id}";
            return eraTransitions.TryGetValue(transitionKey, out var transition) ? transition : null;
        }

        /// <summary>
        /// Check if a building is available in the current era.
        /// </summary>
        public bool IsBuildingAvailable(string buildingId)
        {
            if (currentEra == null) return false;
            return currentEra.Buildings?.Contains(buildingId) ?? false;
        }

        /// <summary>
        /// Get all available buildings for the current era.
        /// </summary>
        public List<string> GetAvailableBuildings()
        {
            return currentEra?.Buildings ?? new List<string>();
        }

        /// <summary>
        /// Get the minimum technology level for the current era.
        /// </summary>
        public int GetMinTechLevel()
        {
            if (currentEra == null) return 1;
            return currentEra.TechnologyLevel;
        }

        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
            {
                ["currentEraIndex"] = currentEraIndex,
                ["currentEraName"] = currentEra?.Id ?? "",
                ["currentEraDisplayName"] = currentEra?.DisplayName ?? ""
            };
        }

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[EraManager] {message}");
        }

        [Serializable]
        private class EraDataWrapper
        {
            public List<Era> eras;
            public Dictionary<string, EraTransitionData> eraTransitions;
        }

        [Serializable]
        private class EraTransitionData
        {
            public int triggerYear;
            public string visualEffect;
            public string soundEffect;
            public string description;
        }
    }
}


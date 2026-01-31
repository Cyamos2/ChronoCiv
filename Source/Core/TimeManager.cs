using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChronoCiv.Core
{
    /// <summary>
    /// Manages game time progression including day/night cycles, years, and time-based events.
    /// Time advances 100 years per day (in-game), even while idle.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [Header("Time Settings")]
        [SerializeField] private float dayDuration = 60f; // Seconds per in-game day
        [SerializeField] private int yearsPerDay = 100; // Years that pass each day
        [SerializeField] private int startingYear = -5000; // Stone Age
        [SerializeField] private bool isPaused = false;

        [Header("Current State")]
        [SerializeField] private int currentYear;
        [SerializeField] private float currentDay;
        [SerializeField] private float currentTimeOfDay; // 0-1 within a day
        [SerializeField] private int currentEraIndex;

        [Header("Day/Night Cycle")]
        [SerializeField] private float dayProgress; // 0-1 for current day
        [SerializeField] private DayPhase currentDayPhase;

        public int CurrentYear => currentYear;
        public int CurrentDay => Mathf.FloorToInt(currentDay);
        public float TimeOfDay => currentTimeOfDay;
        public DayPhase CurrentDayPhase => currentDayPhase;
        public bool IsPaused => isPaused;

        public float DayProgress => dayProgress;
        public float DayDuration => dayDuration;
        public int YearsPerDay => yearsPerDay;

        // Events
        public event Action<int> OnYearChanged;
        public event Action<int> OnDayChanged;
        public event Action<int> OnEraChanged;
        public event Action<float> OnTimeOfDayChanged;
        public event Action<DayPhase> OnDayPhaseChanged;
        public event Action OnDayComplete;
        public event Action<bool> OnPausedChanged;

        // References
        private EventBus eventBus;
        private EraManager eraManager;

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

            Initialize();
        }

        private void Initialize()
        {
            currentYear = startingYear;
            currentDay = 0;
            currentTimeOfDay = 0;
            currentEraIndex = 0;
            dayProgress = 0;
            currentDayPhase = DayPhase.Dawn;
        }

        private void Start()
        {
            eventBus = EventBus.Instance;
            eraManager = EraManager.Instance;

            if (eventBus != null)
            {
                eventBus.Subscribe<SaveSystem.GameLoadedEvent>(OnGameLoaded);
            }

            UpdateEraForYear();
        }

        private void OnDestroy()
        {
            if (eventBus != null)
            {
                eventBus.Unsubscribe<SaveSystem.GameLoadedEvent>(OnGameLoaded);
            }
        }

        private void Update()
        {
            if (isPaused) return;

            AdvanceTime(Time.deltaTime);
        }

        private void AdvanceTime(float deltaTime)
        {
            // Advance day progress
            dayProgress += deltaTime / dayDuration;

            // Update time of day (0-1)
            currentTimeOfDay = dayProgress;
            OnTimeOfDayChanged?.Invoke(currentTimeOfDay);

            // Update day phase
            UpdateDayPhase();

            // Check if day is complete
            if (dayProgress >= 1f)
            {
                CompleteDay();
            }
        }

        private void UpdateDayPhase()
        {
            DayPhase newPhase = currentDayPhase;

            if (currentTimeOfDay < 0.1f)
                newPhase = DayPhase.Midnight;
            else if (currentTimeOfDay < 0.2f)
                newPhase = DayPhase.WeeHours;
            else if (currentTimeOfDay < 0.3f)
                newPhase = DayPhase.Dawn;
            else if (currentTimeOfDay < 0.4f)
                newPhase = DayPhase.Morning;
            else if (currentTimeOfDay < 0.6f)
                newPhase = DayPhase.Noon;
            else if (currentTimeOfDay < 0.75f)
                newPhase = DayPhase.Afternoon;
            else if (currentTimeOfDay < 0.85f)
                newPhase = DayPhase.Dusk;
            else if (currentTimeOfDay < 0.95f)
                newPhase = DayPhase.Evening;
            else
                newPhase = DayPhase.Midnight;

            if (newPhase != currentDayPhase)
            {
                currentDayPhase = newPhase;
                OnDayPhaseChanged?.Invoke(currentDayPhase);
            }
        }

        private void CompleteDay()
        {
            dayProgress = 0;
            currentDay++;
            currentYear += yearsPerDay;

            OnDayChanged?.Invoke(CurrentDay);
            OnYearChanged?.Invoke(currentYear);

            CheckEraTransition();
            OnDayComplete?.Invoke();
        }

        private void CheckEraTransition()
        {
            if (eraManager == null) return;

            int newEraIndex = eraManager.GetEraIndexForYear(currentYear);

            if (newEraIndex != currentEraIndex && newEraIndex > currentEraIndex)
            {
                int previousEraIndex = currentEraIndex;
                currentEraIndex = newEraIndex;
                OnEraChanged?.Invoke(currentEraIndex);

                // Publish era transition event
                eventBus?.Publish(new EraTransitionEvent
                {
                    FromEra = eraManager.GetEra(previousEraIndex),
                    ToEra = eraManager.GetEra(currentEraIndex),
                    Year = currentYear
                });
            }
        }

        private void UpdateEraForYear()
        {
            if (eraManager != null)
            {
                currentEraIndex = eraManager.GetEraIndexForYear(currentYear);
            }
        }

        public void SetPaused(bool paused)
        {
            if (isPaused != paused)
            {
                isPaused = paused;
                OnPausedChanged?.Invoke(isPaused);
            }
        }

        public void TogglePause()
        {
            SetPaused(!isPaused);
        }

        public void SetYear(int year)
        {
            currentYear = year;
            OnYearChanged?.Invoke(currentYear);
            UpdateEraForYear();
        }

        public void SetTimeScale(float scale)
        {
            // For future use - allow speeding up/slowing down time
            Time.timeScale = scale;
        }

        public string GetFormattedDate()
        {
            string eraName = eraManager?.GetCurrentEra()?.DisplayName ?? "Unknown";
            string yearPrefix = currentYear < 0 ? $"{Mathf.Abs(currentYear)} BCE" : $"{currentYear} CE";
            return $"{eraName}, Year {yearPrefix}";
        }

        public string GetDayPhaseName()
        {
            return currentDayPhase switch
            {
                DayPhase.Midnight => "Midnight",
                DayPhase.WeeHours => "Wee Hours",
                DayPhase.Dawn => "Dawn",
                DayPhase.Morning => "Morning",
                DayPhase.Noon => "Noon",
                DayPhase.Afternoon => "Afternoon",
                DayPhase.Dusk => "Dusk",
                DayPhase.Evening => "Evening",
                _ => "Unknown"
            };
        }

        public float GetDayProgressPercent()
        {
            return dayProgress * 100f;
        }

        private void OnGameLoaded(SaveSystem.GameLoadedEvent evt)
        {
            if (evt.SaveData.TryGetValue("TimeManager", out var timeData))
            {
                currentYear = (int)timeData["currentYear"];
                currentDay = (int)timeData["currentDay"];
                currentTimeOfDay = (float)timeData["currentTimeOfDay"];
                currentEraIndex = (int)timeData["currentEraIndex"];
                isPaused = (bool)timeData["isPaused"];

                dayProgress = currentTimeOfDay;
                UpdateDayPhase();
            }
        }

        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
            {
                ["currentYear"] = currentYear,
                ["currentDay"] = currentDay,
                ["currentTimeOfDay"] = currentTimeOfDay,
                ["currentEraIndex"] = currentEraIndex,
                ["isPaused"] = isPaused
            };
        }
    }

    public enum DayPhase
    {
        Midnight,
        WeeHours,
        Dawn,
        Morning,
        Noon,
        Afternoon,
        Dusk,
        Evening
    }

    public class EraTransitionEvent
    {
        public Era FromEra;
        public Era ToEra;
        public int Year;
    }
}


using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChronoCiv.GamePlay.Weather
{
    /// <summary>
    /// Manages weather states and transitions for the game world.
    /// Handles biomes, precipitation, temperature, and visual effects.
    /// </summary>
    public class WeatherManager : MonoBehaviour
    {
        public static WeatherManager Instance { get; private set; }

        [Header("Weather Data")]
        [SerializeField] private TextAsset weatherJsonFile;
        [SerializeField] private TextAsset biomesJsonFile;
        [SerializeField] private TextAsset seasonsJsonFile;

        [Header("Current State")]
        [SerializeField] private WeatherState currentWeather;
        [SerializeField] private Biome currentBiome;
        [SerializeField] private Season currentSeason;
        [SerializeField] private float currentTemperature;
        [SerializeField] private float currentHumidity;
        [SerializeField] private float currentWindSpeed;

        [Header("World Generation")]
        [SerializeField] private int worldWidth = 100;
        [SerializeField] private int worldHeight = 100;
        [SerializeField] private float biomeScale = 0.1f;
        [SerializeField] private float temperatureGradient = 0.5f;

        [Header("Visual")]
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private ParticleSystem rainParticleSystem;
        [SerializeField] private ParticleSystem snowParticleSystem;
        [SerializeField] private Light directionalLight;

        [Header("Settings")]
        [SerializeField] private bool weatherEnabled = true;
        [SerializeField] private float weatherTransitionDuration = 5f;

        // Events
        public event Action<WeatherState> OnWeatherChanged;
        public event Action<WeatherState, WeatherState> OnWeatherTransition;
        public event Action<Biome> OnBiomeChanged;
        public event Action<Season> OnSeasonChanged;
        public event Action<float> OnTemperatureChanged;
        public event Action<float> OnWindSpeedChanged;

        public WeatherState CurrentWeather => currentWeather;
        public Biome CurrentBiome => currentBiome;
        public Season CurrentSeason => currentSeason;
        public float CurrentTemperature => currentTemperature;
        public float CurrentHumidity => currentHumidity;
        public float CurrentWindSpeed => currentWindSpeed;

        private Dictionary<string, WeatherState> weatherStates;
        private Dictionary<string, Biome> biomes;
        private Dictionary<string, Season> seasons;
        private Dictionary<Vector2Int, Biome> worldBiomes;
        private float[,] temperatureMap;
        private float[,] moistureMap;
        private float weatherTimer;
        private float weatherTransitionProgress;
        private WeatherState targetWeather;
        private bool isTransitioning = false;

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

            weatherStates = new Dictionary<string, WeatherState>();
            biomes = new Dictionary<string, Biome>();
            seasons = new Dictionary<string, Season>();
            worldBiomes = new Dictionary<Vector2Int, Biome>();
        }

        private void Start()
        {
            LoadWeatherData();
            LoadBiomeData();
            LoadSeasonData();

            GenerateWorldBiomes();
            InitializeWeather();
        }

        private void Update()
        {
            if (!weatherEnabled) return;

            UpdateWeather();
            UpdateEnvironmentEffects();
        }

        private void UpdateWeather()
        {
            // Handle weather transition
            if (isTransitioning)
            {
                weatherTransitionProgress += Time.deltaTime / weatherTransitionDuration;
                if (weatherTransitionProgress >= 1f)
                {
                    weatherTransitionProgress = 0;
                    isTransitioning = false;
                    currentWeather = targetWeather;
                }
            }
            else
            {
                // Check for weather change
                weatherTimer -= Time.deltaTime;
                if (weatherTimer <= 0)
                {
                    DetermineNextWeather();
                    weatherTimer = UnityEngine.Random.Range(
                        currentWeather.Duration.Min,
                        currentWeather.Duration.Max
                    );
                }
            }

            // Update wind
            currentWindSpeed = Mathf.Lerp(
                currentWindSpeed,
                isTransitioning && targetWeather != null ? targetWeather.WindSpeed : currentWeather.WindSpeed,
                Time.deltaTime * 0.5f
            );
            OnWindSpeedChanged?.Invoke(currentWindSpeed);
        }

        private void UpdateEnvironmentEffects()
        {
            // Update lighting based on weather
            if (directionalLight != null && currentWeather != null)
            {
                Color targetColor = currentWeather.LightColor;
                directionalLight.color = Color.Lerp(
                    directionalLight.color,
                    targetColor,
                    Time.deltaTime * 0.5f
                );

                float intensity = 1f - currentWeather.FogDensity;
                directionalLight.intensity = Mathf.Lerp(
                    directionalLight.intensity,
                    intensity,
                    Time.deltaTime * 0.5f
                );
            }

            // Update particle effects
            UpdateWeatherParticles();
        }

        private void UpdateWeatherParticles()
        {
            if (rainParticleSystem != null)
            {
                var emission = rainParticleSystem.emission;
                emission.rateOverTime = currentWeather?.Precipitation ?? 0;
            }

            if (snowParticleSystem != null)
            {
                var emission = snowParticleSystem.emission;
                emission.rateOverTime = currentWeather?.Id.Contains("snow") == true ? currentWeather.Precipitation * 0.5f : 0;
            }
        }

        private void DetermineNextWeather()
        {
            if (currentBiome == null || currentSeason == null) return;

            // Get possible weather for current biome and season
            var possibleWeather = new List<WeatherState>();

            foreach (var weatherId in currentBiome.PrimaryWeather)
            {
                if (weatherStates.TryGetValue(weatherId, out var weather))
                {
                    possibleWeather.Add(weather);
                }
            }

            foreach (var weatherId in currentBiome.SecondaryWeather)
            {
                if (weatherStates.TryGetValue(weatherId, out var weather))
                {
                    possibleWeather.Add(weather);
                }
            }

            if (possibleWeather.Count == 0) return;

            // Weighted random selection
            var weatherChances = new Dictionary<WeatherState, float>();
            float totalWeight = 0;

            foreach (var weather in possibleWeather)
            {
                float weight = 1f;
                // Adjust weight based on season bias
                if (currentSeason.WeatherBias.Contains(weather.Id))
                {
                    weight = 2f;
                }
                weatherChances[weather] = weight;
                totalWeight += weight;
            }

            float random = UnityEngine.Random.Range(0, totalWeight);
            WeatherState selectedWeather = possibleWeather[0];
            float currentWeight = 0;

            foreach (var weather in possibleWeather)
            {
                currentWeight += weatherChances[weather];
                if (random < currentWeight)
                {
                    selectedWeather = weather;
                    break;
                }
            }

            // Transition to new weather
            TransitionToWeather(selectedWeather);
        }

        private void TransitionToWeather(WeatherState newWeather)
        {
            var previousWeather = currentWeather;
            targetWeather = newWeather;
            weatherTransitionProgress = 0;
            isTransitioning = true;

            OnWeatherTransition?.Invoke(previousWeather, newWeather);
        }

        private void InitializeWeather()
        {
            // Start with clear weather
            if (weatherStates.TryGetValue("clear", out var clearWeather))
            {
                currentWeather = clearWeather;
                currentTemperature = clearWeather.Temperature;
                currentHumidity = clearWeather.Humidity;
                currentWindSpeed = clearWeather.WindSpeed;
            }

            // Set initial season
            if (seasons.TryGetValue("spring", out var spring))
            {
                currentSeason = spring;
            }

            weatherTimer = UnityEngine.Random.Range(30f, 60f);
        }

        private void LoadWeatherData()
        {
            if (weatherJsonFile == null) return;

            try
            {
                var wrapper = JsonUtility.FromJson<WeatherDataWrapper>(weatherJsonFile.text);
                if (wrapper.weatherTypes != null)
                {
                    foreach (var weather in wrapper.weatherTypes)
                    {
                        weatherStates[weather.Id] = weather;
                    }
                }
                DebugLog($"Loaded {weatherStates.Count} weather types");
            }
            catch (Exception e)
            {
                DebugLog($"Error loading weather data: {e.Message}");
            }
        }

        private void LoadBiomeData()
        {
            if (biomesJsonFile == null) return;

            try
            {
                var wrapper = JsonUtility.FromJson<BiomeDataWrapper>(biomesJsonFile.text);
                if (wrapper.biomes != null)
                {
                    foreach (var biome in wrapper.biomes)
                    {
                        biomes[biome.Id] = biome;
                    }
                }
                DebugLog($"Loaded {biomes.Count} biomes");
            }
            catch (Exception e)
            {
                DebugLog($"Error loading biome data: {e.Message}");
            }
        }

        private void LoadSeasonData()
        {
            if (seasonsJsonFile == null) return;

            try
            {
                var wrapper = JsonUtility.FromJson<SeasonDataWrapper>(seasonsJsonFile.text);
                if (wrapper.seasons != null)
                {
                    foreach (var season in wrapper.seasons)
                    {
                        seasons[season.Id] = season;
                    }
                }
                DebugLog($"Loaded {seasons.Count} seasons");
            }
            catch (Exception e)
            {
                DebugLog($"Error loading season data: {e.Message}");
            }
        }

        /// <summary>
        /// Generate world biome map using simplex/perlin noise.
        /// </summary>
        public void GenerateWorldBiomes()
        {
            worldBiomes.Clear();
            temperatureMap = new float[worldWidth, worldHeight];
            moistureMap = new float[worldWidth, worldHeight];

            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    float noiseX = x * biomeScale;
                    float noiseY = y * biomeScale;

                    // Calculate temperature based on latitude and noise
                    float latitudeFactor = Mathf.Abs(y - worldHeight / 2f) / (worldHeight / 2f);
                    float temperatureNoise = Mathf.PerlinNoise(noiseX, noiseY) * 0.5f - 0.25f;
                    float temperature = temperatureGradient * (1f - latitudeFactor) + temperatureNoise;
                    temperature = Mathf.Clamp01(temperature);

                    // Calculate moisture based on noise
                    float moisture = Mathf.PerlinNoise(noiseX + 100, noiseY + 100);
                    moisture = Mathf.Clamp01(moisture);

                    temperatureMap[x, y] = temperature;
                    moistureMap[x, y] = moisture;

                    // Determine biome
                    Biome biome = DetermineBiome(temperature, moisture);
                    worldBiomes[new Vector2Int(x, y)] = biome;
                }
            }

            DebugLog($"Generated world biomes: {worldBiomes.Count} tiles");
        }

        /// <summary>
        /// Determine biome based on temperature and moisture.
        /// </summary>
        private Biome DetermineBiome(float temperature, float moisture)
        {
            foreach (var biome in biomes.Values)
            {
                if (temperature >= biome.Temperature.Min / 50f &&
                    temperature <= biome.Temperature.Max / 50f &&
                    moisture >= (biome.Precipitation - 20) / 100f &&
                    moisture <= (biome.Precipitation + 20) / 100f)
                {
                    return biome;
                }
            }

            // Default to grassland
            return biomes.TryGetValue("grassland", out var grassland) ? grassland : null;
        }

        /// <summary>
        /// Get the biome at a specific world position.
        /// </summary>
        public Biome GetBiomeAtPosition(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x);
            int y = Mathf.FloorToInt(position.z);
            x = Mathf.Clamp(x, 0, worldWidth - 1);
            y = Mathf.Clamp(y, 0, worldHeight - 1);

            Vector2Int tilePos = new Vector2Int(x, y);
            return worldBiomes.TryGetValue(tilePos, out var biome) ? biome : null;
        }

        /// <summary>
        /// Set the biome at a specific position (for terraforming).
        /// </summary>
        public void SetBiomeAtPosition(Vector3 position, string biomeId)
        {
            int x = Mathf.FloorToInt(position.x);
            int y = Mathf.FloorToInt(position.z);
            x = Mathf.Clamp(x, 0, worldWidth - 1);
            y = Mathf.Clamp(y, 0, worldHeight - 1);

            if (biomes.TryGetValue(biomeId, out var biome))
            {
                worldBiomes[new Vector2Int(x, y)] = biome;
            }
        }

        /// <summary>
        /// Get the temperature at a specific position.
        /// </summary>
        public float GetTemperatureAtPosition(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x);
            int y = Mathf.FloorToInt(position.z);
            x = Mathf.Clamp(x, 0, worldWidth - 1);
            y = Mathf.Clamp(y, 0, worldHeight - 1);

            if (temperatureMap != null)
            {
                return temperatureMap[x, y];
            }
            return 20f; // Default temperature
        }

        /// <summary>
        /// Set the current season.
        /// </summary>
        public void SetSeason(string seasonId)
        {
            if (seasons.TryGetValue(seasonId, out var season))
            {
                var previousSeason = currentSeason;
                currentSeason = season;
                OnSeasonChanged?.Invoke(season);

                // Update temperature based on season
                currentTemperature = (currentSeason.Temperature.Min + currentSeason.Temperature.Max) / 2f;
                OnTemperatureChanged?.Invoke(currentTemperature);
            }
        }

        /// <summary>
        /// Advance to the next season.
        /// </summary>
        public void AdvanceSeason()
        {
            if (currentSeason == null) return;

            var seasonKeys = new List<string>(seasons.Keys);
            int currentIndex = seasonKeys.IndexOf(currentSeason.Id);
            int nextIndex = (currentIndex + 1) % seasonKeys.Count;

            SetSeason(seasonKeys[nextIndex]);
        }

        /// <summary>
        /// Force a specific weather state (for testing or events).
        /// </summary>
        public void ForceWeather(string weatherId)
        {
            if (weatherStates.TryGetValue(weatherId, out var weather))
            {
                TransitionToWeather(weather);
            }
        }

        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
            {
                ["currentWeatherId"] = currentWeather?.Id ?? "",
                ["currentBiomeId"] = currentBiome?.Id ?? "",
                ["currentSeasonId"] = currentSeason?.Id ?? "",
                ["currentTemperature"] = currentTemperature,
                ["currentHumidity"] = currentHumidity,
                ["currentWindSpeed"] = currentWindSpeed
            };
        }

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[WeatherManager] {message}");
        }

        [Serializable]
        private class WeatherDataWrapper
        {
            public List<WeatherState> weatherTypes;
        }

        [Serializable]
        private class BiomeDataWrapper
        {
            public List<Biome> biomes;
        }

        [Serializable]
        private class SeasonDataWrapper
        {
            public List<Season> seasons;
        }
    }

    /// <summary>
    /// Represents a weather state.
    /// </summary>
    [Serializable]
    public class WeatherState
    {
        public string Id;
        public string Name;
        public string DisplayName;
        public string Icon;
        public Color SkyColor;
        public Color CloudColor;
        public Color LightColor;
        public float FogDensity;
        public float WindSpeed;
        public float Temperature;
        public float Humidity;
        public float Precipitation;
        public float Visibility;
        public string AmbientSound;
        public Vector2 Duration;
        public List<string> Effects;
        public object EraUnlocks;
    }

    /// <summary>
    /// Represents a world biome.
    /// </summary>
    [Serializable]
    public class Biome
    {
        public string Id;
        public string Name;
        public string DisplayName;
        public List<string> PrimaryWeather = new();
        public List<string> SecondaryWeather = new();
        public List<string> RareWeather = new();
        public TemperatureRange Temperature = new();
        public float Precipitation;
        public float Fertility;
        public List<string> Resources = new();
        public List<string> Vegetation = new();
        public Color Color;
    }

    [Serializable]
    public class TemperatureRange
    {
        public float Min;
        public float Max;
    }

    /// <summary>
    /// Represents a season.
    /// </summary>
    [Serializable]
    public class Season
    {
        public string Id;
        public string Name;
        public int Duration;
        public TemperatureRange Temperature = new();
        public List<string> Effects = new();
        public List<string> WeatherBias = new();
    }
}


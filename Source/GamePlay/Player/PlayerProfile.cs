using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChronoCiv.GamePlay.Player
{
    /// <summary>
    /// Player profile data structure containing character attributes and settings.
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        [Header("Basic Info")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string gender = "neutral";
        [SerializeField] private string archetype = "leader";

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [SerializeField] private float rotationSpeed = 15f;

        [Header("Stats")]
        [SerializeField] private int startingHealth = 100;
        [SerializeField] private int startingEnergy = 100;
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int maxEnergy = 100;

        [Header("Appearance")]
        [SerializeField] private string spriteBase;
        [SerializeField] private List<string> animations = new List<string> { "idle", "walk", "run", "interact" };
        [SerializeField] private Vector2 spriteSize = new Vector2(16, 24);
        [SerializeField] private Color primaryColor = Color.white;
        [SerializeField] private Color secondaryColor = Color.gray;

        [Header("Clothing by Era")]
        [SerializeField] private Dictionary<string, string> clothing = new Dictionary<string, string>();

        [Header("Equipment")]
        [SerializeField] private List<string> startingEquipment = new();

        [Header("Skills")]
        [SerializeField] private List<string> skills = new List<string> { "leadership", "diplomacy" };

        [Header("Progression")]
        [SerializeField] private int startingLevel = 1;
        [SerializeField] private int startingExperience = 0;

        // Public Properties
        public string Id => id;
        public string DisplayName => displayName;
        public string Gender => gender;
        public string Archetype => archetype;
        public float MoveSpeed => moveSpeed;
        public float RunSpeed => runSpeed;
        public float RotationSpeed => rotationSpeed;
        public int StartingHealth => startingHealth;
        public int StartingEnergy => startingEnergy;
        public int MaxHealth => maxHealth;
        public int MaxEnergy => maxEnergy;
        public string SpriteBase => spriteBase;
        public IReadOnlyList<string> Animations => animations;
        public Vector2 SpriteSize => spriteSize;
        public Color PrimaryColor => primaryColor;
        public Color SecondaryColor => secondaryColor;
        public IReadOnlyDictionary<string, string> Clothing => clothing;
        public IReadOnlyList<string> StartingEquipment => startingEquipment;
        public IReadOnlyList<string> Skills => skills;
        public int StartingLevel => startingLevel;
        public int StartingExperience => startingExperience;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PlayerProfile()
        {
            id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Constructor with basic parameters.
        /// </summary>
        public PlayerProfile(string id, string displayName)
        {
            this.id = id;
            this.displayName = displayName;
        }

        /// <summary>
        /// Get clothing for a specific era.
        /// </summary>
        public string GetClothingForEra(string eraId)
        {
            if (clothing.TryGetValue(eraId, out var clothingId))
            {
                return clothingId;
            }
            return clothing.TryGetValue("default", out var defaultClothing) ? defaultClothing : "";
        }

        /// <summary>
        /// Add a skill to the player profile.
        /// </summary>
        public void AddSkill(string skill)
        {
            if (!skills.Contains(skill))
            {
                skills.Add(skill);
            }
        }

        /// <summary>
        /// Remove a skill from the player profile.
        /// </summary>
        public void RemoveSkill(string skill)
        {
            skills.Remove(skill);
        }

        /// <summary>
        /// Set clothing for a specific era.
        /// </summary>
        public void SetClothingForEra(string eraId, string clothingId)
        {
            clothing[eraId] = clothingId;
        }

        /// <summary>
        /// Add equipment to the player profile.
        /// </summary>
        public void AddEquipment(string equipment)
        {
            if (!startingEquipment.Contains(equipment))
            {
                startingEquipment.Add(equipment);
            }
        }

        /// <summary>
        /// Get the full sprite path for a specific animation.
        /// </summary>
        public string GetSpritePath(string animation)
        {
            return $"Sprites/NPCs/Player/{animation}";
        }

        /// <summary>
        /// Convert to dictionary for saving.
        /// </summary>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["id"] = id,
                ["displayName"] = displayName,
                ["gender"] = gender,
                ["archetype"] = archetype,
                ["moveSpeed"] = moveSpeed,
                ["runSpeed"] = runSpeed,
                ["rotationSpeed"] = rotationSpeed,
                ["startingHealth"] = startingHealth,
                ["startingEnergy"] = startingEnergy,
                ["maxHealth"] = maxHealth,
                ["maxEnergy"] = maxEnergy,
                ["spriteBase"] = spriteBase,
                ["spriteSize"] = new List<float> { spriteSize.x, spriteSize.y },
                ["primaryColor"] = new List<float> { primaryColor.r, primaryColor.g, primaryColor.b, primaryColor.a },
                ["secondaryColor"] = new List<float> { secondaryColor.r, secondaryColor.g, secondaryColor.b, secondaryColor.a },
                ["clothing"] = clothing,
                ["startingEquipment"] = startingEquipment,
                ["skills"] = skills,
                ["startingLevel"] = startingLevel,
                ["startingExperience"] = startingExperience
            };
        }

        /// <summary>
        /// Create a PlayerProfile from saved data.
        /// </summary>
        public static PlayerProfile FromDictionary(Dictionary<string, object> data)
        {
            var profile = new PlayerProfile();

            if (data.TryGetValue("id", out var idObj)) profile.id = idObj.ToString();
            if (data.TryGetValue("displayName", out var nameObj)) profile.displayName = nameObj.ToString();
            if (data.TryGetValue("gender", out var genderObj)) profile.gender = genderObj.ToString();
            if (data.TryGetValue("archetype", out var archetypeObj)) profile.archetype = archetypeObj.ToString();
            if (data.TryGetValue("moveSpeed", out var moveSpeedObj)) profile.moveSpeed = Convert.ToSingle(moveSpeedObj);
            if (data.TryGetValue("runSpeed", out var runSpeedObj)) profile.runSpeed = Convert.ToSingle(runSpeedObj);
            if (data.TryGetValue("rotationSpeed", out var rotationSpeedObj)) profile.rotationSpeed = Convert.ToSingle(rotationSpeedObj);
            if (data.TryGetValue("startingHealth", out var healthObj)) profile.startingHealth = Convert.ToInt32(healthObj);
            if (data.TryGetValue("startingEnergy", out var energyObj)) profile.startingEnergy = Convert.ToInt32(energyObj);
            if (data.TryGetValue("maxHealth", out var maxHealthObj)) profile.maxHealth = Convert.ToInt32(maxHealthObj);
            if (data.TryGetValue("maxEnergy", out var maxEnergyObj)) profile.maxEnergy = Convert.ToInt32(maxEnergyObj);
            if (data.TryGetValue("spriteBase", out var spriteObj)) profile.spriteBase = spriteObj.ToString();
            if (data.TryGetValue("startingLevel", out var levelObj)) profile.startingLevel = Convert.ToInt32(levelObj);
            if (data.TryGetValue("startingExperience", out var expObj)) profile.startingExperience = Convert.ToInt32(expObj);

            // Parse sprite size
            if (data.TryGetValue("spriteSize", out var sizeObj) && sizeObj is List<object> sizeList)
            {
                profile.spriteSize = new Vector2(Convert.ToSingle(sizeList[0]), Convert.ToSingle(sizeList[1]));
            }

            // Parse colors
            if (data.TryGetValue("primaryColor", out var colorObj) && colorObj is List<object> primaryColorList)
            {
                profile.primaryColor = new Color(
                    Convert.ToSingle(primaryColorList[0]),
                    Convert.ToSingle(primaryColorList[1]),
                    Convert.ToSingle(primaryColorList[2]),
                    Convert.ToSingle(primaryColorList[3])
                );
            }

            // Parse clothing
            if (data.TryGetValue("clothing", out var clothingObj) && clothingObj is Dictionary<string, object> clothingDict)
            {
                foreach (var kvp in clothingDict)
                {
                    profile.clothing[kvp.Key] = kvp.Value.ToString();
                }
            }

            return profile;
        }
    }

    /// <summary>
    /// Container for multiple player profiles (for save game selection).
    /// </summary>
    [Serializable]
    public class PlayerProfilesWrapper
    {
        public List<PlayerProfile> profiles;
    }
}


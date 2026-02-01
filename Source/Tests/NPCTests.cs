using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ChronoCiv.GamePlay.NPCs;

namespace ChronoCiv.Tests
{
    /// <summary>
    /// Unit tests for NPC system validation.
    /// Tests NPC data structures and basic functionality.
    /// </summary>
    public class NPCTests
    {
        [Test]
        [Category("NPC System")]
        [Description("Verify NPC data structure can be initialized")]
        public void NPC_DataStructureInitialization()
        {
            var npc = new NPC();
            Assert.IsNotNull(npc, "NPC instance should be created successfully");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify NPC properties can be set and retrieved")]
        public void NPC_PropertyAccess()
        {
            var npc = new NPC();
            // Note: NPC properties are private set or read-only, so we can't directly set them
            // This test verifies the properties exist and are accessible
            Assert.IsNotNull(npc.NPCId, "NPC ID should be generated");
            Assert.IsNotNull(npc.DisplayName, "Display name should be accessible");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify NPC state enum values")]
        public void NPCState_EnumValues()
        {
            var states = System.Enum.GetValues(typeof(NPCState));
            Assert.IsNotEmpty(states, "NPCState enum should have defined values");

            // Verify common states exist
            Assert.IsTrue(System.Enum.IsDefined(typeof(NPCState), NPCState.Idle));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NPCState), NPCState.Working));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NPCState), NPCState.Moving));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NPCState), NPCState.Sleeping));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NPCState), NPCState.Socializing));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NPCState), NPCState.Dead));
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify Direction enum values")]
        public void Direction_EnumValues()
        {
            var directions = System.Enum.GetValues(typeof(Direction));
            Assert.IsNotEmpty(directions, "Direction enum should have defined values");

            // Verify all 4 cardinal directions exist
            Assert.IsTrue(System.Enum.IsDefined(typeof(Direction), Direction.Up));
            Assert.IsTrue(System.Enum.IsDefined(typeof(Direction), Direction.Down));
            Assert.IsTrue(System.Enum.IsDefined(typeof(Direction), Direction.Left));
            Assert.IsTrue(System.Enum.IsDefined(typeof(Direction), Direction.Right));
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify Task data structure")]
        public void Task_DataStructure()
        {
            var task = new Task
            {
                Id = "task_harvest",
                Name = "Harvest Crops",
                Description = "Harvest crops from the fields",
                Duration = 10,
                ExperienceReward = 25
            };

            Assert.AreEqual("task_harvest", task.Id, "Task ID should match");
            Assert.AreEqual("Harvest Crops", task.Name, "Task name should match");
            Assert.AreEqual("Harvest crops from the fields", task.Description, "Task description should match");
            Assert.AreEqual(10, task.Duration, "Task duration should match");
            Assert.AreEqual(25, task.ExperienceReward, "Experience reward should match");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify Task with target position")]
        public void Task_TargetPosition()
        {
            var task = new Task
            {
                Id = "task_build",
                Name = "Build House",
                TargetPosition = new Vector3(10, 5, 0)
            };

            Assert.IsTrue(task.TargetPosition.HasValue, "Task should have target position");
            Assert.AreEqual(new Vector3(10, 5, 0), task.TargetPosition.Value, "Target position should match");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify Task without target position")]
        public void Task_NoTargetPosition()
        {
            var task = new Task
            {
                Id = "task_pray",
                Name = "Pray at Temple"
            };

            Assert.IsFalse(task.TargetPosition.HasValue, "Task should not have target position");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify NPCProfile data structure")]
        public void NPCProfile_DataStructure()
        {
            var profile = new NPCProfile
            {
                Id = "farmer_basic",
                Name = "Basic Farmer",
                Era = "stone_age",
                Archetype = "farmer",
                DefaultTask = "farming",
                SpawnWeight = 10,
                CanMigrate = true,
                CanReproduce = true,
                Lifespan = 60,
                Description = "A basic farmer NPC"
            };

            Assert.AreEqual("farmer_basic", profile.Id, "Profile ID should match");
            Assert.AreEqual("Basic Farmer", profile.Name, "Profile name should match");
            Assert.AreEqual("stone_age", profile.Era, "Era should match");
            Assert.AreEqual("farmer", profile.Archetype, "Archetype should match");
            Assert.AreEqual("farming", profile.DefaultTask, "Default task should match");
            Assert.AreEqual(10, profile.SpawnWeight, "Spawn weight should match");
            Assert.IsTrue(profile.CanMigrate, "Should be able to migrate");
            Assert.IsTrue(profile.CanReproduce, "Should be able to reproduce");
            Assert.AreEqual(60, profile.Lifespan, "Lifespan should match");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify NPCProfile with animations")]
        public void NPCProfile_Animations()
        {
            var profile = new NPCProfile
            {
                Id = "guard",
                Name = "Town Guard",
                Animations = new List<string> { "idle", "walk", "run", "attack", "salute" }
            };

            Assert.AreEqual(5, profile.Animations.Count, "Should have 5 animations");
            Assert.Contains("idle", profile.Animations, "Should have idle animation");
            Assert.Contains("attack", profile.Animations, "Should have attack animation");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify NPCProfile sprite settings")]
        public void NPCProfile_SpriteSettings()
        {
            var profile = new NPCProfile
            {
                SpriteBase = "npc_human_male_01",
                SpriteSize = new List<int> { 32, 48 }
            };

            Assert.AreEqual("npc_human_male_01", profile.SpriteBase, "Sprite base should match");
            Assert.AreEqual(2, profile.SpriteSize.Count, "Should have 2 size values");
            Assert.AreEqual(32, profile.SpriteSize[0], "Width should match");
            Assert.AreEqual(48, profile.SpriteSize[1], "Height should match");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify NPCProfile clothing dictionary")]
        public void NPCProfile_Clothing()
        {
            var profile = new NPCProfile
            {
                Clothing = new Dictionary<string, string>
                {
                    { "stone_age", "tunic_basic" },
                    { "ancient", "tunic_fine" },
                    { "medieval", "armor_iron" }
                }
            };

            Assert.AreEqual(3, profile.Clothing.Count, "Should have clothing for 3 eras");
            Assert.AreEqual("tunic_basic", profile.Clothing["stone_age"], "Stone age clothing should match");
            Assert.AreEqual("armor_iron", profile.Clothing["medieval"], "Medieval clothing should match");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify NPCProfile equipment list")]
        public void NPCProfile_Equipment()
        {
            var profile = new NPCProfile
            {
                Equipment = new List<string> { "shovel", "backpack", "water_bottle" }
            };

            Assert.AreEqual(3, profile.Equipment.Count, "Should have 3 equipment items");
            Assert.Contains("shovel", profile.Equipment, "Should have shovel");
            Assert.Contains("backpack", profile.Equipment, "Should have backpack");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify NPCProfile skills list")]
        public void NPCProfile_Skills()
        {
            var profile = new NPCProfile
            {
                Skills = new List<string> { "farming", "cooking", "bartering" }
            };

            Assert.AreEqual(3, profile.Skills.Count, "Should have 3 skills");
            Assert.Contains("farming", profile.Skills, "Should have farming skill");
            Assert.Contains("cooking", profile.Skills, "Should have cooking skill");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify NPC profile with dialogue set")]
        public void NPCProfile_DialogueSet()
        {
            var profile = new NPCProfile
            {
                Id = "merchant",
                Name = "Traveling Merchant",
                DialogueSet = "merchant_greetings"
            };

            Assert.AreEqual("merchant_greetings", profile.DialogueSet, "Dialogue set should match");
        }
    }
}


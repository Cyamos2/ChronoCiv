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
            var npc = new NPC
            {
                npcId = "npc_001",
                name = "Test NPC",
                profession = "Farmer",
                era = "stone_age",
                age = 25
            };

            Assert.AreEqual("npc_001", npc.npcId, "NPC ID should match");
            Assert.AreEqual("Test NPC", npc.name, "NPC name should match");
            Assert.AreEqual("Farmer", npc.profession, "NPC profession should match");
            Assert.AreEqual("stone_age", npc.era, "NPC era should match");
            Assert.AreEqual(25, npc.age, "NPC age should match");
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify NPC profile data structure")]
        public void NPCProfile_DataStructure()
        {
            var profile = new NPCProfile
            {
                id = "farmer_basic",
                name = "Basic Farmer",
                professions = new List<string> { "farmer" },
                eras = new List<string> { "stone_age", "ancient", "medieval" },
                spawnWeight = 10
            };

            Assert.AreEqual("farmer_basic", profile.id, "Profile ID should match");
            Assert.AreEqual("Basic Farmer", profile.name, "Profile name should match");
            Assert.AreEqual(3, profile.eras.Count, "Should support 3 eras");
            Assert.AreEqual(10, profile.spawnWeight, "Spawn weight should match");
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
        }

        [Test]
        [Category("NPC System")]
        [Description("Verify NPC direction enum values")]
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
                taskId = "task_harvest",
                name = "Harvest Crops",
                description = "Harvest crops from the fields",
                duration = 10,
                rewards = new Dictionary<string, int>
                {
                    { "food", 50 },
                    { "gold", 10 }
                }
            };

            Assert.AreEqual("task_harvest", task.taskId, "Task ID should match");
            Assert.AreEqual("Harvest Crops", task.name, "Task name should match");
            Assert.AreEqual(10, task.duration, "Task duration should match");
            Assert.AreEqual(2, task.rewards.Count, "Should have 2 reward types");
        }
    }
}

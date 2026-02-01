using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ChronoCiv.GamePlay.Eras;

namespace ChronoCiv.Tests
{
    /// <summary>
    /// Unit tests for Era system validation and transitions.
    /// Tests the core era progression mechanics.
    /// </summary>
    public class EraTests
    {
        [Test]
        [Category("Era System")]
        [Description("Verify era data structure is properly initialized")]
        public void Era_DataStructureInitialization()
        {
            // Test Era class can be instantiated
            var era = new Era();
            Assert.IsNotNull(era, "Era instance should be created successfully");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify era properties can be set and retrieved")]
        public void Era_PropertyAccess()
        {
            var era = new Era
            {
                Id = "test_era",
                Name = "Test Era",
                DisplayName = "Test Era Display",
                StartYear = 2000,
                EndYear = 2500,
                Description = "A test era for validation"
            };

            Assert.AreEqual("test_era", era.Id, "Era ID should match");
            Assert.AreEqual("Test Era", era.Name, "Era name should match");
            Assert.AreEqual("Test Era Display", era.DisplayName, "Display name should match");
            Assert.AreEqual(2000, era.StartYear, "Era start year should match");
            Assert.AreEqual(2500, era.EndYear, "Era end year should match");
            Assert.AreEqual("A test era for validation", era.Description, "Era description should match");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify era year ranges are logical")]
        public void Era_YearRangeValidation()
        {
            var ancientEra = new Era
            {
                Id = "ancient",
                Name = "Ancient Era",
                StartYear = -3000,
                EndYear = -500
            };

            Assert.Less(ancientEra.StartYear, ancientEra.EndYear,
                "Start year should be less than end year for ancient era");
            Assert.Negative(ancientEra.StartYear, "Ancient era start year should be negative (BCE)");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify era duration calculation")]
        public void Era_DurationCalculation()
        {
            var medievalEra = new Era
            {
                StartYear = 500,
                EndYear = 1400
            };

            int expectedDuration = 900; // 1400 - 500
            int actualDuration = medievalEra.EndYear - medievalEra.StartYear;

            Assert.AreEqual(expectedDuration, actualDuration,
                "Era duration should be correctly calculated from year range");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify Era.Duration property")]
        public void Era_DurationProperty()
        {
            var renaissanceEra = new Era
            {
                StartYear = 1400,
                EndYear = 1600
            };

            Assert.AreEqual(200, renaissanceEra.Duration, "Duration property should return correct value");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify ContainsYear method")]
        public void Era_ContainsYear()
        {
            var modernEra = new Era
            {
                StartYear = 1900,
                EndYear = 2000
            };

            Assert.IsTrue(modernEra.ContainsYear(1950), "Year 1950 should be in modern era");
            Assert.IsTrue(modernEra.ContainsYear(1900), "Start year should be included");
            Assert.IsFalse(modernEra.ContainsYear(2000), "End year should not be included");
            Assert.IsFalse(modernEra.ContainsYear(1800), "Year before era should not be included");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify GetProgress method")]
        public void Era_GetProgress()
        {
            var industrialEra = new Era
            {
                StartYear = 1760,
                EndYear = 1840
            };

            // At start
            Assert.AreEqual(0f, industrialEra.GetProgress(1760), "Progress at start should be 0");

            // At midpoint
            float midProgress = industrialEra.GetProgress(1800);
            Assert.AreEqual(0.5f, midProgress, 0.01f, "Progress at midpoint should be 0.5");

            // At end
            Assert.AreEqual(1f, industrialEra.GetProgress(1840), "Progress at end should be 1");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify GetColor method")]
        public void Era_GetColor()
        {
            var goldenEra = new Era
            {
                Color = "#FFD700"
            };

            Color color = goldenEra.GetColor();
            Assert.AreNotEqual(Color.white, color, "Should return actual color, not white");
            Assert.AreEqual(1f, color.r, "Red component should be 1");
            Assert.AreEqual(0.84f, color.g, 0.01f, "Green component should be ~0.84");
            Assert.AreEqual(0f, color.b, "Blue component should be 0");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify EraTransition data structure")]
        public void EraTransition_DataStructure()
        {
            var transition = new EraTransition
            {
                TriggerYear = "0",
                VisualEffect = "fade",
                SoundEffect = "chime",
                Description = "A new era begins!"
            };

            Assert.AreEqual("0", transition.TriggerYear, "Trigger year should match (as string)");
            Assert.AreEqual("fade", transition.VisualEffect, "Visual effect should match");
            Assert.AreEqual("chime", transition.SoundEffect, "Sound effect should match");
            Assert.AreEqual("A new era begins!", transition.Description, "Description should match");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify era with buildings list")]
        public void Era_BuildingsList()
        {
            var medievalEra = new Era
            {
                Id = "medieval",
                Buildings = new List<string> { "castle", "blacksmith", "market", "tower" }
            };

            Assert.AreEqual(4, medievalEra.Buildings.Count, "Should have 4 buildings");
            Assert.Contains("castle", medievalEra.Buildings, "Castle should be in buildings list");
            Assert.Contains("blacksmith", medievalEra.Buildings, "Blacksmith should be in buildings list");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify era technology level")]
        public void Era_TechnologyLevel()
        {
            var stoneAge = new Era { TechnologyLevel = 1 };
            var modernEra = new Era { TechnologyLevel = 10 };

            Assert.AreEqual(1, stoneAge.TechnologyLevel, "Stone age should have tech level 1");
            Assert.AreEqual(10, modernEra.TechnologyLevel, "Modern era should have tech level 10");
        }
    }
}


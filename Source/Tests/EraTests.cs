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
                id = "test_era",
                name = "Test Era",
                startYear = 2000,
                endYear = 2500,
                description = "A test era for validation"
            };

            Assert.AreEqual("test_era", era.id, "Era ID should match");
            Assert.AreEqual("Test Era", era.name, "Era name should match");
            Assert.AreEqual(2000, era.startYear, "Era start year should match");
            Assert.AreEqual(2500, era.endYear, "Era end year should match");
            Assert.AreEqual("A test era for validation", era.description, "Era description should match");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify era year ranges are logical")]
        public void Era_YearRangeValidation()
        {
            var ancientEra = new Era
            {
                id = "ancient",
                name = "Ancient Era",
                startYear = -3000,
                endYear = -500
            };

            Assert.Less(ancientEra.startYear, ancientEra.endYear,
                "Start year should be less than end year for ancient era");
            Assert.Negative(ancientEra.startYear, "Ancient era start year should be negative (BCE)");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify era duration calculation")]
        public void Era_DurationCalculation()
        {
            var medievalEra = new Era
            {
                startYear = 500,
                endYear = 1400
            };

            int expectedDuration = 900; // 1400 - 500
            int actualDuration = medievalEra.endYear - medievalEra.startYear;

            Assert.AreEqual(expectedDuration, actualDuration,
                "Era duration should be correctly calculated from year range");
        }

        [Test]
        [Category("Era System")]
        [Description("Verify era transition data structure")]
        public void EraTransition_DataStructure()
        {
            var transition = new EraTransition
            {
                fromEra = "stone_age",
                toEra = "ancient",
                triggerYear = -3000,
                requirements = new List<string> { "writing", "wheel" }
            };

            Assert.AreEqual("stone_age", transition.fromEra, "From era should match");
            Assert.AreEqual("ancient", transition.toEra, "To era should match");
            Assert.AreEqual(-3000, transition.triggerYear, "Trigger year should match");
            Assert.AreEqual(2, transition.requirements.Count, "Should have 2 requirements");
        }
    }
}

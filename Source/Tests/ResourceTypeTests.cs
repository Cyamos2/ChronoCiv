using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ChronoCiv.GamePlay.Resources;

namespace ChronoCiv.Tests
{
    /// <summary>
    /// Unit tests for ResourceType enum validation and resource system.
    /// Tests the core resource types used throughout the game.
    /// </summary>
    public class ResourceTypeTests
    {
        [Test]
        [Category("Resource System")]
        [Description("Verify all resource types are properly defined")]
        public void ResourceType_AllResourcesAreDefined()
        {
            // Test that we can iterate through all resource types
            var resourceTypes = System.Enum.GetValues(typeof(ResourceType));
            Assert.IsNotEmpty(resourceTypes, "ResourceType enum should have defined values");

            int resourceCount = resourceTypes.Length;
            Assert.GreaterOrEqual(resourceCount, 10, "Should have at least 10 resource types defined");
        }

        [Test]
        [Category("Resource System")]
        [Description("Verify core resources exist in the enum")]
        public void ResourceType_CoreResourcesExist()
        {
            // Verify essential resources are defined
            Assert.IsTrue(System.Enum.IsDefined(typeof(ResourceType), ResourceType.Food),
                "Food resource should be defined");
            Assert.IsTrue(System.Enum.IsDefined(typeof(ResourceType), ResourceType.Gold),
                "Gold resource should be defined");
            Assert.IsTrue(System.Enum.IsDefined(typeof(ResourceType), ResourceType.Wood),
                "Wood resource should be defined");
            Assert.IsTrue(System.Enum.IsDefined(typeof(ResourceType), ResourceType.Stone),
                "Stone resource should be defined");
            Assert.IsTrue(System.Enum.IsDefined(typeof(ResourceType), ResourceType.Iron),
                "Iron resource should be defined");
        }

        [Test]
        [Category("Resource System")]
        [Description("Verify resource type conversion to string works correctly")]
        public void ResourceType_StringConversionWorks()
        {
            // Test string representation of resource types
            foreach (ResourceType resource in System.Enum.GetValues(typeof(ResourceType)))
            {
                string resourceName = resource.ToString();
                Assert.IsNotNullOrEmpty(resourceName,
                    $"Resource type {resource} should have a valid string representation");
                Assert.IsTrue(resourceName.Length > 0,
                    $"Resource type {resource} should not convert to empty string");
            }
        }

        [Test]
        [Category("Resource System")]
        [Description("Verify resource type parsing from string works")]
        public void ResourceType_ParseFromStringWorks()
        {
            // Test parsing common resource types from strings
            Assert.AreEqual(ResourceType.Food, System.Enum.Parse<ResourceType>("Food", ignoreCase: false));
            Assert.AreEqual(ResourceType.Gold, System.Enum.Parse<ResourceType>("Gold", ignoreCase: false));
            Assert.AreEqual(ResourceType.Wood, System.Enum.Parse<ResourceType>("Wood", ignoreCase: false));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ChronoCiv.GamePlay.Resources;

namespace ChronoCiv.Tests
{
    /// <summary>
    /// Unit tests for ResourceType class validation and resource system.
    /// Tests the core resource types used throughout the game.
    /// </summary>
    public class ResourceTypeTests
    {
        [Test]
        [Category("Resource System")]
        [Description("Verify ResourceType class can be instantiated")]
        public void ResourceType_CanBeInstantiated()
        {
            var resourceType = new ResourceType();
            Assert.IsNotNull(resourceType, "ResourceType instance should be created successfully");
        }

        [Test]
        [Category("Resource System")]
        [Description("Verify ResourceType properties can be set and retrieved")]
        public void ResourceType_PropertyAccess()
        {
            var resourceType = new ResourceType
            {
                Id = "food",
                Name = "Food",
                DisplayName = "Food Resources",
                Icon = "food_icon",
                Category = "Basic",
                Description = "Essential for survival",
                Renewable = true,
                RenewalRate = 10f,
                DepletionRate = 0f,
                BaseValue = 1f,
                Weight = 1f,
                Color = "#FF0000"
            };

            Assert.AreEqual("food", resourceType.Id, "Resource ID should match");
            Assert.AreEqual("Food", resourceType.Name, "Resource name should match");
            Assert.AreEqual("Food Resources", resourceType.DisplayName, "Display name should match");
            Assert.AreEqual("food_icon", resourceType.Icon, "Icon should match");
            Assert.AreEqual("Basic", resourceType.Category, "Category should match");
            Assert.AreEqual("Essential for survival", resourceType.Description, "Description should match");
            Assert.IsTrue(resourceType.Renewable, "Should be renewable");
            Assert.AreEqual(10f, resourceType.RenewalRate, "Renewal rate should match");
            Assert.AreEqual(0f, resourceType.DepletionRate, "Depletion rate should match");
            Assert.AreEqual(1f, resourceType.BaseValue, "Base value should match");
            Assert.AreEqual(1f, resourceType.Weight, "Weight should match");
            Assert.AreEqual("#FF0000", resourceType.Color, "Color should match");
        }

        [Test]
        [Category("Resource System")]
        [Description("Verify ResourceType with different categories")]
        public void ResourceType_DifferentCategories()
        {
            var food = new ResourceType { Id = "food", Category = "Basic" };
            var wood = new ResourceType { Id = "wood", Category = "Material" };
            var gold = new ResourceType { Id = "gold", Category = "Currency" };

            Assert.AreEqual("Basic", food.Category, "Food should be Basic category");
            Assert.AreEqual("Material", wood.Category, "Wood should be Material category");
            Assert.AreEqual("Currency", gold.Category, "Gold should be Currency category");
        }

        [Test]
        [Category("Resource System")]
        [Description("Verify renewable vs non-renewable resources")]
        public void ResourceType_Renewability()
        {
            var renewableResource = new ResourceType
            {
                Id = "wood",
                Renewable = true,
                RenewalRate = 5f
            };

            var nonRenewableResource = new ResourceType
            {
                Id = "iron",
                Renewable = false,
                RenewalRate = 0f
            };

            Assert.IsTrue(renewableResource.Renewable, "Wood should be renewable");
            Assert.AreEqual(5f, renewableResource.RenewalRate, "Wood should have renewal rate");
            Assert.IsFalse(nonRenewableResource.Renewable, "Iron should not be renewable");
            Assert.AreEqual(0f, nonRenewableResource.RenewalRate, "Iron should have zero renewal rate");
        }

        [Test]
        [Category("Resource System")]
        [Description("Verify resource value and weight properties")]
        public void ResourceType_ValueAndWeight()
        {
            var valuableResource = new ResourceType
            {
                Id = "gold",
                BaseValue = 100f,
                Weight = 0.5f
            };

            var basicResource = new ResourceType
            {
                Id = "stone",
                BaseValue = 1f,
                Weight = 5f
            };

            Assert.AreEqual(100f, valuableResource.BaseValue, "Gold should have high base value");
            Assert.AreEqual(0.5f, valuableResource.Weight, "Gold should have low weight");
            Assert.AreEqual(1f, basicResource.BaseValue, "Stone should have low base value");
            Assert.AreEqual(5f, basicResource.Weight, "Stone should have high weight");
        }
    }
}


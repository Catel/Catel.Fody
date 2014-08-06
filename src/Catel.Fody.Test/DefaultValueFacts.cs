// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultValueFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
    using System;
    using System.Runtime.Remoting;
    using Catel.Data;
    using Catel.Fody.TestAssembly;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultValueFacts
    {
        [TestCase]
        public void SetsNullAsDefaultValueWhenNoAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "LastName");
            Assert.AreEqual(null, propertyData.GetDefaultValue());
        }

        [TestCase]
        public void SetsDefaultValueForStringWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "FirstName");
            Assert.AreEqual("Geert", propertyData.GetDefaultValue());
        }

        [TestCase]
        public void SetsDefaultValueForBoolWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "BoolValue");
            Assert.AreEqual(true, propertyData.GetDefaultValue());
        }

        [TestCase]
        public void SetsDefaultValueForNullableBoolWhenNullAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "NullableBoolDefaultNullValue");
            Assert.AreEqual(null, propertyData.GetDefaultValue());
        }

        [TestCase]
        public void SetsDefaultValueForNullableBoolWhenTrueAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "NullableBoolDefaultTrueValue");
            Assert.AreEqual(true, propertyData.GetDefaultValue());
        }

        [TestCase]
        public void SetsDefaultValueForNullableBoolWhenFalseAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "NullableBoolDefaultFalseValue");
            Assert.AreEqual(false, propertyData.GetDefaultValue());
        }

        [TestCase]
        public void SetsDefaultValueForIntWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "IntValue");
            Assert.AreEqual(42, propertyData.GetDefaultValue());
        }

        [TestCase]
        public void SetsDefaultValueForLongWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "LongValue");
            Assert.AreEqual(42L, propertyData.GetDefaultValue());
        }

        [TestCase]
        public void SetsDefaultValueForDoubleWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "DoubleValue");
            Assert.AreEqual(42d, propertyData.GetDefaultValue());
        }

        [TestCase]
        public void SetsDefaultValueForFloatWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "FloatValue");
            Assert.AreEqual(42f, propertyData.GetDefaultValue());
        }

        [TestCase]
        public void SetsDefaultValueForEnumWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "EnumValue");
            Assert.AreEqual(ExampleEnum.B, (ExampleEnum)propertyData.GetDefaultValue());
        }
    }
}
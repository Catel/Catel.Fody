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
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DefaultValueFacts
    {
        [TestMethod]
        public void SetsNullAsDefaultValueWhenNoAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "LastName");
            Assert.AreEqual(null, propertyData.GetDefaultValue());
        }

        [TestMethod]
        public void SetsDefaultValueForStringWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "FirstName");
            Assert.AreEqual("Geert", propertyData.GetDefaultValue());
        }

        [TestMethod]
        public void SetsDefaultValueForBoolWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "BoolValue");
            Assert.AreEqual(true, propertyData.GetDefaultValue());
        }

        [TestMethod]
        public void SetsDefaultValueForIntWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "IntValue");
            Assert.AreEqual(42, propertyData.GetDefaultValue());
        }

        [TestMethod]
        public void SetsDefaultValueForLongWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "LongValue");
            Assert.AreEqual(42L, propertyData.GetDefaultValue());
        }

        [TestMethod]
        public void SetsDefaultValueForDoubleWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "DoubleValue");
            Assert.AreEqual(42d, propertyData.GetDefaultValue());
        }

        [TestMethod]
        public void SetsDefaultValueForFloatWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "FloatValue");
            Assert.AreEqual(42f, propertyData.GetDefaultValue());
        }

        [TestMethod]
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
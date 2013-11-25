// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExposeFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
    using System;
    using Catel.Data;
    using Catel.Fody.TestAssembly;
    using Catel.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExposeFacts
    {
        #region Methods
        [TestMethod]
        public void CreatesExposedProperties()
        {
            var modelType = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ExposingModel");
            var viewModelType = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ExposingViewModel");

            var model = Activator.CreateInstance(modelType);
            var viewModel = Activator.CreateInstance(viewModelType, new object[] {model});

            Assert.IsTrue(PropertyDataManager.Default.IsPropertyRegistered(viewModelType, "FirstName"));
            Assert.IsTrue(PropertyDataManager.Default.IsPropertyRegistered(viewModelType, "MappedLastName"));

            // Default value of the FirstName property on the model is "Geert"
            Assert.AreEqual("Geert",  PropertyHelper.GetPropertyValue<string>(viewModel, "FirstName"));

            // Default value of the LastName property on the model is "Geert"
            Assert.AreEqual("van Horrik", PropertyHelper.GetPropertyValue<string>(viewModel, "MappedLastName"));
        }

        [TestMethod]
        public void CreatesExposedPropertiesFromExternalTypes()
        {
            var modelType = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ExposingModel");
            var viewModelType = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ExposingViewModel");

            var model = Activator.CreateInstance(modelType);
            var viewModel = Activator.CreateInstance(viewModelType, new object[] { model });

            Assert.IsTrue(PropertyDataManager.Default.IsPropertyRegistered(viewModelType, "IsOk"));

            // Default value of the ExternalTypeProperty property on the model is "null"
            Assert.AreEqual(null, PropertyHelper.GetPropertyValue<bool?>(viewModel, "IsOk"));
        }
        #endregion
    }
}
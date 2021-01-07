﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExposeFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Tests
{
    using System;
    using Data;
    using Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class ExposeFacts
    {
        #region Methods
        [TestCase]
        public void CreatesExposedProperties()
        {
            var modelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ExposingDerivedModel");
            var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ExposingViewModel");

            var model = Activator.CreateInstance(modelType);
            var viewModel = Activator.CreateInstance(viewModelType, new [] {model});

            Assert.IsTrue(PropertyDataManager.Default.IsPropertyRegistered(viewModelType, "FirstName"));
            Assert.IsTrue(PropertyDataManager.Default.IsPropertyRegistered(viewModelType, "MappedLastName"));

            // Default value of the FirstName property on the model is "Geert"
            Assert.AreEqual("Geert",  PropertyHelper.GetPropertyValue<string>(viewModel, "FirstName"));

            // Default value of the LastName property on the model is "Geert"
            Assert.AreEqual("van Horrik", PropertyHelper.GetPropertyValue<string>(viewModel, "MappedLastName"));
        }

        [TestCase]
        public void CreatesExposedPropertiesFromExternalTypes()
        {
            var modelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ExposingDerivedModel");
            var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ExposingViewModel");

            var model = Activator.CreateInstance(modelType);
            var viewModel = Activator.CreateInstance(viewModelType, new [] { model });

            Assert.IsTrue(PropertyDataManager.Default.IsPropertyRegistered(viewModelType, "IsOk"));

            // Default value of the ExternalTypeProperty property on the model is "null"
            Assert.AreEqual(null, PropertyHelper.GetPropertyValue<bool?>(viewModel, "IsOk"));
        }

        [TestCase]
        public void CanCreateReadOnlyExposedProperties()
        {
            var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ExposingViewModel");

            var propertyInfo = viewModelType.GetPropertyEx("ReadOnlyProperty");

            var setMethod = propertyInfo.GetSetMethod(true);
            if (setMethod != null)
            {
                Assert.IsFalse(setMethod.IsPublic);
            }
        }
        #endregion
    }
}
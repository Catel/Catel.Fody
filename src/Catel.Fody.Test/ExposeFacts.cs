// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExposeFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
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
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ExposingViewModel");

            var model = new ExposingModel();
            var viewModel = new ExposingViewModel(model);

            Assert.IsTrue(PropertyDataManager.Default.IsPropertyRegistered(type, "FirstName"));
            Assert.IsTrue(PropertyDataManager.Default.IsPropertyRegistered(type, "MappedLastName"));

            // Default value of the FirstName property on the model is "Geert"
            Assert.AreEqual("Geert",  PropertyHelper.GetPropertyValue<string>(viewModel, "FirstName"));

            // Default value of the LastName property on the model is "Geert"
            Assert.AreEqual("van Horrik", PropertyHelper.GetPropertyValue<string>(viewModel, "MappedLastName"));
        }
        #endregion
    }
}
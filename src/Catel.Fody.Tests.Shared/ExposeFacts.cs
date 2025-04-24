namespace Catel.Fody.Tests
{
    using System;
    using Data;
    using Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class ExposeFacts
    {
        [TestCase]
        public void CreatesExposedProperties()
        {
            var modelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ExposingDerivedModel");
            var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ExposingViewModel");

            var model = Activator.CreateInstance(modelType);
            var viewModel = Activator.CreateInstance(viewModelType, new [] {model});

            Assert.That(PropertyDataManager.Default.IsPropertyRegistered(viewModelType, "FirstName"), Is.True);
            Assert.That(PropertyDataManager.Default.IsPropertyRegistered(viewModelType, "MappedLastName"), Is.True);

            // Default value of the FirstName property on the model is "Geert"
            Assert.That(PropertyHelper.GetPropertyValue<string>(viewModel, "FirstName"), Is.EqualTo("Geert"));

            // Default value of the LastName property on the model is "Geert"
            Assert.That(PropertyHelper.GetPropertyValue<string>(viewModel, "MappedLastName"), Is.EqualTo("van Horrik"));
        }

        [TestCase]
        public void CreatesExposedPropertiesFromExternalTypes()
        {
            var modelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ExposingDerivedModel");
            var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ExposingViewModel");

            var model = Activator.CreateInstance(modelType);
            var viewModel = Activator.CreateInstance(viewModelType, new [] { model });

            Assert.That(PropertyDataManager.Default.IsPropertyRegistered(viewModelType, "IsOk"), Is.True);

            // Default value of the ExternalTypeProperty property on the model is "null"
            Assert.That(PropertyHelper.GetPropertyValue<bool?>(viewModel, "IsOk"), Is.EqualTo(null));
        }

        [TestCase]
        public void CanCreateReadOnlyExposedProperties()
        {
            var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ExposingViewModel");

            var propertyInfo = viewModelType.GetPropertyEx("ReadOnlyProperty");

            var setMethod = propertyInfo.GetSetMethod(true);
            if (setMethod is not null)
            {
                Assert.That(setMethod.IsPublic, Is.False);
            }
        }
    }
}

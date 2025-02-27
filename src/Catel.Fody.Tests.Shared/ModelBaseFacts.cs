namespace Catel.Fody.Tests
{
    using System;
    using System.Collections.ObjectModel;
    using Data;
    using Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class ModelBaseFacts
    {
        [TestCase]
        public void StringsCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);
            obj.Name = "hi there";
            Assert.That("hi there", Is.EqualTo(obj.Name));
        }

        [TestCase]
        public void BooleansCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            Assert.That(obj.BoolValue, Is.False);
            obj.BoolValue = true;
            Assert.That(obj.BoolValue, Is.True);
        }

        [TestCase]
        public void IntegersCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            Assert.That(0, Is.EqualTo(obj.IntValue));
            obj.IntValue = 42;
            Assert.That(42, Is.EqualTo(obj.IntValue));
        }

        [TestCase]
        public void GuidsCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            Assert.That(Guid.Empty, Is.EqualTo(obj.GuidValue));
            obj.GuidValue = Guid.NewGuid();
            Assert.That(Guid.Empty, Is.Not.EqualTo(obj.GuidValue));
        }

        [TestCase]
        public void CollectionsCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            obj.CollectionProperty.Add(1);

            Assert.That(1, Is.EqualTo(obj.CollectionProperty.Count));
            Assert.That(1, Is.EqualTo(obj.CollectionProperty[0]));
        }

        [TestCase]
        public void DoesNotWeaveExistingProperties()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            obj.FullName = "hi there";

            Assert.That("hi there", Is.EqualTo(obj.FullName));
        }

        [TestCase]
        public void IgnoresPropertiesWithoutBackingField()
        {
            // TODO: test Title property
        }

        [TestCase]
        public void HandlesChangeNotificationsMethodsCorrectly()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var modelBase = (dynamic) Activator.CreateInstance(type);

            Assert.That(modelBase.OnFullNameWithChangeCallbackChangedCalled, Is.False);
            modelBase.FullNameWithChangeCallback = "change";
            Assert.That(modelBase.OnFullNameWithChangeCallbackChangedCalled, Is.True);

            Assert.That(modelBase.OnAnotherPropertyWithChangeCallbackChangedCalled, Is.False);
            modelBase.AnotherPropertyWithChangeCallback = "change";
            Assert.That(modelBase.OnAnotherPropertyWithChangeCallbackChangedCalled, Is.True);

            Assert.That(modelBase.OnLastNameChangedCalled, Is.False);
            modelBase.LastName = "change";
            Assert.That(modelBase.OnLastNameChangedCalled, Is.True);
        }

        [TestCase]
        public void IgnoresChangeNotificationsWithoutRightSignature()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var modelBase = (dynamic)Activator.CreateInstance(type);

            Assert.That(modelBase.OnLastNameChangedCalled, Is.False);
            modelBase.LastName = "change";
            Assert.That(modelBase.OnLastNameChangedCalled, Is.True);
        }

        [TestCase]
        public void CorrectlyWorksOnGenericClasses()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GenericModelBaseTest");
            var model = (dynamic)Activator.CreateInstance(type);

            Assert.That(PropertyDataManager.Default.IsPropertyRegistered(type, "Operations"), Is.True);

            model.Operations = new ObservableCollection<int>();

            Assert.That(model.HasChangedNotificationBeenCalled, Is.True);
        }

        [TestCase]
        public void CorrectlyWorksOnClassesWithGenericModelsWithValueTypes()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GenericPropertyModelAsInt");
            var model = Activator.CreateInstance(type);

            var propertyNameToCheck = "MyModel";

            Assert.That(PropertyDataManager.Default.IsPropertyRegistered(type, propertyNameToCheck), Is.True);

            PropertyHelper.SetPropertyValue(model, propertyNameToCheck, 42);

            Assert.That(PropertyHelper.GetPropertyValue<int>(model, propertyNameToCheck), Is.EqualTo(42));
        }

        [TestCase]
        public void CorrectlyWorksOnClassesWithGenericModelsWithReferenceTypes()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GenericPropertyModelAsObject");
            var model = Activator.CreateInstance(type);

            var propertyNameToCheck = "MyModel";

            Assert.That(PropertyDataManager.Default.IsPropertyRegistered(type, propertyNameToCheck), Is.True);

            var tempObject = new object();

            PropertyHelper.SetPropertyValue(model, propertyNameToCheck, tempObject);

            Assert.That(PropertyHelper.GetPropertyValue<object>(model, propertyNameToCheck), Is.EqualTo(tempObject));
        }
    }
}

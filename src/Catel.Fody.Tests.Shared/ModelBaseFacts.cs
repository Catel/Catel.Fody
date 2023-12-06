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
            Assert.AreEqual("hi there", obj.Name);
        }

        [TestCase]
        public void BooleansCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            Assert.IsFalse(obj.BoolValue);
            obj.BoolValue = true;
            Assert.IsTrue(obj.BoolValue);
        }

        [TestCase]
        public void IntegersCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            Assert.AreEqual(0, obj.IntValue);
            obj.IntValue = 42;
            Assert.AreEqual(42, obj.IntValue);
        }

        [TestCase]
        public void GuidsCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            Assert.AreEqual(Guid.Empty, obj.GuidValue);
            obj.GuidValue = Guid.NewGuid();
            Assert.AreNotEqual(Guid.Empty, obj.GuidValue);
        }

        [TestCase]
        public void CollectionsCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            obj.CollectionProperty.Add(1);

            Assert.AreEqual(1, obj.CollectionProperty.Count);
            Assert.AreEqual(1, obj.CollectionProperty[0]);
        }

        [TestCase]
        public void DoesNotWeaveExistingProperties()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            obj.FullName = "hi there";

            Assert.AreEqual("hi there", obj.FullName);
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

            Assert.IsFalse(modelBase.OnFullNameWithChangeCallbackChangedCalled);
            modelBase.FullNameWithChangeCallback = "change";
            Assert.IsTrue(modelBase.OnFullNameWithChangeCallbackChangedCalled);

            Assert.IsFalse(modelBase.OnAnotherPropertyWithChangeCallbackChangedCalled);
            modelBase.AnotherPropertyWithChangeCallback = "change";
            Assert.IsTrue(modelBase.OnAnotherPropertyWithChangeCallbackChangedCalled);

            Assert.IsFalse(modelBase.OnLastNameChangedCalled);
            modelBase.LastName = "change";
            Assert.IsTrue(modelBase.OnLastNameChangedCalled);
        }

        [TestCase]
        public void IgnoresChangeNotificationsWithoutRightSignature()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var modelBase = (dynamic)Activator.CreateInstance(type);

            Assert.IsFalse(modelBase.OnLastNameChangedCalled);
            modelBase.LastName = "change";
            Assert.IsTrue(modelBase.OnLastNameChangedCalled);
        }

        [TestCase]
        public void CorrectlyWorksOnGenericClasses()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GenericModelBaseTest");
            var model = (dynamic)Activator.CreateInstance(type);

            Assert.That(PropertyDataManager.Default.IsPropertyRegistered(type, "Operations"), Is.True);

            model.Operations = new ObservableCollection<int>();

            Assert.IsTrue(model.HasChangedNotificationBeenCalled);
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

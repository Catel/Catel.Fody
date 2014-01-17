// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelBaseFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
    using System;
    using System.Collections.ObjectModel;
    using Catel.Data;
    using Catel.Fody.TestAssembly;
    using Catel.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ModelBaseFacts
    {
        #region Methods
        [TestMethod]
        public void StringsCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);
            obj.Name = "hi there";
            Assert.AreEqual("hi there", obj.Name);
        }

        [TestMethod]
        public void BooleansCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            Assert.IsFalse(obj.BoolValue);
            obj.BoolValue = true;
            Assert.IsTrue(obj.BoolValue);
        }

        [TestMethod]
        public void IntegersCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            Assert.AreEqual(0, obj.IntValue);
            obj.IntValue = 42;
            Assert.AreEqual(42, obj.IntValue);
        }

        [TestMethod]
        public void GuidsCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            Assert.AreEqual(Guid.Empty, obj.GuidValue);
            obj.GuidValue = Guid.NewGuid();
            Assert.AreNotEqual(Guid.Empty, obj.GuidValue);
        }

        [TestMethod]
        public void CollectionsCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            obj.CollectionProperty.Add(1);

            Assert.AreEqual(1, obj.CollectionProperty.Count);
            Assert.AreEqual(1, obj.CollectionProperty[0]);
        }

        [TestMethod]
        public void DoesNotWeaveExistingProperties()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var obj = (dynamic) Activator.CreateInstance(type);

            obj.FullName = "hi there";

            Assert.AreEqual("hi there", obj.FullName);
        }

        [TestMethod]
        public void IgnoresPropertiesWithoutBackingField()
        {
            // TODO: test Title property
        }

        [TestMethod]
        public void HandlesChangeNotificationsMethodsCorrectly()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
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

        [TestMethod]
        public void IgnoresChangeNotificationsWithoutRightSignature()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
            var modelBase = (dynamic)Activator.CreateInstance(type);

            Assert.IsFalse(modelBase.OnLastNameChangedCalled);
            modelBase.LastName = "change";
            Assert.IsTrue(modelBase.OnLastNameChangedCalled);
        }

        [TestMethod]
        public void CorrectlyWorksOnGenericClasses()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.GenericModelBaseTest");
            var model = (dynamic)Activator.CreateInstance(type);

            Assert.IsTrue(PropertyDataManager.Default.IsPropertyRegistered(type, "Operations"));

            model.Operations = new ObservableCollection<int>();

            Assert.IsTrue(model.HasChangedNotificationBeenCalled);
        }

        //[TestMethod]
        //public void CorrectlyWorksOnClassesWithGenericModels()
        //{
        //    var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.GenericPropertyModelAsInt");
        //    var model = Activator.CreateInstance(type);

        //    Assert.IsTrue(PropertyDataManager.Default.IsPropertyRegistered(type, "MyModel"));

        //    PropertyHelper.SetPropertyValue(model, "MyModel", 42);

        //    Assert.AreEqual(42, PropertyHelper.GetPropertyValue<int>(model, "MyModel"));
        //}
        #endregion
    }
}
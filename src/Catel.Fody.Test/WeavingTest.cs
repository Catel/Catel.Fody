// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WeavingTest.cs" company="Catel development team">
//   Copyright (c) 2008 - 2012 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class WeavingFacts
    {
        // TODO: Allow attribute ignoring

        [TestClass]
        public class ModelBase
        {
            [TestMethod]
            public void StringsCanBeUsedAfterWeaving()
            {
	            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
				var obj = (dynamic)Activator.CreateInstance(type);
                obj.Name = "hi there";
                Assert.AreEqual("hi there", obj.Name);                
            }

            [TestMethod]
            public void BooleansCanBeUsedAfterWeaving()
			{
				var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
				var obj = (dynamic)Activator.CreateInstance(type);

                Assert.IsFalse(obj.BoolValue);
                obj.BoolValue = true;
                Assert.IsTrue(obj.BoolValue);
            }

            [TestMethod]
            public void IntegersCanBeUsedAfterWeaving()
			{
				var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
				var obj = (dynamic)Activator.CreateInstance(type);

                Assert.AreEqual(0, obj.IntValue);
                obj.IntValue = 42;
                Assert.AreEqual(42, obj.IntValue);
            }

            [TestMethod]
            public void GuidsCanBeUsedAfterWeaving()
			{
				var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
				var obj = (dynamic)Activator.CreateInstance(type);

                Assert.AreEqual(Guid.Empty, obj.GuidValue);
                obj.GuidValue = Guid.NewGuid();
                Assert.AreNotEqual(Guid.Empty, obj.GuidValue);
            }

            [TestMethod]
            public void CollectionsCanBeUsedAfterWeaving()
			{
				var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
				var obj = (dynamic)Activator.CreateInstance(type);

                obj.CollectionProperty.Add(1);

                Assert.AreEqual(1, obj.CollectionProperty.Count);
                Assert.AreEqual(1, obj.CollectionProperty[0]);
            }

            [TestMethod]
            public void DoesNotWeaveExistingProperties()
			{
				var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelBaseTest");
				var obj = (dynamic)Activator.CreateInstance(type);

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
				var modelBase = (dynamic)Activator.CreateInstance(type);

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
        }

        [TestClass]
        public class ViewModelBase
        {
            [TestMethod]
            public void StringsCanBeUsedAfterWeaving()
			{
				var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ViewModelBaseTest");
				var vm = (dynamic)Activator.CreateInstance(type);

                vm.Name = "hi there";

                Assert.AreEqual("hi there", vm.Name);
            }

            [TestMethod]
            public void IgnoresICommandProperties()
			{
				var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ViewModelBaseTest");
				var vm = (dynamic)Activator.CreateInstance(type);

                // TODO: Test command by setting it and check if property changed is invoked
            }

            [TestMethod]
            public void IgnoresPropertiesWithoutBackingField()
            {
                // tODO: test Title property
            }
        }

        [TestClass]
        public class Inheritance
        {
            [TestMethod]
            public void InheritanceWorks()
			{
				var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.InheritedClass");
				var obj = (dynamic)Activator.CreateInstance(type);

                obj.PropertyOnBase = "base prop";
                obj.PropertyOnInherited = "inherited prop";

                Assert.AreEqual("base prop", obj.PropertyOnBase);
                Assert.AreEqual("inherited prop", obj.PropertyOnInherited);
            }
        }
    }
}
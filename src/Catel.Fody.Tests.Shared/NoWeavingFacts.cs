namespace Catel.Fody.Tests
{
    using System;
    using Data;
    using NUnit.Framework;

    [TestFixture]
    public class NoWeavingFacts
    {
        [TestCase]
        public void IgnoresTypesWithNoWeavingAttribute()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.NoWeavingModelTest");

            Assert.That(PropertyDataManager.Default.IsPropertyRegistered(type, "FirstName"), Is.False);
        }

        [TestCase]
        public void IgnoresPropertiesWithNoWeavingAttribute()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.NoPropertyWeavingModelTest");

            Assert.That(PropertyDataManager.Default.IsPropertyRegistered(type, "FirstName"), Is.False);
        }

        [TestCase]
        public void SuppressesWarningForChangeCallbackWithParametersDecoratedWithNoWeavingAttribute()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.NoWeavingChangeCallbackWithParametersViewModel");
            var vm = (dynamic)Activator.CreateInstance(type);

            vm.SelectedItem = new object();

            Assert.That((bool)vm.WasCallbackInvoked, Is.True);
        }
    }
}
namespace Catel.Fody.Tests
{
    using System;
    using System.ComponentModel;
    using NUnit.Framework;
    using Reflection;

    [TestFixture]
    public class CTL569
    {
        [TestCase]
        public void WeavingCalculatedPropertiesWithExistingOverride()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CTL569_ViewModel");

            var vm = (INotifyPropertyChanged)Activator.CreateInstance(type);

            Assert.That(PropertyHelper.GetPropertyValue<bool>(vm, "SearchIsEnabled"), Is.False);

            var hasChanged = false;
            vm.PropertyChanged += (sender, e) =>
            {
                if (string.Equals(e.PropertyName, "SearchIsEnabled"))
                {
                    hasChanged = true;
                }
            };

            PropertyHelper.SetPropertyValue(vm, "SearchTerms", "testvalue");

            Assert.That(hasChanged, Is.False);
        }
    }
}
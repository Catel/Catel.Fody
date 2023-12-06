namespace Catel.Fody.Tests.Repros
{
    using System;
    using System.Collections.Generic;
    using Catel.Data;
    using Catel.Fody.TestAssembly.Bugs.GH0291;
    using NUnit.Framework;

    [TestFixture]
    public class GH0291TestFixture
    {
        [TestCase]
        public void WeavingVirtualPropertiesInPropertyChanged()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0291.Person");
            var instance = Activator.CreateInstance(type) as dynamic;

            Assert.IsNotNull(instance);

            var changedProperties = new List<string>();

            ((ModelBase)instance).PropertyChanged += (sender, e) =>
            {
                changedProperties.Add(e.PropertyName);
            };

            var addressType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0291.Address");
            var addressInstance = Activator.CreateInstance(addressType) as dynamic;
            instance.Address = addressInstance; // virtual
            instance.Address2 = addressInstance; // non-virtual

            Assert.That(changedProperties.Contains("Address"), Is.True);
            Assert.That(changedProperties.Contains("Address2"), Is.True);
            Assert.That(changedProperties.Contains("ComplAddress"), Is.True);
            Assert.That(changedProperties.Contains("ComplAddress2"), Is.True);
        }
    }
}

namespace Catel.Fody.Tests
{
    using System;
    using System.ComponentModel;
    using NUnit.Framework;

    [TestFixture]
    public class CTL768
    {
        [TestCase]
        public void WeavingConstructors()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CTL768_Model");

            var model = (INotifyPropertyChanged) Activator.CreateInstance(type, new [] { "test" });

            Assert.That(model, Is.Not.Null);
        }
    }
}

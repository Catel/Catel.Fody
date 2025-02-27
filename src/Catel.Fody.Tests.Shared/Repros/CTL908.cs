namespace Catel.Fody.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class CTL908TestFixture
    {
        [TestCase]
        public void WeavingConstructorWithString()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CTL908");

            var model = Activator.CreateInstance(type, new[] {"test"});

            Assert.That(model, Is.Not.Null);
        }

        [TestCase]
        public void WeavingConstructorWithObject()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CTL908");

            var model = Activator.CreateInstance(type, new[] { new object() });

            Assert.That(model, Is.Not.Null);
        }
    }
}

namespace Catel.Fody.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class CSharp6Facts
    {
        [TestCase]
        public void AutoPropertyInitializer()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CSharp6_AutoPropertyInitializer");
            var obj = (dynamic)Activator.CreateInstance(type);

            Assert.That(obj.SimpleModels, Is.Not.Null);
        }

        [TestCase]
        public void AutoPropertyInitializerWithMultipleConstructors()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CSharp6_AutoPropertyInitializerWithMultipleConstructors");
            var obj = (dynamic)Activator.CreateInstance(type);

            Assert.That(obj.ShowErrors, Is.True);
        }

        [TestCase]
        public void AutoPropertyInitializer_Generic()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CSharp6_AutoPropertyInitializer_Generic");
            var obj = (dynamic)Activator.CreateInstance(type);

            Assert.That(obj.SimpleModels, Is.Not.Null);
            Assert.That(obj.SelectedItem, Is.Null);
            Assert.That(obj.AdditionalProperty, Is.Null);
        }
    }
}

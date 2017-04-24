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

            Assert.IsNotNull(obj.SimpleModels);
        }

        [TestCase]
        public void AutoPropertyInitializer_Generic()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CSharp6_AutoPropertyInitializer_Generic");
            var obj = (dynamic)Activator.CreateInstance(type);

            Assert.IsNotNull(obj.SimpleModels);
            Assert.IsNull(obj.SelectedItem);
            Assert.IsNull(obj.AdditionalProperty);
        }
    }
}
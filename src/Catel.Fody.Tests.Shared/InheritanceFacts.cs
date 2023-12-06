namespace Catel.Fody.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class Inheritance
    {
        [TestCase]
        public void InheritanceWorks()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.InheritedClass");
            var obj = (dynamic) Activator.CreateInstance(type);

            obj.PropertyOnBase = "base prop";
            obj.PropertyOnInherited = "inherited prop";

            Assert.AreEqual("base prop", obj.PropertyOnBase);
            Assert.AreEqual("inherited prop", obj.PropertyOnInherited);
        }
    }
}

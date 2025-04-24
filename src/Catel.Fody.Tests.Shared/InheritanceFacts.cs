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

            Assert.That("base prop", Is.EqualTo(obj.PropertyOnBase));
            Assert.That("inherited prop", Is.EqualTo(obj.PropertyOnInherited));
        }
    }
}

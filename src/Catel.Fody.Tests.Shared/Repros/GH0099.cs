namespace Catel.Fody.Tests.Repros
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class GH0099TestFixture
    {
        [TestCase]
        public void WeavingArgumentCheckForUnusedArgument()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0099.TestViewModel");
            var instance = Activator.CreateInstance(type) as dynamic;

            Assert.That(instance, Is.Not.Null);

            var idProperty = type.GetProperty("ID");

            Assert.That(idProperty.PropertyType, Is.EqualTo(typeof(int)));
        }
    }
}

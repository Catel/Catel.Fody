namespace Catel.Fody.Tests.Repros
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class GH0021TestFixture
    {
        [TestCase]
        public void WeavingArgumentCheckForUnusedArgument()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0021.MyDerivedService");
            var service = Activator.CreateInstance(type) as dynamic;

            Assert.Throws<ArgumentNullException>(() => service.MyMethod(null, null));
        }
    }
}
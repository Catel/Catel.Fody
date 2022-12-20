namespace Catel.Fody.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class CTL504
    {
        [TestCase]
        public void PropertyWeavingDoesNotThrowException()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CTL504_Model");

            var model = Activator.CreateInstance(type);
        }
    }
}
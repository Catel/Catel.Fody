namespace Catel.Fody.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class LoggingFacts
    {
        [TestCase]
        public void InheritanceWorks()
        {
            // Instantiating is sufficient
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.LoggingClass");
            var obj = (dynamic)Activator.CreateInstance(type);
        }
    }
}
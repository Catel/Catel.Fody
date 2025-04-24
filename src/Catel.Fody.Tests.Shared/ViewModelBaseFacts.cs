namespace Catel.Fody.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class ViewModelBaseFacts
    {
        [TestCase]
        public void StringsCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ViewModelBaseTest");
            var vm = (dynamic) Activator.CreateInstance(type);

            vm.Name = "hi there";

            Assert.That("hi there", Is.EqualTo(vm.Name));
        }

        [TestCase]
        public void IgnoresICommandProperties()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ViewModelBaseTest");
            var vm = (dynamic) Activator.CreateInstance(type);

            // TODO: Test command by setting it and check if property changed is invoked
        }

        [TestCase]
        public void IgnoresPropertiesWithoutBackingField()
        {
            // tODO: test Title property
        }
    }
}

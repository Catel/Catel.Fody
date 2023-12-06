namespace Catel.Fody.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class GenericModelFacts
    {
        [TestCase]
        public void ImportedGenericTypeFromExternalAssemblyWorks()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GenericModel");
            var obj = (dynamic)Activator.CreateInstance(type);

            Assert.IsNull(obj.GenericProperty);
        }
    }
}

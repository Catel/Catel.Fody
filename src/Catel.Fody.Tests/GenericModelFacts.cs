namespace Catel.Fody.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class GenericModelFacts
    {
        #region Methods
        [TestCase]
        public void ImportedGenericTypeFromExternalAssemblyWorks()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.GenericModel");
            var obj = (dynamic)Activator.CreateInstance(type);

            Assert.IsNull(obj.GenericProperty);
        }
        #endregion
    }
}
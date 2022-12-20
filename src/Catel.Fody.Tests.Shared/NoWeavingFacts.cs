namespace Catel.Fody.Tests
{
    using Data;
    using NUnit.Framework;

    [TestFixture]
    public class NoWeavingFacts
    {
        [TestCase]
        public void IgnoresTypesWithNoWeavingAttribute()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.NoWeavingModelTest");

            Assert.IsFalse(PropertyDataManager.Default.IsPropertyRegistered(type, "FirstName"));
        }

        [TestCase]
        public void IgnoresPropertiesWithNoWeavingAttribute()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.NoPropertyWeavingModelTest");

            Assert.IsFalse(PropertyDataManager.Default.IsPropertyRegistered(type, "FirstName"));
        }
    }
}
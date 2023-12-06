namespace Catel.Fody.Tests
{
    using System;
    using Data;
    using NUnit.Framework;
    using TestAssembly;

    [TestFixture]
    public class DefaultValueFacts
    {
        [TestCase]
        public void SetsNullAsDefaultValueWhenNoAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "LastName");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(null));
        }

        [TestCase]
        public void SetsDefaultValueForStringWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "FirstName");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo("Geert"));
        }

        [TestCase]
        public void SetsDefaultValueForBoolWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "BoolValue");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(true));
        }

        [TestCase]
        public void SetsDefaultValueForNullableBoolWhenNullAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "NullableBoolDefaultNullValue");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(null));
        }

        [TestCase]
        public void SetsDefaultValueForNullableBoolWhenTrueAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "NullableBoolDefaultTrueValue");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(true));
        }

        [TestCase]
        public void SetsDefaultValueForNullableBoolWhenFalseAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "NullableBoolDefaultFalseValue");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(false));
        }

        [TestCase]
        public void SetsDefaultValueForNullableIntWhenNullAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "NullableIntDefaultNullValueCatel");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(null));
        }

        [TestCase]
        public void SetsDefaultValueForNullableIntWhen0AttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "NullableIntDefault0ValueCatel");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(0));
        }

        [TestCase]
        public void SetsDefaultValueForNullableIntWhen1AttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "NullableIntDefault1ValueCatel");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(1));
        }

        [TestCase]
        public void SetsDefaultValueForIntWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "IntValue");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(42));
        }

        [TestCase]
        public void SetsDefaultValueForLongWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "LongValue");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(42L));
        }

        [TestCase]
        public void SetsDefaultValueForDoubleWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "DoubleValue");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(42d));
        }

        [TestCase]
        public void SetsDefaultValueForFloatWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "FloatValue");
            Assert.That(propertyData.GetDefaultValue(), Is.EqualTo(42f));
        }

        [TestCase]
        public void SetsDefaultValueForEnumWhenAttributeDefined()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DefaultValueModel");

            // Instantiate to have properties registered
            Activator.CreateInstance(type);

            var propertyData = PropertyDataManager.Default.GetPropertyData(type, "EnumValue");
            Assert.That((ExampleEnum)propertyData.GetDefaultValue(), Is.EqualTo(ExampleEnum.B));
        }
    }
}

namespace Catel.Fody.Tests
{
    using System;
    using NUnit.Framework;

    /// <summary>
    /// Required to reproduce https://catelproject.atlassian.net/browse/CTL-237.
    /// </summary>
    [TestFixture]
    public class ModelWithDoubleValuesFacts
    {
        [TestCase]
        public void CorrectlyDefaultsToDefaultDoubleValues()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ModelWithDoubleValues");
            var obj = (dynamic)Activator.CreateInstance(type);

            Assert.AreEqual(0.0d, obj.Top);
            Assert.AreEqual(0.0d, obj.Left);
            Assert.AreEqual(0.0d, obj.Width);
        }
    }
}
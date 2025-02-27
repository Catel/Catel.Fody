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

            Assert.That(0.0d, Is.EqualTo(obj.Top));
            Assert.That(0.0d, Is.EqualTo(obj.Left));
            Assert.That(0.0d, Is.EqualTo(obj.Width));
        }
    }
}

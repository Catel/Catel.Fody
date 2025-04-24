namespace Catel.Fody.Tests
{
    using System.Xml.Linq;
    using Fody;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigurationFacts
    {
        [TestCase]
        public void CanReadFalseNode()
        {
            var xElement = XElement.Parse("<Node attr='false'/>");

            xElement.ReadBool("attr", b => Assert.That(b, Is.False));
        }

        [TestCase]
        public void CanReadTrueNode()
        {
            var xElement = XElement.Parse("<Node attr='true'/>");

            xElement.ReadBool("attr", b => Assert.That(b, Is.True));
        }

        [TestCase]
        public void DoesNotReadInvalidBoolNode()
        {
            var xElement = XElement.Parse("<Node attr='foo'/>");

            Assert.Throws<WeavingException>(() => xElement.ReadBool("attr", b => Assert.Fail()));
        }

        [TestCase]
        public void DoesNotSetBoolWhenNodeMissing()
        {
            var xElement = XElement.Parse("<Node attr='false'/>");

            xElement.ReadBool("missing", b => Assert.Fail());
        }

        [TestCase]
        public void CorrectlyReadsWeaveProperties()
        {
            var falseElement = XElement.Parse("<Node WeaveProperties='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.That(falseConfiguration.WeaveProperties, Is.False);

            var trueElement = XElement.Parse("<Node WeaveProperties='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.That(trueConfiguration.WeaveProperties, Is.True);
        }

        [TestCase]
        public void CorrectlyReadsWeaveExposedProperties()
        {
            var falseElement = XElement.Parse("<Node WeaveExposedProperties='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.That(falseConfiguration.WeaveExposedProperties, Is.False);

            var trueElement = XElement.Parse("<Node WeaveExposedProperties='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.That(trueConfiguration.WeaveExposedProperties, Is.True);
        }

        [TestCase]
        public void CorrectlyReadsWeaveArguments()
        {
            var falseElement = XElement.Parse("<Node WeaveArguments='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.That(falseConfiguration.WeaveArguments, Is.False);

            var trueElement = XElement.Parse("<Node WeaveArguments='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.That(trueConfiguration.WeaveArguments, Is.True);
        }

        [TestCase]
        public void CorrectlyReadsWeaveLogging()
        {
            var falseElement = XElement.Parse("<Node WeaveLogging='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.That(falseConfiguration.WeaveLogging, Is.False);

            var trueElement = XElement.Parse("<Node WeaveLogging='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.That(trueConfiguration.WeaveLogging, Is.True);
        }

        [TestCase]
        public void CorrectlyReadsGenerateXmlSchemas()
        {
            var falseElement = XElement.Parse("<Node GenerateXmlSchemas='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.That(falseConfiguration.GenerateXmlSchemas, Is.False);

            var trueElement = XElement.Parse("<Node GenerateXmlSchemas='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.That(trueConfiguration.GenerateXmlSchemas, Is.True);
        }

        [TestCase]
        public void CorrectlyReadsGeneratedPropertyDataAccessibility()
        {
            var publicElement = XElement.Parse("<Node GeneratedPropertyDataAccessibility='Public' />");
            var publicConfiguration = new Configuration(publicElement);

            Assert.That(publicConfiguration.GeneratedPropertyDataAccessibility, Is.EqualTo(Accessibility.Public));

            var internalElement = XElement.Parse("<Node GeneratedPropertyDataAccessibility='Internal' />");
            var internalConfiguration = new Configuration(internalElement);

            Assert.That(internalConfiguration.GeneratedPropertyDataAccessibility, Is.EqualTo(Accessibility.Internal));

            var privateElement = XElement.Parse("<Node GeneratedPropertyDataAccessibility='Private' />");
            var privateConfiguration = new Configuration(privateElement);

            Assert.That(privateConfiguration.GeneratedPropertyDataAccessibility, Is.EqualTo(Accessibility.Private));
        }

        [TestCase]
        public void CorrectlyReadsValues()
        {
            var configuration = new Configuration(null);

            Assert.That(configuration.WeaveProperties, Is.True);
            Assert.That(configuration.WeaveExposedProperties, Is.True);
            Assert.That(configuration.WeaveArguments, Is.True);
            Assert.That(configuration.WeaveLogging, Is.True);
            Assert.That(configuration.GenerateXmlSchemas, Is.False);
        }
    }
}

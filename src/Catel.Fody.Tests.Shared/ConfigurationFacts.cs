// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationFacts.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


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

            xElement.ReadBool("attr", b => Assert.IsFalse(b));
        }

        [TestCase]
        public void CanReadTrueNode()
        {
            var xElement = XElement.Parse("<Node attr='true'/>");

            xElement.ReadBool("attr", b => Assert.IsTrue(b));
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

            Assert.IsFalse(falseConfiguration.WeaveProperties);

            var trueElement = XElement.Parse("<Node WeaveProperties='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.IsTrue(trueConfiguration.WeaveProperties);
        }

        [TestCase]
        public void CorrectlyReadsWeaveExposedProperties()
        {
            var falseElement = XElement.Parse("<Node WeaveExposedProperties='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.IsFalse(falseConfiguration.WeaveExposedProperties);

            var trueElement = XElement.Parse("<Node WeaveExposedProperties='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.IsTrue(trueConfiguration.WeaveExposedProperties);
        }

        [TestCase]
        public void CorrectlyReadsWeaveArguments()
        {
            var falseElement = XElement.Parse("<Node WeaveArguments='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.IsFalse(falseConfiguration.WeaveArguments);

            var trueElement = XElement.Parse("<Node WeaveArguments='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.IsTrue(trueConfiguration.WeaveArguments);
        }

        [TestCase]
        public void CorrectlyReadsWeaveLogging()
        {
            var falseElement = XElement.Parse("<Node WeaveLogging='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.IsFalse(falseConfiguration.WeaveLogging);

            var trueElement = XElement.Parse("<Node WeaveLogging='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.IsTrue(trueConfiguration.WeaveLogging);
        }

        [TestCase]
        public void CorrectlyReadsGenerateXmlSchemas()
        {
            var falseElement = XElement.Parse("<Node GenerateXmlSchemas='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.IsFalse(falseConfiguration.GenerateXmlSchemas);

            var trueElement = XElement.Parse("<Node GenerateXmlSchemas='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.IsTrue(trueConfiguration.GenerateXmlSchemas);
        }

        [TestCase]
        public void CorrectlyReadsGeneratedPropertyDataAccessibility()
        {
            var publicElement = XElement.Parse("<Node GeneratedPropertyDataAccessibility='Public' />");
            var publicConfiguration = new Configuration(publicElement);

            Assert.AreEqual(Accessibility.Public, publicConfiguration.GeneratedPropertyDataAccessibility);

            var internalElement = XElement.Parse("<Node GeneratedPropertyDataAccessibility='Internal' />");
            var internalConfiguration = new Configuration(internalElement);

            Assert.AreEqual(Accessibility.Internal, internalConfiguration.GeneratedPropertyDataAccessibility);

            var privateElement = XElement.Parse("<Node GeneratedPropertyDataAccessibility='Private' />");
            var privateConfiguration = new Configuration(privateElement);

            Assert.AreEqual(Accessibility.Private, privateConfiguration.GeneratedPropertyDataAccessibility);
        }

        [TestCase]
        public void CorrectlyReadsValues()
        {
            var configuration = new Configuration(null);

            Assert.IsTrue(configuration.WeaveProperties);
            Assert.IsTrue(configuration.WeaveExposedProperties);
            Assert.IsTrue(configuration.WeaveArguments);
            Assert.IsTrue(configuration.WeaveLogging);
            Assert.IsFalse(configuration.GenerateXmlSchemas);
        }
    }
}

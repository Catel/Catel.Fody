// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationFacts.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
    using System.Xml.Linq;
    using Catel.Test;
    using Fody;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigurationFacts
    {
        [TestMethod]
        public void CanReadFalseNode()
        {
            var xElement = XElement.Parse(@"<Node attr='false'/>");

            xElement.ReadBool("attr", b => Assert.IsFalse(b));
        }

        [TestMethod]
        public void CanReadTrueNode()
        {
            var xElement = XElement.Parse(@"<Node attr='true'/>");

            xElement.ReadBool("attr", b => Assert.IsTrue(b));
        }

        [TestMethod]
        public void DoesNotReadInvalidBoolNode()
        {
            var xElement = XElement.Parse(@"<Node attr='foo'/>");

            ExceptionTester.CallMethodAndExpectException<WeavingException>(() => xElement.ReadBool("attr", b => Assert.Fail()));
        }

        [TestMethod]
        public void DoesNotSetBoolWhenNodeMissing()
        {
            var xElement = XElement.Parse(@"<Node attr='false'/>");

            xElement.ReadBool("missing", b => Assert.Fail());
        }

        [TestMethod]
        public void CorrectlyReadsWeaveProperties()
        {
            var falseElement = XElement.Parse(@"<Node WeaveProperties='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.IsFalse(falseConfiguration.WeaveProperties);

            var trueElement = XElement.Parse(@"<Node WeaveProperties='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.IsTrue(trueConfiguration.WeaveProperties);
        }

        [TestMethod]
        public void CorrectlyReadsWeaveExposedProperties()
        {
            var falseElement = XElement.Parse(@"<Node WeaveExposedProperties='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.IsFalse(falseConfiguration.WeaveExposedProperties);

            var trueElement = XElement.Parse(@"<Node WeaveExposedProperties='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.IsTrue(trueConfiguration.WeaveExposedProperties);
        }

        [TestMethod]
        public void CorrectlyReadsWeaveArguments()
        {
            var falseElement = XElement.Parse(@"<Node WeaveArguments='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.IsFalse(falseConfiguration.WeaveArguments);

            var trueElement = XElement.Parse(@"<Node WeaveArguments='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.IsTrue(trueConfiguration.WeaveArguments);
        }

        [TestMethod]
        public void CorrectlyReadsWeaveLogging()
        {
            var falseElement = XElement.Parse(@"<Node WeaveLogging='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.IsFalse(falseConfiguration.WeaveLogging);

            var trueElement = XElement.Parse(@"<Node WeaveLogging='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.IsTrue(trueConfiguration.WeaveLogging);
        }

        [TestMethod]
        public void CorrectlyReadsGenerateXmlSchemas()
        {
            var falseElement = XElement.Parse(@"<Node GenerateXmlSchemas='false' />");
            var falseConfiguration = new Configuration(falseElement);

            Assert.IsFalse(falseConfiguration.GenerateXmlSchemas);

            var trueElement = XElement.Parse(@"<Node GenerateXmlSchemas='true' />");
            var trueConfiguration = new Configuration(trueElement);

            Assert.IsTrue(trueConfiguration.GenerateXmlSchemas);
        }

        [TestMethod]
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
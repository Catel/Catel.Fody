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
    }
}
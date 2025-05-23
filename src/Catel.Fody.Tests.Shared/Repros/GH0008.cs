﻿namespace Catel.Fody.Tests
{
    using System;
    using Catel.Services;
    using Data;
    using NUnit.Framework;

    [TestFixture]
    public class GH0008TestFixture
    {
        [TestCase]
        public void WeavingConstructorWithValidationContext()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GH0008");

            var model = Activator.CreateInstance(type, new object[] { new ValidationContext(), new ProcessService() });

            Assert.That(model, Is.Not.Null);
        }

        [TestCase]
        public void WeavingConstructorWithoutValidationContext()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GH0008");

            var model = Activator.CreateInstance(type, new object[] { new ProcessService() });

            Assert.That(model, Is.Not.Null);
        }
    }
}

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CTL908.cs" company="Catel development team">
//   Copyright (c) 2008 - 2016 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Tests
{
    using System;
    using System.ComponentModel;
    using NUnit.Framework;

    [TestFixture]
    public class CTL908TestFixture
    {
        [TestCase]
        public void WeavingConstructorWithString()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CTL908");

            var model = Activator.CreateInstance(type, new[] {"test"});

            Assert.IsNotNull(model);
        }

        [TestCase]
        public void WeavingConstructorWithObject()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CTL908");

            var model = Activator.CreateInstance(type, new[] { new object() });

            Assert.IsNotNull(model);
        }
    }
}
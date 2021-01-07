// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CTL908.cs" company="Catel development team">
//   Copyright (c) 2008 - 2016 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Tests
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

            Assert.IsNotNull(model);
        }

        [TestCase]
        public void WeavingConstructorWithoutValidationContext()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GH0008");

            var model = Activator.CreateInstance(type, new object[] { new ProcessService() });

            Assert.IsNotNull(model);
        }
    }
}

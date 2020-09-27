// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GH0021.cs" company="Catel development team">
//   Copyright (c) 2008 - 2018 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Tests.Repros
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class GH0021TestFixture
    {
        [TestCase]
        public void WeavingArgumentCheckForUnusedArgument()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0021.MyDerivedService");
            var service = Activator.CreateInstance(type) as dynamic;

            Assert.Throws<ArgumentNullException>(() => service.MyMethod(null, null));
        }
    }
}
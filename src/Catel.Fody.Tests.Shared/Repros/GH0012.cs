﻿namespace Catel.Fody.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class GH0012TestFixture
    {
        [TestCase]
        public void WeavingArgumentCheckForUnusedArgument()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GH0012");
            var model = Activator.CreateInstance(type) as dynamic;

            model.a("123");
        }
    }
}

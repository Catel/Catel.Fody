// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggingFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class LoggingFacts
    {
        [TestCase]
        public void InheritanceWorks()
        {
            // Instantiating is sufficient
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.LoggingClass");
            var obj = (dynamic)Activator.CreateInstance(type);
        }
    }
}
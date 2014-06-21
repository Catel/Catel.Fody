// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggingFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LoggingFacts
    {
        [TestMethod]
        public void InheritanceWorks()
        {
            // Instantiating is sufficient
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.LoggingClass");
            var obj = (dynamic)Activator.CreateInstance(type);
        }
    }
}
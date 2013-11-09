// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InheritanceFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Inheritance
    {
        #region Methods
        [TestMethod]
        public void InheritanceWorks()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.InheritedClass");
            var obj = (dynamic) Activator.CreateInstance(type);

            obj.PropertyOnBase = "base prop";
            obj.PropertyOnInherited = "inherited prop";

            Assert.AreEqual("base prop", obj.PropertyOnBase);
            Assert.AreEqual("inherited prop", obj.PropertyOnInherited);
        }
        #endregion
    }
}
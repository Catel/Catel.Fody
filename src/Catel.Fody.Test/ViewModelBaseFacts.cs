// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModelBaseFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ViewModelBaseFacts
    {
        #region Methods
        [TestMethod]
        public void StringsCanBeUsedAfterWeaving()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ViewModelBaseTest");
            var vm = (dynamic) Activator.CreateInstance(type);

            vm.Name = "hi there";

            Assert.AreEqual("hi there", vm.Name);
        }

        [TestMethod]
        public void IgnoresICommandProperties()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ViewModelBaseTest");
            var vm = (dynamic) Activator.CreateInstance(type);

            // TODO: Test command by setting it and check if property changed is invoked
        }

        [TestMethod]
        public void IgnoresPropertiesWithoutBackingField()
        {
            // tODO: test Title property
        }
        #endregion
    }
}
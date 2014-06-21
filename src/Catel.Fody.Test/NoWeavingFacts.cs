// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoWeavingFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
    using Data;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NoWeavingFacts
    {
        [TestMethod]
        public void IgnoresTypesWithNoWeavingAttribute()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.NoWeavingModelTest");

            Assert.IsFalse(PropertyDataManager.Default.IsPropertyRegistered(type, "FirstName"));
        }

        [TestMethod]
        public void IgnoresPropertiesWithNoWeavingAttribute()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.NoPropertyWeavingModelTest");

            Assert.IsFalse(PropertyDataManager.Default.IsPropertyRegistered(type, "FirstName"));
        }
    }
}
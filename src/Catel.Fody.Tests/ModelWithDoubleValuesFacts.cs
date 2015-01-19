// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelWithDoubleValuesFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
    using System;
    using NUnit.Framework;

    /// <summary>
    /// Required to reproduce https://catelproject.atlassian.net/browse/CTL-237.
    /// </summary>
    [TestFixture]
    public class ModelWithDoubleValuesFacts
    {
        [TestCase]
        public void CorrectlyDefaultsToDefaultDoubleValues()
        {
            var type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.ModelWithDoubleValues");
            var obj = (dynamic)Activator.CreateInstance(type);

            Assert.AreEqual(0.0d, obj.Top);
            Assert.AreEqual(0.0d, obj.Left);
            Assert.AreEqual(0.0d, obj.Width);
        }
    }
}
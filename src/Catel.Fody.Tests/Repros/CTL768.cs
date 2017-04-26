﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CTL569.cs" company="Catel development team">
//   Copyright (c) 2008 - 2015 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Tests
{
    using System;
    using System.ComponentModel;
    using NUnit.Framework;

    [TestFixture]
    public class CTL768
    {
        [TestCase]
        public void WeavingConstructors()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CTL768_Model");

            var model = (INotifyPropertyChanged) Activator.CreateInstance(type, new [] { "test" });

            Assert.IsNotNull(model);
        }
    }
}
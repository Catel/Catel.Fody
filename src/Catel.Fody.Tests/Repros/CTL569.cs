// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CTL569.cs" company="Catel development team">
//   Copyright (c) 2008 - 2015 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Tests
{
    using System;
    using System.ComponentModel;
    using NUnit.Framework;
    using Reflection;

    [TestFixture]
    public class CTL569
    {
        [TestCase]
        public void WeavingCalculatedPropertiesWithExistingOverride()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CTL569_ViewModel");

            var vm = (INotifyPropertyChanged)Activator.CreateInstance(type);

            Assert.IsFalse(PropertyHelper.GetPropertyValue<bool>(vm, "SearchIsEnabled"));

            var hasChanged = false;
            vm.PropertyChanged += (sender, e) =>
            {
                if (string.Equals(e.PropertyName, "SearchIsEnabled"))
                {
                    hasChanged = true;
                }
            };

            PropertyHelper.SetPropertyValue(vm, "SearchTerms", "testvalue");

            Assert.IsFalse(hasChanged);
        }
    }
}
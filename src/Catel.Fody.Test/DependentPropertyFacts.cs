// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependentPropertyFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Test
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using NUnit.Framework;

    [TestFixture]
    public class DependentPropertyFacts
    {
        #region Methods
        [TestCase("Catel.Fody.TestAssembly.DependentPropertyModel", "FirstName", "Igr Alexander", "FullName")]
        [TestCase("Catel.Fody.TestAssembly.DependentPropertyModel", "LastName", "Fernández Saúco", "FullName")]
        [TestCase("Catel.Fody.TestAssembly.DependentPropertyModel", "MiddleName", "middleName", "FullName")]
        [TestCase("Catel.Fody.TestAssembly.DetailedDependentPropertyModel", "LastName", "Fernández Saúco", "Profile")]
        public void NotifiesPropertyChangedOfDepedentProperties(string modelType, string propertyName, string newValue, string expectedPropertyName)
        {
            var type = AssemblyWeaver.Assembly.GetType(modelType);
            var instance = Activator.CreateInstance(type);

            var changedProperties = new List<string>();
            ((INotifyPropertyChanged) instance).PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

            var propertyInfo = instance.GetType().GetProperty(propertyName);
            propertyInfo.SetValue(instance, newValue);

            Assert.IsTrue(changedProperties.Contains(expectedPropertyName));
        }
        #endregion
    }
}
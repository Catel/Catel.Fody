namespace Catel.Fody.Test
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DependentPropertyFacts
    {
        #region Methods
        [TestMethod]
        public void NotifiesPropertyChangedOfDepedentProperties1()
        {
            Type type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DependentPropertyModel");
            object instance = Activator.CreateInstance(type);

            var changedProperties = new List<string>();
            ((INotifyPropertyChanged)instance).PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

            PropertyInfo propertyInfo = instance.GetType().GetProperty("FirstName");
            propertyInfo.SetValue(instance, "Igr Alexander");

            Assert.IsTrue(changedProperties.Contains("FullName"));
        }

        [TestMethod]
        public void NotifiesPropertyChangedOfDepedentProperties2()
        {
            Type type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DependentPropertyModel");
            object instance = Activator.CreateInstance(type);

            var changedProperties = new List<string>();
            ((INotifyPropertyChanged)instance).PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

            PropertyInfo propertyInfo = instance.GetType().GetProperty("LastName");
            propertyInfo.SetValue(instance, "Fernández Saúco");

            Assert.IsTrue(changedProperties.Contains("FullName"));
        }

        [TestMethod]
        public void NotifiesPropertyChangedOfDepedentProperties3()
        {
            Type type = AssemblyWeaver.Assembly.GetType("Catel.Fody.TestAssembly.DetailedDependentPropertyModel");
            object instance = Activator.CreateInstance(type);

            var changedProperties = new List<string>();
            ((INotifyPropertyChanged)instance).PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

            PropertyInfo propertyInfo = instance.GetType().GetProperty("LastName");
            propertyInfo.SetValue(instance, "Fernández Saúco");

            Assert.IsTrue(changedProperties.Contains("Profile"));
        }

        #endregion
    }
}
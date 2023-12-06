namespace Catel.Fody.Tests
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
        [TestCase("Catel.Fody.TestAssembly.DependentPropertyModel", "MiddleName", "middleName", "FullName")]
        [TestCase("Catel.Fody.TestAssembly.DependentPropertyModel", "LastName", "Fernández Saúco", "FullName")]
        [TestCase("Catel.Fody.TestAssembly.DetailedDependentPropertyModel", "MiddleName", "middleName", "Profile")]
        [TestCase("Catel.Fody.TestAssembly.DetailedDependentPropertyModel", "LastName", "Fernández Saúco", "Profile")]
        //[TestCase("Catel.Fody.TestAssembly.DependentPropertyModelWithExistingOnPropertyChanged", "LastName", "Fernández Saúco", "Profile")]
        public void NotifiesPropertyChangedOfDependentProperties(string modelType, string propertyName, string newValue, string expectedPropertyName)
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType(modelType);
            var instance = Activator.CreateInstance(type);

            var changedProperties = new List<string>();
            ((INotifyPropertyChanged) instance).PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

            var propertyInfo = instance.GetType().GetProperty(propertyName);
            propertyInfo.SetValue(instance, newValue);

            var isPropertyChangedWorkingPropertyInfo = instance.GetType().GetProperty("IsPropertyChangedWorking");
            if (isPropertyChangedWorkingPropertyInfo is not null)
            {
                Assert.That((bool)isPropertyChangedWorkingPropertyInfo.GetValue(instance), Is.True);
            }

            Assert.Contains(expectedPropertyName, changedProperties);
        }

        [TestCase("John", "Doe")]
        public void NotifiesDependentProperties(string firstName, string lastName)
        {
            var changeCount = 0;

            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.DependentPersonViewModel");
            dynamic instance = Activator.CreateInstance(type);

            var notifyPropertyChanged = (INotifyPropertyChanged)instance;
            notifyPropertyChanged.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "FullName")
                {
                    changeCount++;
                }
            };

            instance.FirstName = firstName;

            Assert.That(changeCount, Is.EqualTo(1));
            Assert.AreEqual(firstName + " ", instance.FullName);

            instance.Surnames = lastName;

            Assert.That(changeCount, Is.EqualTo(2));
            Assert.AreEqual(firstName + " " + lastName, instance.FullName);
        }
        #endregion
    }
}

﻿namespace Catel.Fody.Tests
{
    using System;
    using System.ComponentModel;
    using NUnit.Framework;

    [TestFixture]
    public class ObservableObjectFacts
    {
        [TestCase]
        public void DoesNotWeaveExistingProperties()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            oo.ExistingProperty = "hi there";

            Assert.That(oo.ExistingProperty, Is.EqualTo("hi there"));
        }

        [TestCase]
        public void IgnoresPropertiesWithoutBackingField()
        {
            // TODO: test Title property
        }

        [TestCase]
        public void HandlesChangeNotificationsMethodsCorrectly()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            Assert.That(oo.OnFirstNameChangedCallbackCalled, Is.False);
            oo.FirstName = "change";
            Assert.That(oo.OnFirstNameChangedCallbackCalled, Is.True);
        }

        [TestCase]
        public void IgnoresChangeNotificationsWithoutRightSignature()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            Assert.That(oo.OnLastNameWithWrongCallbackChangedCallbackCalled, Is.False);
            oo.LastNameWithWrongCallback = "change";
            Assert.That(oo.OnLastNameWithWrongCallbackChangedCallbackCalled, Is.False);
        }

        [TestCase]
        public void ReplacesRaisePropertyChanged()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            var worked = false;

            oo.PropertyChanged += new PropertyChangedEventHandler((sender, e) =>
            {
                worked = true;
            });

            oo.ManuallyRaiseChangeNotificationForManualChangeNotificationProperty();

            Assert.That(worked, Is.True);
        }


        [TestCase]
        public void ReplacesRaisePropertyChanged_Advanced()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            var worked = false;

            oo.PropertyChanged += new PropertyChangedEventHandler((sender, e) =>
            {
                worked = true;
            });

            oo.IsExpanded = true;

            Assert.That(worked, Is.True);
        }
    }
}

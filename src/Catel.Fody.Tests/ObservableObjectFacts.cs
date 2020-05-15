namespace Catel.Fody.Tests
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

            Assert.AreEqual("hi there", oo.ExistingProperty);
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

            Assert.IsFalse(oo.OnFirstNameChangedCallbackCalled);
            oo.FirstName = "change";
            Assert.IsTrue(oo.OnFirstNameChangedCallbackCalled);
        }

        [TestCase]
        public void IgnoresChangeNotificationsWithoutRightSignature()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            Assert.IsFalse(oo.OnLastNameWithWrongCallbackChangedCallbackCalled);
            oo.LastNameWithWrongCallback = "change";
            Assert.IsFalse(oo.OnLastNameWithWrongCallbackChangedCallbackCalled);
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

            Assert.IsTrue(worked);
        }
    }
}

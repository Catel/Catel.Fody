namespace Catel.Fody.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using NUnit.Framework;

    [TestFixture]
    public class ObservableObjectFacts
    {
        [TestCase]
        public void Does_Not_Weave_Existing_Properties()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            oo.ExistingProperty = "hi there";

            Assert.That(oo.ExistingProperty, Is.EqualTo("hi there"));
        }

        [TestCase]
        public void Ignores_Properties_Without_Backing_Field()
        {
            // TODO: test Title property
        }

        [TestCase]
        public void Handles_Change_Notifications_Methods_Correctly()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            Assert.That(oo.OnFirstNameChangedCallbackCalled, Is.False);
            oo.FirstName = "change";
            Assert.That(oo.OnFirstNameChangedCallbackCalled, Is.True);
        }

        [TestCase]
        public void Ignores_Change_Notifications_When_Int_Value_Is_Equal()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            var raised = false;

            var callback = oo.PropertyChanged += new PropertyChangedEventHandler((sender, e) =>
            {
                if (e.PropertyName == "IntProperty")
                {
                    raised = true;
                }
            });

            oo.IntProperty = 42;

            Assert.That(raised, Is.True);
            raised = false;

            oo.IntProperty = 42;

            Assert.That(raised, Is.False);
        }

        [TestCase]
        public void Ignores_Change_Notifications_When_String_Value_Is_Equal()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            var raised = false;

            var callback = oo.PropertyChanged += new PropertyChangedEventHandler((sender, e) =>
            {
                if (e.PropertyName == "StringProperty")
                {
                    raised = true;
                }
            });

            oo.StringProperty = "test";

            Assert.That(raised, Is.True);
            raised = false;

            oo.StringProperty = "test";

            Assert.That(raised, Is.False);
        }

        [TestCase]
        public void Ignores_Change_Notifications_When_List_Value_Is_Reference_Equal()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            var raised = false;

            var callback = oo.PropertyChanged += new PropertyChangedEventHandler((sender, e) =>
            {
                if (e.PropertyName == "ListProperty")
                {
                    raised = true;
                }
            });

            var list = new List<int>(new[] { 1, 2, 3 });

            oo.ListProperty = list;

            Assert.That(raised, Is.True);
            raised = false;

            oo.StringProperty = list;

            Assert.That(raised, Is.False);
        }

        [TestCase]
        public void Raised_Change_Notifications_When_List_Value_Is_Not_Reference_Equal()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            var raised = false;

            var callback = oo.PropertyChanged += new PropertyChangedEventHandler((sender, e) =>
            {
                if (e.PropertyName == "ListProperty")
                {
                    raised = true;
                }
            });

            oo.ListProperty = new List<int>(new[] { 1, 2, 3 });

            Assert.That(raised, Is.True);
            raised = false;

            oo.ListProperty = new List<int>(new[] { 1, 2, 3 });

            Assert.That(raised, Is.True);
        }

        [TestCase]
        public void Ignores_Change_Notifications_Without_Right_Signature()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ObservableObjectTest");
            var oo = (dynamic)Activator.CreateInstance(type);

            Assert.That(oo.OnLastNameWithWrongCallbackChangedCallbackCalled, Is.False);
            oo.LastNameWithWrongCallback = "change";
            Assert.That(oo.OnLastNameWithWrongCallbackChangedCallbackCalled, Is.False);
        }

        [TestCase]
        public void Replaces_RaisePropertyChanged()
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
        public void Replaces_RaisePropertyChanged_Advanced()
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

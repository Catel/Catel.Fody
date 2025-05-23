﻿namespace Catel.Fody.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class CSharp6Facts
    {
        [TestCase]
        public void AutoPropertyInitializer()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CSharp6_AutoPropertyInitializer");
            var obj = (dynamic)Activator.CreateInstance(type);

            Assert.That(obj.SimpleModels, Is.Not.Null);
        }

        [TestCase]
        public void AutoPropertyInitializerWithMultipleConstructors()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CSharp6_AutoPropertyInitializerWithMultipleConstructors");
            var obj = (dynamic)Activator.CreateInstance(type);

            Assert.That(obj.ShowErrorsWithChangeNotification, Is.True);
        }

        [TestCase]
        public void AutoPropertyInitializer_Generic()
        {
            var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.CSharp6_AutoPropertyInitializer_Generic");
            var obj = (dynamic)Activator.CreateInstance(type);

            Assert.That(obj.SimpleModels, Is.Not.Null);
            Assert.That(obj.SelectedItem, Is.Null);
            Assert.That(obj.AdditionalProperty, Is.Null);
        }

        [TestCase]
        public void Errors_When_Using_Auto_Property_Initializers()
        {
            var weaver = new AssemblyWeaver(AssemblyWeaver.GetPathToAssemblyToWeave(false),
                "AutoPropertyInitializers_Errors",
                AssemblyWeaver.GenerateConfigurationXml(
                    new KeyValuePair<string, string>("DisableWarningsForAutoPropertyInitializers", "true"))
                );

            // Note: there are 2 default values, but only 1 has a change callback
            Assert.That(weaver.Errors.Where(x => x.Contains("Do not use C# 6")).Count, Is.EqualTo(1));
            Assert.That(weaver.Errors[0], Is.EqualTo("Do not use C# 6 auto property initializers for 'ShowErrorsWithChangeNotification' since it has a change callback. This might result in unexpected code execution"));
        }
    }
}

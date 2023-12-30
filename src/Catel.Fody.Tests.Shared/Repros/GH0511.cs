namespace Catel.Fody.Tests.Repros
{
    using System;
    using Catel.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class GH0511TestFixture
    {
        [TestCase]
        public void Raises_PropertyChanged_With_Updated_Value()
        {
            var modelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0511.AppSettingsModel");
            var model = Activator.CreateInstance(modelType);

            var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0511.AppSettingsViewModel");
            var viewModel = Activator.CreateInstance(viewModelType, model) as dynamic;
            viewModel.ExpectedValue = "test";

            PropertyHelper.SetPropertyValue(model, "SelectedThemeName", "test");

            Assert.That(viewModel.AppSettings.SelectedThemeName, Is.EqualTo("test"));
        }
    }
}

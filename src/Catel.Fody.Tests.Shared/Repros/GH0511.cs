namespace Catel.Fody.Tests.Repros;

using System;
using Catel.Reflection;
using NUnit.Framework;

#if CATEL_7_OR_HIGHER
using Microsoft.Extensions.DependencyInjection;
#endif

[TestFixture]
public class GH0511TestFixture
{
    [TestCase]
    public void Raises_PropertyChanged_With_Updated_Value()
    {
        var modelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0511.AppSettingsModel");
        var model = Activator.CreateInstance(modelType);

        var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0511.AppSettingsViewModel");

#if CATEL_7_OR_HIGHER
        var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var viewModel = (dynamic)Activator.CreateInstance(viewModelType, model, serviceProvider);
#else
        var viewModel = (dynamic)Activator.CreateInstance(viewModelType, model);
#endif

        viewModel.ExpectedValue = "test";

        PropertyHelper.SetPropertyValue(model, "SelectedThemeName", "test");

        Assert.That(viewModel.AppSettings.SelectedThemeName, Is.EqualTo("test"));
    }
}

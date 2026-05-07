namespace Catel.Fody.Tests.Repros;

using System;
using System.ComponentModel;
using Catel.MVVM;
using Catel.Reflection;
using NUnit.Framework;

#if CATEL_7_OR_HIGHER
using Microsoft.Extensions.DependencyInjection;
#endif

[TestFixture]
public class GH0473TestFixture
{
    [TestCase]
    public void Weaving_Init_Only_Model_Properties()
    {
        var modelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0473.TestModel");
        var model = Activator.CreateInstance(modelType);

        var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0473.GH0473ViewModel");

#if CATEL_7_OR_HIGHER
        var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var viewModel = (dynamic)Activator.CreateInstance(viewModelType, model, serviceProvider);
#else
        var viewModel = (dynamic)Activator.CreateInstance(viewModelType, model);
#endif

        viewModel.Property = new object();

        var isCalled = false;

        var vm = (ViewModelBase)viewModel;
        vm.PropertyChanged += (sender, e) =>
        {
            if (e.HasPropertyChanged("Model"))
            {
                isCalled = true;
            }
        };

        PropertyHelper.SetPropertyValue(viewModel, "Model", null);

        Assert.That(isCalled, Is.True);
    }
}

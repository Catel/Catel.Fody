#if CATEL_6_OR_HIGHER

namespace Catel.Fody.Tests.Repros;

using System;
using Catel.Reflection;
using NUnit.Framework;

#if CATEL_7_OR_HIGHER
using Microsoft.Extensions.DependencyInjection;
#endif

[TestFixture]
public class GH0531TestFixture
{
    [TestCase]
    public void Raises_PropertyChanged_Count_Correctly()
    {
        var modelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0531.TestModel");
        var model = Activator.CreateInstance(modelType);

        var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0531.MyDerivedViewModel");

#if CATEL_7_OR_HIGHER
        var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var viewModel = (dynamic)Activator.CreateInstance(viewModelType, model, serviceProvider);
#else
        var viewModel = (dynamic)Activator.CreateInstance(viewModelType, model);
#endif

        Assert.That(viewModel.Model.Counter, Is.EqualTo(0));

        PropertyHelper.SetPropertyValue(model, "MyProperty", "test");

        Assert.That(viewModel.Model.Counter, Is.EqualTo(1));
    }
}

#endif

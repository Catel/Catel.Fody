namespace Catel.Fody.Tests;

using System;
using NUnit.Framework;

#if CATEL_7_OR_HIGHER
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
#endif

[TestFixture]
public class GH0012TestFixture
{
    [TestCase]
    public void WeavingArgumentCheckForUnusedArgument()
    {
        var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GH0012");

#if CATEL_7_OR_HIGHER
        var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var viewModel = Activator.CreateInstance(type, serviceProvider) as dynamic;
#else
        var viewModel = Activator.CreateInstance(type) as dynamic;
#endif

        viewModel.a("123");
    }
}

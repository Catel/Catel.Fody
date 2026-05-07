namespace Catel.Fody.Tests.Repros;

using System;
using NUnit.Framework;

#if CATEL_7_OR_HIGHER
using Microsoft.Extensions.DependencyInjection;
#endif

[TestFixture]
public class GH0099TestFixture
{
    [TestCase]
    public void WeavingArgumentCheckForUnusedArgument()
    {
        var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0099.TestViewModel");

#if CATEL_7_OR_HIGHER
        var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var vm = (dynamic)Activator.CreateInstance(viewModelType, serviceProvider);
#else
        var vm = (dynamic)Activator.CreateInstance(viewModelType);
#endif

        Assert.That(vm, Is.Not.Null);

        var idProperty = viewModelType.GetProperty("ID");

        Assert.That(idProperty.PropertyType, Is.EqualTo(typeof(int)));
    }
}

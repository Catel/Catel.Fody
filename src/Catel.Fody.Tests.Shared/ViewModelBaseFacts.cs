namespace Catel.Fody.Tests;

using System;
using NUnit.Framework;

#if CATEL_7_OR_HIGHER
using Microsoft.Extensions.DependencyInjection;
#endif

[TestFixture]
public class ViewModelBaseFacts
{
    [TestCase]
    public void StringsCanBeUsedAfterWeaving()
    {
        var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ViewModelBaseTest");

#if CATEL_7_OR_HIGHER
        var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var vm = (dynamic)Activator.CreateInstance(type, serviceProvider);
#else
        var vm = (dynamic)Activator.CreateInstance(type);
#endif

        vm.Name = "hi there";

        Assert.That("hi there", Is.EqualTo(vm.Name));
    }

    [TestCase]
    public void IgnoresICommandProperties()
    {
        var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.ViewModelBaseTest");

#if CATEL_7_OR_HIGHER
        var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var vm = (dynamic)Activator.CreateInstance(type, serviceProvider);
#else
        var vm = (dynamic) Activator.CreateInstance(type);
#endif

        // TODO: Test command by setting it and check if property changed is invoked
    }

    [TestCase]
    public void IgnoresPropertiesWithoutBackingField()
    {
        // tODO: test Title property
    }
}

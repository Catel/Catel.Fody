namespace Catel.Fody.Tests;

using System;
using Catel.Services;
using Data;
using NUnit.Framework;

#if CATEL_7_OR_HIGHER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
#endif

[TestFixture]
public class GH0008TestFixture
{
    [TestCase]
    public void WeavingConstructorWithValidationContext()
    {
        var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GH0008");

#if CATEL_7_OR_HIGHER
        var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var model = Activator.CreateInstance(type, new object[] { new ValidationContext(), serviceProvider, new ProcessService(new NullLogger<ProcessService>()) });
#else
        var model = Activator.CreateInstance(type, new object[] { new ValidationContext(), new ProcessService() });
#endif

        Assert.That(model, Is.Not.Null);
    }

    [TestCase]
    public void WeavingConstructorWithoutValidationContext()
    {
        var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.GH0008");

#if CATEL_7_OR_HIGHER
        var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var model = Activator.CreateInstance(type, new object[] { serviceProvider, new ProcessService(new NullLogger<ProcessService>()) });
#else
        var model = Activator.CreateInstance(type, new object[] { new ProcessService() });
#endif

        Assert.That(model, Is.Not.Null);
    }
}

namespace Catel.Fody.Tests.Repros;

using System;
using NUnit.Framework;

#if CATEL_7_OR_HIGHER
using Microsoft.Extensions.DependencyInjection;
#endif

[TestFixture]
public class GH0361TestFixture
{
    [TestCase]
    public void WeavingWithSetValueMethodInViewModel()
    {
        var viewModelType = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.Bugs.GH0361.MyDerivedViewModel");

#if CATEL_7_OR_HIGHER
        var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var vm = (dynamic)Activator.CreateInstance(viewModelType, serviceProvider);
#else
        var vm = (dynamic)Activator.CreateInstance(viewModelType);
#endif

        vm.Property = new object();
    }
}

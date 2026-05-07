#if CATEL_7_OR_HIGHER
namespace Catel.Fody.Tests;

using Catel;
using Microsoft.Extensions.DependencyInjection;

internal static class ServiceCollectionHelper
{
    public static IServiceCollection CreateServiceCollection()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging();
        serviceCollection.AddCatelCore();
        serviceCollection.AddCatelMvvm();

        return serviceCollection;
    }
}
#endif

using System.Diagnostics;
using System.Globalization;
using Catel.Reflection;
using NUnit.Framework;

#if CATEL_7_OR_HIGHER
using Catel.Logging;
using Catel.Fody.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#endif

[SetUpFixture]
public class GlobalInitialization
{
    [OneTimeSetUp]
    public static void SetUp()
    {
#if CATEL_7_OR_HIGHER
        LogManager.FallbackLoggerFactory = LoggerFactory.Create(x =>
        {
            if (Debugger.IsAttached)
            {
                x.AddFilter(x => x == LogLevel.Debug);

                x.AddDebug();
            }

            x.AddConsole();
        });
#endif

        var culture = new CultureInfo("en-US");
        System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

        // Required since we do multithreaded initialization
        TypeCache.InitializeTypes(allowMultithreadedInitialization: false);

#if CATEL_7_OR_HIGHER
        // Set a global service provider for helpers such as LanguageHelper
        var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();

        Catel.IoC.IoCContainer.ServiceProvider = serviceCollection.BuildServiceProvider();
#endif
    }
}

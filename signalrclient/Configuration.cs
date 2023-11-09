using Microsoft.Extensions.DependencyInjection;
using signalrclient.Backend;

namespace signalrclient;

public class Configuration
{
    public static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddHttpClient(nameof(AccessTokenClient), c => c.BaseAddress = new(AccessTokenClient.TrumfIdUrl));
        services.AddSingleton<AccessTokenClient, AccessTokenClient>();

        BackendModule.Configure(services);

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }
}
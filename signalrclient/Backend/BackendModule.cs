using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace signalrclient.Backend;

public class BackendModule
{
    public static void Configure(ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IBackendConnection, HubConnectionFacade>();
        serviceCollection.AddSingleton<IBackendService, BackendService>();
    }
}
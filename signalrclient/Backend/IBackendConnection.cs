using Microsoft.AspNetCore.SignalR.Client;
using signalrclient.Backend.Models.Envelope;
using TrumfApp.Framework.Backend.Models.Envelope;

namespace signalrclient.Backend;

public interface IBackendConnection : IAsyncDisposable
{
    HubConnectionState State { get; }
    string ConnectionId { get; }

    Task StartAsync();
    
    Task<ResponseEnvelope> Send(RequestEnvelope envelope, CancellationToken cancellationToken);

    event Func<string, Task> Reconnected;
    
    event Func<Exception, Task> Reconnecting;
    
    event Func<Exception, Task> Closed;
    
    Task StopAsync();

    Task UploadTelemetry(string[] data);
    void AddReceiveHandler(Action<RequestEnvelope> receive);
}
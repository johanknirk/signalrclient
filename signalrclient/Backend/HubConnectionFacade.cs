using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using signalrclient.Backend.Models.Envelope;
using System.Data.Common;
using TrumfApp.Framework.Backend.Models.Envelope;
using ILogger = Serilog.ILogger;

namespace signalrclient.Backend;

/// <summary>
/// Wrapper for SignalR HubConnection as a IBackendConnection
/// </summary>
public class HubConnectionFacade : IBackendConnection
{
    private readonly AccessTokenClient _accessTokenClient;
    HubConnection _connection;

    //string url = "https://dev-app.trumf.no/mediator";
    //string url = "https://localhost:7047/mediator";
    string url= "https://ephem101326-app.trumf.no/mediator";
    
    public HubConnectionFacade(IOptions<BackendServiceOptions> options)
    {
        Log = Serilog.Log.ForContext(GetType());

        //TODO hacked
        //var accessToken = File.ReadAllText(@"c:\temp\token.txt");
        var accessToken = "dummy";

        try
        {
            _connection = new HubConnectionBuilder()
                .ConfigureLogging(builder =>
                {
                    builder.AddSerilog();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .WithUrl(url, opt =>
                {
                    opt.Transports = HttpTransportType.WebSockets;
                    opt.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
                .Build();

            _connection.ServerTimeout = TimeSpan.FromSeconds(20); // note: should be 2x the KeepAliveInterval. Also remember default timeout in IBackendService.Send (10 sec)
            _connection.KeepAliveInterval = TimeSpan.FromSeconds(10);

            _connection.On<RequestEnvelope>("Receive", OnReceive);
            _connection.On("Reconnect", () =>
            {
                Log.Information("Reconnecting...");
                Task.Run(async () =>
                {
                    Log.Information("Calling _connection.StopAsync");
                    await _connection.StopAsync();
                    Log.Information("StopAsync completed, calling StartAsync");
                    await _connection.StartAsync();
                    Log.Information("Reconnection complete");
                });
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to build HubConnection");
            throw;
        }
    }

    protected ILogger Log { get; }

    Action<RequestEnvelope> _onReceiveHandler;

    private void OnReceive(RequestEnvelope envelope)
    {
        var handler = _onReceiveHandler;

        if (handler == null)
        {
            Log.Warning("Dropping received envelope because no handler registered (forgot to call IBackendConnection.AddReceiveHandler?)");
            return;
        }

        Log.Verbose("On('Receive') passing envelope on to handler");
        handler.Invoke(envelope);
    }

    public void AddReceiveHandler(Action<RequestEnvelope> handler) => _onReceiveHandler = handler;

    public HubConnectionState State => _connection?.State ?? HubConnectionState.Disconnected;
    public string ConnectionId => _connection.ConnectionId;

    public Task StartAsync() => _connection.StartAsync();

    public Task<ResponseEnvelope> Send(RequestEnvelope envelope, CancellationToken cancellationToken) => _connection?.InvokeAsync<ResponseEnvelope>("Send", envelope, cancellationToken);

    public event Func<string, Task> Reconnected
    {
        add => _connection.Reconnected += value;
        remove => _connection.Reconnected -= value;
    }

    public event Func<Exception, Task> Reconnecting
    {
        add => _connection.Reconnecting += value;
        remove => _connection.Reconnecting -= value;
    }

    public event Func<Exception, Task> Closed
    {
        add => _connection.Closed += value;
        remove => _connection.Closed -= value;
    }

    public Task StopAsync() => _connection?.StopAsync();

    public Task UploadTelemetry(string[] data) => _connection.SendAsync("UploadTelemetry", data);

    public async ValueTask DisposeAsync()
    {
        if (_connection == null)
        {
            return;
        }

        await _connection.DisposeAsync();
        _connection = null;
    }
}
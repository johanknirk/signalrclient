using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using signalrclient.Backend.Models;
using signalrclient.Backend.Models.Envelope;
using TrumfApp.Framework.Backend;
using TrumfApp.Framework.Backend.Models;
using TrumfApp.Framework.Backend.Models.Envelope;

namespace signalrclient.Backend;

public class BackendService : IBackendService
{
    private readonly JsonSerializerOptions _bffJsonOptions;

    private readonly BackendServiceOptions _options;
    private IBackendConnection _connection;
    private bool _isConnecting;
    private bool _isDisposed;

    public BackendService(IBackendConnection connection, IOptions<BackendServiceOptions> options)
    {
        _bffJsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _bffJsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));

        _connection = connection;
        _options = options.Value;

        _connection.Reconnected += OnConnectionOnReconnected;
        _connection.Reconnecting += OnConnectionOnReconnecting;
        _connection.Closed += OnConnectionOnClosed;

        _connection.AddReceiveHandler(Receive);


        Log = Serilog.Log.ForContext(GetType());
    }

    protected ILogger Log { get; }

    public string Url => _options.Url;
    public string ConnectionId => _connection?.ConnectionId;

    public BackendConnectionStatus ConnectionStatus
    {
        get
        {
            if (_isConnecting)
            {
                return BackendConnectionStatus.TryingToConnect;
            }

            switch (_connection?.State)
            {
                case HubConnectionState.Connected:
                    return BackendConnectionStatus.Connected;

                default:
                    return BackendConnectionStatus.Disconnected;
            }
        }
    }

    /// <summary>
    /// Attempts to connect to the BFF
    /// </summary>
    /// <returns></returns>
    public async Task Connect()
    {
        var sw = Stopwatch.StartNew();

        try
        {
            Log.Information("Attempting to connect...");
            _isConnecting = true;
            await _connection.StartAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Connection attempt failed after {Elapsed}ms: {ErrorMessage} ", sw.ElapsedMilliseconds,
                ex.Message);
        }
        finally
        {
            _isConnecting = false;

            Log.Information(" {BffConnectionState} (took {Elapsed}ms)",
                ConnectionStatus, sw.ElapsedMilliseconds);

        }
    }

    public async Task<BffResponse<TResponse>> Send<TResponse>(IBffRequest<TResponse> request, string accesstoken, int timeoutMs = 10_000)
    {
        var sw = Stopwatch.StartNew();
        var requestType = request.GetType().Name;
        var requestId = Guid.NewGuid().ToString("n");

        using var _ = LogContext.PushProperty("Envelope.RequestId", requestId);

        ResponseEnvelope responseEnvelope = null;
        ErrorResponse error = null;

        try
        {

            var requestPayload = JsonSerializer.Serialize(request, request.GetType(), _bffJsonOptions);

            Log.Information(
                    "🖂 Sending {RequestType}, expecting {ResponseType} , ConnectionState: {BffConnectionState})",
                    requestType, typeof(TResponse).Name, ConnectionStatus.ToString());

            Log.Debug("Request body: {@Request}", request);

            if (ConnectionStatus == BackendConnectionStatus.TryingToConnect)
            {
                await WaitForState(HubConnectionState.Connected);
            }

            if (ConnectionStatus != BackendConnectionStatus.Connected)
            {
                Log.Information("Sending request failed: {ErrorCode}", ErrorCodes.Offline);
                return new BffResponse<TResponse>(new ErrorResponse { ErrorCode = ErrorCodes.Offline });
            }


            var envelope = new RequestEnvelope
            {
                RequestType = requestType, 
                Request = requestPayload, 
                RequestId = requestId,
                JwtBearerToken = accesstoken
            };

            var timeout = new CancellationTokenSource(timeoutMs).Token;

            responseEnvelope = await _connection.Send(envelope, timeout);

            if (responseEnvelope.IsError)
            {
                error = JsonSerializer.Deserialize<ErrorResponse>(responseEnvelope.Response, _bffJsonOptions);

                Log.ForContext("ErrorResponse", error, true)
                    .Verbose("Sending request failed: {ErrorCode}", error?.ErrorCode);

                return new BffResponse<TResponse>(error);
            }

            if (typeof(TResponse) == typeof(Default))
            {
                return new BffResponse<TResponse>((TResponse)default);
            }

            var response = JsonSerializer.Deserialize<TResponse>(responseEnvelope.Response, _bffJsonOptions);
            Log.Debug("Response body: {@Response}", response);

            return new BffResponse<TResponse>(response);
        }
        catch (InvalidOperationException ex) when(ex.Message.Contains("connection is not active"))
        {
            error = new ErrorResponse { ErrorCode = ErrorCodes.Offline };
            return new BffResponse<TResponse>(error);
        }
        catch (Exception ex)
        {
            error = new ErrorResponse { ErrorCode = ErrorCodes.Exception };

            Log.ForContext("RawResponseEnvelope", responseEnvelope, true)
                .Error(ex, "Invoking {RequestType} caused an exception", requestType);

            return new BffResponse<TResponse>(error);
        }
        finally
        {
            if (error != null)
            {
                Log.Information("Invoking {RequestType} failed after {Elapsed}ms ({ErrorCode})", requestType,
                    sw.ElapsedMilliseconds, error.ErrorCode);
            }
            else
            {
                Log.Information("Invoking {RequestType} responded successfully in {Elapsed}ms", requestType,
                    sw.ElapsedMilliseconds);
            }
        }
    }


    public Task Disconnect()
    {
        Log.Verbose("Disconnect() called");
        return _connection.StopAsync();
    }

    public async Task<bool> UploadTelemetry(string[] data)
    {
        if (ConnectionStatus != BackendConnectionStatus.Connected)
        {
            Log.Debug("SendTelemetry failed: {ErrorCode}", ErrorCodes.Offline);
            return false;
        }

        try
        {
            await _connection.UploadTelemetry(data);
            return true;
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "SendTelemetry failed");
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed || _connection == null)
        {
            return;
        }

        _isDisposed = true;

        _connection.Reconnected -= OnConnectionOnReconnected;
        _connection.Reconnecting -= OnConnectionOnReconnecting;
        _connection.Closed -= OnConnectionOnClosed;

        await _connection.StopAsync();
        await _connection.DisposeAsync();

        _connection = null;
    }

    private void Receive(RequestEnvelope envelope)
    {
        using var _ = LogContext.PushProperty("Envelope.RequestId", envelope.RequestId);

        Log.Information("📨 Received {RequestType} from BFF: Publishing as event", envelope.RequestType);
        Log.Verbose("{RequestType} payload was {Request}", envelope.RequestType, envelope.Request);

    }

    /// <summary>
    ///     Waits a maximum of ten 50ms intervals for the connection state to become the given state
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task WaitForState(HubConnectionState state)
    {
        var totalDelayMs = 0;
        var delayMs = 50;
        var maxDelayMs = TimeSpan.FromSeconds(6).TotalMilliseconds;
        var randomJitter = new Random();

        Log.Information("Waiting up to {MaxDelayMs}ms for connection to become {HubConnectionState}", maxDelayMs, state);

        while (state != _connection?.State)
        {
            if (totalDelayMs > maxDelayMs)
            {
                Log.Information("Gave up waiting for connection to become {HubConnectionState}", state);
                break;
            }

            var jitter = randomJitter.Next(0, delayMs);
            await Task.Delay(delayMs + jitter);
            totalDelayMs += delayMs;
        }
    }

    private Task OnConnectionOnReconnected(string newConnectionId)
    {
        Log.Information("BFF connected");

        return Task.CompletedTask;
    }

    private Task OnConnectionOnReconnecting(Exception error)
    {
        Log.Verbose("BFF reconnecting");

        return Task.CompletedTask;
    }

    private Task OnConnectionOnClosed(Exception error)
    {
        if (error != null)
        {
            Log.Error(error, "BFF disconnected with error: {ErrorMessage}", error.Message);
        }
        else
        {
            Log.Information("BFF Disconnected");
        }
        return Task.CompletedTask;
    }
}
using signalrclient.Backend.Models;
using TrumfApp.Framework.Backend;
using TrumfApp.Framework.Backend.Models;

namespace signalrclient.Backend;

public interface IBackendConnectivity : IBackendConnectionState, IAsyncDisposable
{
    /// <summary>
    /// Connects to the BFF SignalR Hub, unless already connected
    /// </summary>
    Task Connect();

    /// <summary>
    /// Disconnects from the BFF SignalR hub
    /// </summary>
    /// <returns></returns>
    Task Disconnect();
}

public interface IBackendService : IBackendConnectionState
{
    /// <summary>
    /// Sends the given message to the BFF Signal R Hub and waits for a response
    /// </summary>
    /// <param name="request">The request to send</param>
    /// <param name="timeoutMs">The timeout to observe. Default is 10 seconds.</param>
    /// <returns></returns>

    /// <summary>
    /// Attempts to upload the given telemetry data to the BFF
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    Task<bool> UploadTelemetry(string[] data);

    /// <summary>
    /// Attempts to connect to the BFF
    /// </summary>
    /// <returns></returns>
    Task Connect();

    Task<BffResponse<TResponse>> Send<TResponse>(IBffRequest<TResponse> request, string accesstoken, int timeoutMs = 10_000);
}
using TrumfApp.Framework.Backend;

namespace signalrclient.Backend;

public interface IBackendConnectionState
{
    /// <summary>
    /// Determines whether the app is currently connected to the BFF SignalR Hub.
    ///
    /// Listen for BackendConnectionStateChanged events to be notified whenever this value may have changed.
    /// </summary>
    BackendConnectionStatus ConnectionStatus { get; }

    string Url { get; }
    
    string ConnectionId { get; }
}
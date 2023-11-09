namespace signalrclient.Backend;

public enum BackendConnectionStatus
{
    /// <summary>
    /// Disconnected from the backend
    /// </summary>
    Disconnected,

    /// <summary>
    /// Currently trying to establish a connection to the backend
    /// </summary>
    TryingToConnect,

    /// <summary>
    /// A connection to the backend is open
    /// </summary>
    Connected
}
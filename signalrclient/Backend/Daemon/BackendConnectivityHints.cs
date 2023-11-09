using Serilog;

namespace signalrclient.Backend.Daemon;

public class BackendConnectivityHints : IBackendConnectivityHints
{
    protected ILogger Log { get; } = Serilog.Log.Logger.ForContext(typeof(BackendConnectivityHints));

    public void ShortAppStopExpected()
    {
        Log.Verbose("Setting IgnoreNextStop");
        IgnoreNextStop = true;
    }

    public bool IgnoreNextStop { get; private set; }

    public void Reset()
    {
        Log.Verbose("Resetting IgnoreNextStop");
        IgnoreNextStop = false;
    }
}
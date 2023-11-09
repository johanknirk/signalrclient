namespace signalrclient.Backend.Daemon
{
    public interface IBackendConnectivityHints
    {
        /// <summary>
        /// Hints the connection manager that a short app stop is expected to occur,
        /// so that disconnecting from the BFF may be avoided.
        /// </summary>
        void ShortAppStopExpected();
    }
}
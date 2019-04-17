namespace MachineStreamCore.Configuration
{
    /// <summary>
    /// POCO class that contains the configuration for the connection to
    /// the WebSocket
    /// (check appsettings.json to find the options)
    /// </summary>
    public class WebSocketConfiguration
    {
        public string FullUrl { get; set; }
        public bool DebugMode { get; set; }
        public ushort SendDelay { get; set; }
        public int MinReconnectInterval { get; set; }
        public int MaxReconnectInterval { get; set; }
    }
}

namespace MachineStreamCore.Configuration
{
    /// <summary>
    /// POCO class that contains the configuration to setup the MongoClient,
    /// the database and the collection needed for the project
    /// (check appsettings.json to find the options)
    /// </summary>
    public class MongoDBConfiguration
    {
        public ClientSettingsConfiguration ClientSettings { get; set; }
        public string DatabaseName { get; set; }
        public string EventCollection { get; set; }
    }

    /// <summary>
    /// Configuration to setup the MongoClient and the authentication to it
    /// </summary>
    public class ClientSettingsConfiguration
    {
        public string Server { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string AuthenticationDB { get; set; }
        public string Mechanism { get; set; }
    }
}
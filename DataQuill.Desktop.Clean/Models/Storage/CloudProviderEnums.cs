using System.ComponentModel;

namespace DataQuillDesktop.Models.Storage
{
    /// <summary>
    /// Supported cloud storage providers
    /// </summary>
    public enum CloudProvider
    {
        [Description("Amazon Web Services")]
        AWS,

        [Description("Google Cloud Platform")]
        GoogleCloud,

        [Description("Microsoft Azure")]
        Azure,

        [Description("Oracle Cloud Infrastructure")]
        Oracle
    }

    /// <summary>
    /// Authentication types for cloud providers
    /// </summary>
    public enum AuthenticationType
    {
        [Description("Access Key & Secret")]
        AccessKeySecret,

        [Description("Service Account JSON")]
        ServiceAccountJson,

        [Description("Connection String")]
        ConnectionString,

        [Description("OAuth 2.0")]
        OAuth2,

        [Description("Managed Identity")]
        ManagedIdentity
    }

    /// <summary>
    /// Connection status for cloud connections
    /// </summary>
    public enum ConnectionStatus
    {
        [Description("Not Connected")]
        Disconnected,

        [Description("Connecting...")]
        Connecting,

        [Description("Connected")]
        Connected,

        [Description("Connection Failed")]
        Failed,

        [Description("Testing Connection")]
        Testing
    }
}
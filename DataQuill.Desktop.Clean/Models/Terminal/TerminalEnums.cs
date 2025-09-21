using System.ComponentModel;

namespace DataQuillDesktop.Models.Terminal
{
    /// <summary>
    /// Terminal connection types
    /// </summary>
    public enum TerminalConnectionType
    {
        [Description("SSH")]
        SSH,

        [Description("Telnet")]
        Telnet,

        [Description("Raw TCP")]
        RawTcp,

        [Description("Serial")]
        Serial,

        [Description("Local Shell")]
        LocalShell,

        [Description("Data Stream Monitor")]
        DataStreamMonitor
    }

    /// <summary>
    /// Authentication methods for terminal connections
    /// </summary>
    public enum TerminalAuthType
    {
        [Description("Password")]
        Password,

        [Description("Public Key")]
        PublicKey,

        [Description("Private Key")]
        PrivateKey,

        [Description("Keyboard Interactive")]
        KeyboardInteractive,

        [Description("None")]
        None
    }

    /// <summary>
    /// Terminal connection status
    /// </summary>
    public enum TerminalConnectionStatus
    {
        [Description("Disconnected")]
        Disconnected,

        [Description("Connecting")]
        Connecting,

        [Description("Connected")]
        Connected,

        [Description("Authentication Failed")]
        AuthenticationFailed,

        [Description("Connection Failed")]
        ConnectionFailed,

        [Description("Connection Lost")]
        ConnectionLost,

        [Description("Monitoring")]
        Monitoring
    }

    /// <summary>
    /// Terminal emulation types
    /// </summary>
    public enum TerminalEmulationType
    {
        [Description("VT100")]
        VT100,

        [Description("VT220")]
        VT220,

        [Description("ANSI")]
        ANSI,

        [Description("xterm")]
        XTerm,

        [Description("Raw")]
        Raw
    }
}
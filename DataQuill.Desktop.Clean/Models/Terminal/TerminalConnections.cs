using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace DataQuillDesktop.Models.Terminal
{
    /// <summary>
    /// SSH connection configuration
    /// </summary>
    public class SshConnection : TerminalConnectionBase
    {
        private string _hostname = string.Empty;
        private int _port = 22;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private TerminalAuthType _authType = TerminalAuthType.Password;
        private string _privateKeyPath = string.Empty;
        private string _privateKeyContent = string.Empty;
        private string _passphrase = string.Empty;
        private bool _usePrivateKeyFile = false;
        private int _connectionTimeout = 30;
        private bool _keepAlive = true;
        private int _keepAliveInterval = 60;

        public SshConnection()
        {
            ConnectionType = TerminalConnectionType.SSH;
        }

        public string Hostname
        {
            get => _hostname;
            set => SetProperty(ref _hostname, value);
        }

        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public TerminalAuthType AuthType
        {
            get => _authType;
            set => SetProperty(ref _authType, value);
        }

        public TerminalAuthType AuthenticationType
        {
            get => _authType;
            set => SetProperty(ref _authType, value);
        }

        public string PrivateKeyPath
        {
            get => _privateKeyPath;
            set => SetProperty(ref _privateKeyPath, value);
        }

        public string PrivateKeyPassphrase
        {
            get => _passphrase;
            set => SetProperty(ref _passphrase, value);
        }

        public string PrivateKeyContent
        {
            get => _privateKeyContent;
            set => SetProperty(ref _privateKeyContent, value);
        }

        public string Passphrase
        {
            get => _passphrase;
            set => SetProperty(ref _passphrase, value);
        }

        public bool UsePrivateKeyFile
        {
            get => _usePrivateKeyFile;
            set => SetProperty(ref _usePrivateKeyFile, value);
        }

        public int ConnectionTimeout
        {
            get => _connectionTimeout;
            set => SetProperty(ref _connectionTimeout, value);
        }

        public bool KeepAlive
        {
            get => _keepAlive;
            set => SetProperty(ref _keepAlive, value);
        }

        public int KeepAliveInterval
        {
            get => _keepAliveInterval;
            set => SetProperty(ref _keepAliveInterval, value);
        }

        public override bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Hostname) || string.IsNullOrWhiteSpace(Username))
                return false;

            if (Port <= 0 || Port > 65535)
                return false;

            switch (AuthType)
            {
                case TerminalAuthType.Password:
                    return !string.IsNullOrWhiteSpace(Password);
                case TerminalAuthType.PublicKey:
                    if (UsePrivateKeyFile)
                        return !string.IsNullOrWhiteSpace(PrivateKeyPath) && File.Exists(PrivateKeyPath);
                    else
                        return !string.IsNullOrWhiteSpace(PrivateKeyContent);
                case TerminalAuthType.None:
                    return true;
                default:
                    return false;
            }
        }

        public override string GetConnectionIdentifier()
        {
            return $"ssh://{Username}@{Hostname}:{Port}";
        }
    }

    /// <summary>
    /// Telnet connection configuration
    /// </summary>
    public class TelnetConnection : TerminalConnectionBase
    {
        private string _hostname = string.Empty;
        private int _port = 23;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private int _connectionTimeout = 30;
        private bool _localEcho = false;

        public TelnetConnection()
        {
            ConnectionType = TerminalConnectionType.Telnet;
        }

        public string Hostname
        {
            get => _hostname;
            set => SetProperty(ref _hostname, value);
        }

        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public int ConnectionTimeout
        {
            get => _connectionTimeout;
            set => SetProperty(ref _connectionTimeout, value);
        }

        public bool LocalEcho
        {
            get => _localEcho;
            set => SetProperty(ref _localEcho, value);
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Hostname) && Port > 0 && Port <= 65535;
        }

        public override string GetConnectionIdentifier()
        {
            return $"telnet://{Hostname}:{Port}";
        }
    }

    /// <summary>
    /// Raw TCP connection configuration
    /// </summary>
    public class RawTcpConnection : TerminalConnectionBase
    {
        private string _hostname = string.Empty;
        private int _port = 23;
        private int _connectionTimeout = 30;
        private bool _binaryMode = false;

        public RawTcpConnection()
        {
            ConnectionType = TerminalConnectionType.RawTcp;
        }

        public string Hostname
        {
            get => _hostname;
            set => SetProperty(ref _hostname, value);
        }

        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public int ConnectionTimeout
        {
            get => _connectionTimeout;
            set => SetProperty(ref _connectionTimeout, value);
        }

        public bool BinaryMode
        {
            get => _binaryMode;
            set => SetProperty(ref _binaryMode, value);
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Hostname) && Port > 0 && Port <= 65535;
        }

        public override string GetConnectionIdentifier()
        {
            return $"tcp://{Hostname}:{Port}";
        }
    }

    /// <summary>
    /// Serial connection configuration
    /// </summary>
    public class SerialConnection : TerminalConnectionBase
    {
        private string _portName = "COM1";
        private int _baudRate = 9600;
        private int _dataBits = 8;
        private string _parity = "None";
        private string _stopBits = "One";
        private string _handshake = "None";
        private int _readTimeout = 500;
        private int _writeTimeout = 500;
        private bool _dtrEnable = false;
        private bool _rtsEnable = false;

        public SerialConnection()
        {
            ConnectionType = TerminalConnectionType.Serial;
        }

        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        public int BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        public int DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        public string Parity
        {
            get => _parity;
            set => SetProperty(ref _parity, value);
        }

        public string StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }

        public string Handshake
        {
            get => _handshake;
            set => SetProperty(ref _handshake, value);
        }

        public int ReadTimeout
        {
            get => _readTimeout;
            set => SetProperty(ref _readTimeout, value);
        }

        public int WriteTimeout
        {
            get => _writeTimeout;
            set => SetProperty(ref _writeTimeout, value);
        }

        public bool DtrEnable
        {
            get => _dtrEnable;
            set => SetProperty(ref _dtrEnable, value);
        }

        public bool RtsEnable
        {
            get => _rtsEnable;
            set => SetProperty(ref _rtsEnable, value);
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(PortName) && BaudRate > 0;
        }

        public override string GetConnectionIdentifier()
        {
            return $"serial://{PortName}@{BaudRate}";
        }
    }

    /// <summary>
    /// Local shell connection configuration
    /// </summary>
    public class LocalShellConnection : TerminalConnectionBase
    {
        private string _shellCommand = "cmd.exe";
        private string _workingDirectory = string.Empty;
        private string _arguments = string.Empty;

        public LocalShellConnection()
        {
            ConnectionType = TerminalConnectionType.LocalShell;
        }

        public string ShellCommand
        {
            get => _shellCommand;
            set => SetProperty(ref _shellCommand, value);
        }

        public string ShellPath
        {
            get => _shellCommand;
            set => SetProperty(ref _shellCommand, value);
        }

        public string WorkingDirectory
        {
            get => _workingDirectory;
            set => SetProperty(ref _workingDirectory, value);
        }

        public string Arguments
        {
            get => _arguments;
            set => SetProperty(ref _arguments, value);
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ShellCommand);
        }

        public override string GetConnectionIdentifier()
        {
            return $"local://{ShellCommand}";
        }
    }

    /// <summary>
    /// Data stream monitor configuration
    /// </summary>
    public class DataStreamMonitorConnection : TerminalConnectionBase
    {
        private string _cloudConnectionId = string.Empty;
        private string _cloudConnectionName = string.Empty;
        private bool _showRawData = true;
        private bool _showTimestamps = true;
        private int _bufferSize = 10000;

        public DataStreamMonitorConnection()
        {
            ConnectionType = TerminalConnectionType.DataStreamMonitor;
        }

        public string CloudConnectionId
        {
            get => _cloudConnectionId;
            set => SetProperty(ref _cloudConnectionId, value);
        }

        public string CloudConnectionName
        {
            get => _cloudConnectionName;
            set => SetProperty(ref _cloudConnectionName, value);
        }

        public bool ShowRawData
        {
            get => _showRawData;
            set => SetProperty(ref _showRawData, value);
        }

        public bool ShowTimestamps
        {
            get => _showTimestamps;
            set => SetProperty(ref _showTimestamps, value);
        }

        public int BufferSize
        {
            get => _bufferSize;
            set => SetProperty(ref _bufferSize, value);
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(CloudConnectionId);
        }

        public override string GetConnectionIdentifier()
        {
            return $"monitor://{CloudConnectionName}";
        }
    }
}
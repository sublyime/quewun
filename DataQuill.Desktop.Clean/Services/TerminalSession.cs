using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DataQuillDesktop.Models.Terminal;
using Renci.SshNet;

namespace DataQuillDesktop.Services
{
    /// <summary>
    /// Represents an active terminal session
    /// </summary>
    public class TerminalSession : INotifyPropertyChanged, IDisposable
    {
        private readonly TerminalConnectionBase _connection;
        private SshClient? _sshClient;
        private ShellStream? _shellStream;
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private SerialPort? _serialPort;
        private Process? _localProcess;
        private bool _isConnected;
        private bool _disposed;

        private readonly ObservableCollection<TerminalMessage> _messages;
        private readonly StringBuilder _outputBuffer;

        public TerminalSession(TerminalConnectionBase connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Id = Guid.NewGuid().ToString();
            _messages = new ObservableCollection<TerminalMessage>();
            _outputBuffer = new StringBuilder();

            // Update connection properties
            ConnectionId = _connection.Id;
            ConnectionName = _connection.Name;
            ConnectionType = _connection.ConnectionType;
        }

        public string Id { get; }
        public string ConnectionId { get; }
        public string ConnectionName { get; }
        public TerminalConnectionType ConnectionType { get; }
        public DateTime CreatedAt { get; } = DateTime.Now;

        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged();

                    // Update connection status
                    _connection.Status = value ?
                        TerminalConnectionStatus.Connected :
                        TerminalConnectionStatus.Disconnected;
                }
            }
        }

        public ObservableCollection<TerminalMessage> Messages => _messages;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<string>? DataReceived;
        public event EventHandler? Disconnected;

        /// <summary>
        /// Connects to the terminal
        /// </summary>
        public async Task ConnectAsync()
        {
            if (IsConnected) return;

            try
            {
                _connection.Status = TerminalConnectionStatus.Connecting;
                AddMessage("Connecting...", TerminalMessageType.System);

                switch (_connection.ConnectionType)
                {
                    case TerminalConnectionType.SSH:
                        await ConnectSshAsync();
                        break;
                    case TerminalConnectionType.Telnet:
                        await ConnectTelnetAsync();
                        break;
                    case TerminalConnectionType.RawTcp:
                        await ConnectRawTcpAsync();
                        break;
                    case TerminalConnectionType.Serial:
                        await ConnectSerialAsync();
                        break;
                    case TerminalConnectionType.LocalShell:
                        await ConnectLocalShellAsync();
                        break;
                    case TerminalConnectionType.DataStreamMonitor:
                        await ConnectDataStreamMonitorAsync();
                        break;
                    default:
                        throw new NotSupportedException($"Connection type {_connection.ConnectionType} is not supported");
                }

                IsConnected = true;
                _connection.LastConnected = DateTime.Now;
                AddMessage("Connected successfully", TerminalMessageType.System);
            }
            catch (Exception ex)
            {
                _connection.Status = TerminalConnectionStatus.ConnectionFailed;
                _connection.ErrorMessage = ex.Message;
                AddMessage($"Connection failed: {ex.Message}", TerminalMessageType.Error);
                throw;
            }
        }

        /// <summary>
        /// Disconnects from the terminal
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected) return;

            try
            {
                // Clean up connection-specific resources
                _sshClient?.Disconnect();
                _sshClient?.Dispose();
                _shellStream?.Dispose();

                _tcpClient?.Close();
                _tcpClient?.Dispose();
                _networkStream?.Dispose();

                _serialPort?.Close();
                _serialPort?.Dispose();

                _localProcess?.Kill();
                _localProcess?.Dispose();

                IsConnected = false;
                AddMessage("Disconnected", TerminalMessageType.System);
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                AddMessage($"Error during disconnect: {ex.Message}", TerminalMessageType.Error);
            }
        }

        /// <summary>
        /// Sends data to the terminal
        /// </summary>
        public async Task SendDataAsync(string data)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected");

            try
            {
                switch (_connection.ConnectionType)
                {
                    case TerminalConnectionType.SSH:
                        if (_shellStream != null)
                        {
                            await _shellStream.WriteAsync(Encoding.UTF8.GetBytes(data));
                            AddMessage(data, TerminalMessageType.Sent);
                        }
                        break;

                    case TerminalConnectionType.Telnet:
                    case TerminalConnectionType.RawTcp:
                        if (_networkStream != null)
                        {
                            var bytes = Encoding.UTF8.GetBytes(data);
                            await _networkStream.WriteAsync(bytes, 0, bytes.Length);
                            AddMessage(data, TerminalMessageType.Sent);
                        }
                        break;

                    case TerminalConnectionType.Serial:
                        if (_serialPort != null && _serialPort.IsOpen)
                        {
                            await _serialPort.BaseStream.WriteAsync(Encoding.UTF8.GetBytes(data));
                            AddMessage(data, TerminalMessageType.Sent);
                        }
                        break;

                    case TerminalConnectionType.LocalShell:
                        if (_localProcess != null && !_localProcess.HasExited)
                        {
                            await _localProcess.StandardInput.WriteAsync(data);
                            AddMessage(data, TerminalMessageType.Sent);
                        }
                        break;

                    case TerminalConnectionType.DataStreamMonitor:
                        // Data stream monitor is read-only
                        AddMessage("Data stream monitor is read-only", TerminalMessageType.Warning);
                        break;
                }
            }
            catch (Exception ex)
            {
                AddMessage($"Error sending data: {ex.Message}", TerminalMessageType.Error);
                throw;
            }
        }

        /// <summary>
        /// Connects via SSH
        /// </summary>
        private async Task ConnectSshAsync()
        {
            var sshConnection = (SshConnection)_connection;

            ConnectionInfo connectionInfo;

            switch (sshConnection.AuthenticationType)
            {
                case TerminalAuthType.Password:
                    connectionInfo = new ConnectionInfo(sshConnection.Hostname, sshConnection.Port,
                        sshConnection.Username, new PasswordAuthenticationMethod(sshConnection.Username, sshConnection.Password));
                    break;

                case TerminalAuthType.PrivateKey:
                    if (!File.Exists(sshConnection.PrivateKeyPath))
                        throw new FileNotFoundException($"Private key file not found: {sshConnection.PrivateKeyPath}");

                    var keyFile = new PrivateKeyFile(sshConnection.PrivateKeyPath, sshConnection.PrivateKeyPassphrase);
                    connectionInfo = new ConnectionInfo(sshConnection.Hostname, sshConnection.Port,
                        sshConnection.Username, new PrivateKeyAuthenticationMethod(sshConnection.Username, keyFile));
                    break;

                default:
                    throw new NotSupportedException($"Authentication type {sshConnection.AuthenticationType} is not supported");
            }

            _sshClient = new SshClient(connectionInfo);
            await Task.Run(() => _sshClient.Connect());

            // Create shell stream
            _shellStream = _sshClient.CreateShellStream("terminal",
                (uint)_connection.TerminalWidth, (uint)_connection.TerminalHeight, 800, 600, 1024);

            // Start reading output
            _ = Task.Run(ReadSshOutputAsync);
        }

        /// <summary>
        /// Connects via Telnet
        /// </summary>
        private async Task ConnectTelnetAsync()
        {
            var telnetConnection = (TelnetConnection)_connection;

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(telnetConnection.Hostname, telnetConnection.Port);
            _networkStream = _tcpClient.GetStream();

            // Start reading output
            _ = Task.Run(ReadNetworkOutputAsync);

            // Send initial authentication if provided
            if (!string.IsNullOrEmpty(telnetConnection.Username))
            {
                await Task.Delay(500); // Wait for login prompt
                await SendDataAsync(telnetConnection.Username + "\r\n");

                if (!string.IsNullOrEmpty(telnetConnection.Password))
                {
                    await Task.Delay(500); // Wait for password prompt
                    await SendDataAsync(telnetConnection.Password + "\r\n");
                }
            }
        }

        /// <summary>
        /// Connects via Raw TCP
        /// </summary>
        private async Task ConnectRawTcpAsync()
        {
            var tcpConnection = (RawTcpConnection)_connection;

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(tcpConnection.Hostname, tcpConnection.Port);
            _networkStream = _tcpClient.GetStream();

            // Start reading output
            _ = Task.Run(ReadNetworkOutputAsync);
        }

        /// <summary>
        /// Connects via Serial Port
        /// </summary>
        private async Task ConnectSerialAsync()
        {
            var serialConnection = (SerialConnection)_connection;

            _serialPort = new SerialPort
            {
                PortName = serialConnection.PortName,
                BaudRate = serialConnection.BaudRate,
                DataBits = serialConnection.DataBits,
                Parity = Enum.Parse<Parity>(serialConnection.Parity),
                StopBits = Enum.Parse<StopBits>(serialConnection.StopBits),
                Handshake = Enum.Parse<Handshake>(serialConnection.Handshake),
                ReadTimeout = serialConnection.ReadTimeout,
                WriteTimeout = serialConnection.WriteTimeout,
                DtrEnable = serialConnection.DtrEnable,
                RtsEnable = serialConnection.RtsEnable
            };

            await Task.Run(() => _serialPort.Open());

            // Start reading output
            _ = Task.Run(ReadSerialOutputAsync);
        }

        /// <summary>
        /// Connects to local shell
        /// </summary>
        private async Task ConnectLocalShellAsync()
        {
            var shellConnection = (LocalShellConnection)_connection;

            var startInfo = new ProcessStartInfo
            {
                FileName = shellConnection.ShellPath,
                Arguments = shellConnection.Arguments,
                WorkingDirectory = shellConnection.WorkingDirectory,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _localProcess = Process.Start(startInfo);
            if (_localProcess == null)
                throw new InvalidOperationException("Failed to start local shell process");

            // Start reading output
            _ = Task.Run(ReadLocalProcessOutputAsync);
        }

        /// <summary>
        /// Connects to data stream monitor
        /// </summary>
        private async Task ConnectDataStreamMonitorAsync()
        {
            var monitorConnection = (DataStreamMonitorConnection)_connection;

            // This would connect to cloud services and monitor data streams
            // For now, simulate connection
            await Task.Delay(1000);

            AddMessage($"Monitoring cloud connection: {monitorConnection.CloudConnectionId}", TerminalMessageType.Info);
            AddMessage("Data stream monitoring is simulated in this version", TerminalMessageType.Warning);
        }

        /// <summary>
        /// Reads SSH output asynchronously
        /// </summary>
        private async Task ReadSshOutputAsync()
        {
            if (_shellStream == null) return;

            var buffer = new byte[1024];

            try
            {
                while (IsConnected && _shellStream.CanRead)
                {
                    var bytesRead = await _shellStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        ProcessReceivedData(data);
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsConnected)
                    AddMessage($"SSH read error: {ex.Message}", TerminalMessageType.Error);
            }
        }

        /// <summary>
        /// Reads network output asynchronously
        /// </summary>
        private async Task ReadNetworkOutputAsync()
        {
            if (_networkStream == null) return;

            var buffer = new byte[1024];

            try
            {
                while (IsConnected && _networkStream.CanRead)
                {
                    var bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        ProcessReceivedData(data);
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsConnected)
                    AddMessage($"Network read error: {ex.Message}", TerminalMessageType.Error);
            }
        }

        /// <summary>
        /// Reads local process output asynchronously
        /// </summary>
        private async Task ReadLocalProcessOutputAsync()
        {
            if (_localProcess == null) return;

            try
            {
                // Read both stdout and stderr
                var outputTask = Task.Run(async () =>
                {
                    using var reader = _localProcess.StandardOutput;
                    char[] buffer = new char[1024];
                    int charsRead;
                    while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        var data = new string(buffer, 0, charsRead);
                        ProcessReceivedData(data);
                    }
                });

                var errorTask = Task.Run(async () =>
                {
                    using var reader = _localProcess.StandardError;
                    char[] buffer = new char[1024];
                    int charsRead;
                    while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        var data = new string(buffer, 0, charsRead);
                        AddMessage(data, TerminalMessageType.Error);
                    }
                });

                await Task.WhenAny(outputTask, errorTask);
            }
            catch (Exception ex)
            {
                if (IsConnected)
                    AddMessage($"Process read error: {ex.Message}", TerminalMessageType.Error);
            }
        }

        /// <summary>
        /// Reads serial port output asynchronously
        /// </summary>
        private async Task ReadSerialOutputAsync()
        {
            if (_serialPort == null) return;

            var buffer = new byte[1024];

            try
            {
                while (IsConnected && _serialPort.IsOpen)
                {
                    var bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        ProcessReceivedData(data);
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsConnected)
                    AddMessage($"Serial read error: {ex.Message}", TerminalMessageType.Error);
            }
        }

        /// <summary>
        /// Processes received data
        /// </summary>
        private void ProcessReceivedData(string data)
        {
            _outputBuffer.Append(data);
            AddMessage(data, TerminalMessageType.Received);
            DataReceived?.Invoke(this, data);
        }

        /// <summary>
        /// Adds a message to the terminal
        /// </summary>
        private void AddMessage(string content, TerminalMessageType type)
        {
            var message = new TerminalMessage
            {
                Content = content,
                Type = type,
                Timestamp = DateTime.Now
            };

            App.Current.Dispatcher.Invoke(() => _messages.Add(message));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (_disposed) return;

            Disconnect();
            _disposed = true;
        }
    }

    /// <summary>
    /// Represents a terminal message
    /// </summary>
    public class TerminalMessage
    {
        public string Content { get; set; } = string.Empty;
        public TerminalMessageType Type { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Types of terminal messages
    /// </summary>
    public enum TerminalMessageType
    {
        Received,   // Data received from terminal
        Sent,       // Data sent to terminal
        System,     // System messages
        Error,      // Error messages
        Warning,    // Warning messages
        Info        // Information messages
    }
}
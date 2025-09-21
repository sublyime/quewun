using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DataQuillDesktop.Models.Terminal;

namespace DataQuillDesktop.Services
{
    /// <summary>
    /// Service for managing terminal connections
    /// </summary>
    public class TerminalConnectionManager
    {
        private static TerminalConnectionManager? _instance;
        private static readonly object _lock = new object();

        private readonly ObservableCollection<TerminalConnectionBase> _connections;
        private readonly ObservableCollection<TerminalSession> _activeSessions;
        private readonly string _connectionsFilePath;

        private TerminalConnectionManager()
        {
            _connections = new ObservableCollection<TerminalConnectionBase>();
            _activeSessions = new ObservableCollection<TerminalSession>();

            // Store connections in user's AppData folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "DataQuill");
            Directory.CreateDirectory(appFolder);
            _connectionsFilePath = Path.Combine(appFolder, "terminal_connections.json");

            LoadConnections();
        }

        public static TerminalConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new TerminalConnectionManager();
                    }
                }
                return _instance;
            }
        }

        public ObservableCollection<TerminalConnectionBase> Connections => _connections;
        public ObservableCollection<TerminalSession> ActiveSessions => _activeSessions;

        /// <summary>
        /// Adds a new terminal connection
        /// </summary>
        public void AddConnection(TerminalConnectionBase connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            // Ensure unique name
            var baseName = connection.Name;
            var counter = 1;
            while (_connections.Any(c => c.Name.Equals(connection.Name, StringComparison.OrdinalIgnoreCase)))
            {
                connection.Name = $"{baseName} ({counter++})";
            }

            _connections.Add(connection);
            SaveConnections();
        }

        /// <summary>
        /// Removes a terminal connection
        /// </summary>
        public bool RemoveConnection(string connectionId)
        {
            var connection = _connections.FirstOrDefault(c => c.Id == connectionId);
            if (connection == null) return false;

            // Close active session if exists
            var session = _activeSessions.FirstOrDefault(s => s.ConnectionId == connectionId);
            if (session != null)
            {
                CloseSession(session.Id);
            }

            _connections.Remove(connection);
            SaveConnections();
            return true;
        }

        /// <summary>
        /// Updates an existing terminal connection
        /// </summary>
        public bool UpdateConnection(TerminalConnectionBase updatedConnection)
        {
            var existingConnection = _connections.FirstOrDefault(c => c.Id == updatedConnection.Id);
            if (existingConnection == null) return false;

            var index = _connections.IndexOf(existingConnection);
            _connections[index] = updatedConnection;
            SaveConnections();
            return true;
        }

        /// <summary>
        /// Gets a connection by ID
        /// </summary>
        public TerminalConnectionBase? GetConnection(string connectionId)
        {
            return _connections.FirstOrDefault(c => c.Id == connectionId);
        }

        /// <summary>
        /// Creates a new connection instance for the specified type
        /// </summary>
        public TerminalConnectionBase CreateConnection(TerminalConnectionType connectionType)
        {
            try
            {
                return connectionType switch
                {
                    TerminalConnectionType.SSH => new SshConnection { Name = "New SSH Connection" },
                    TerminalConnectionType.Telnet => new TelnetConnection { Name = "New Telnet Connection" },
                    TerminalConnectionType.RawTcp => new RawTcpConnection { Name = "New TCP Connection" },
                    TerminalConnectionType.Serial => new SerialConnection { Name = "New Serial Connection" },
                    TerminalConnectionType.LocalShell => new LocalShellConnection { Name = "New Local Shell" },
                    TerminalConnectionType.DataStreamMonitor => new DataStreamMonitorConnection { Name = "New Data Monitor" },
                    _ => throw new ArgumentException($"Unsupported connection type: {connectionType}")
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating connection: {ex.Message}");
                System.Console.WriteLine($"Error creating connection: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Opens a new terminal session
        /// </summary>
        public async Task<TerminalSession?> OpenSessionAsync(string connectionId)
        {
            var connection = GetConnection(connectionId);
            if (connection == null) return null;

            var session = new TerminalSession(connection);
            _activeSessions.Add(session);

            try
            {
                await session.ConnectAsync();
                return session;
            }
            catch (Exception ex)
            {
                _activeSessions.Remove(session);
                connection.ErrorMessage = ex.Message;
                connection.Status = TerminalConnectionStatus.ConnectionFailed;
                throw;
            }
        }

        /// <summary>
        /// Closes a terminal session
        /// </summary>
        public void CloseSession(string sessionId)
        {
            var session = _activeSessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                session.Disconnect();
                _activeSessions.Remove(session);
            }
        }

        /// <summary>
        /// Gets active session for a connection
        /// </summary>
        public TerminalSession? GetActiveSession(string connectionId)
        {
            return _activeSessions.FirstOrDefault(s => s.ConnectionId == connectionId);
        }

        /// <summary>
        /// Gets all cloud connections for data stream monitoring
        /// </summary>
        public IEnumerable<CloudConnectionInfo> GetAvailableCloudConnections()
        {
            var cloudManager = CloudConnectionManager.Instance;
            return cloudManager.Connections
                .Where(c => c.Status == Models.Storage.ConnectionStatus.Connected)
                .Select(c => new CloudConnectionInfo
                {
                    Id = c.Id,
                    Name = c.Name,
                    Provider = c.Provider.ToString(),
                    Description = c.Description
                });
        }

        /// <summary>
        /// Loads connections from file
        /// </summary>
        private void LoadConnections()
        {
            try
            {
                if (!File.Exists(_connectionsFilePath)) return;

                var jsonContent = File.ReadAllText(_connectionsFilePath);
                var connectionDtos = JsonSerializer.Deserialize<List<TerminalConnectionDto>>(jsonContent);

                if (connectionDtos == null) return;

                foreach (var dto in connectionDtos)
                {
                    var connection = DeserializeConnection(dto);
                    if (connection != null)
                    {
                        // Reset status to disconnected on startup
                        connection.Status = TerminalConnectionStatus.Disconnected;
                        _connections.Add(connection);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the application
                Console.WriteLine($"Error loading terminal connections: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves connections to file
        /// </summary>
        private void SaveConnections()
        {
            try
            {
                var connectionDtos = _connections.Select(SerializeConnection).ToList();
                var jsonContent = JsonSerializer.Serialize(connectionDtos, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_connectionsFilePath, jsonContent);
            }
            catch (Exception ex)
            {
                // Log error but don't crash the application
                Console.WriteLine($"Error saving terminal connections: {ex.Message}");
            }
        }

        /// <summary>
        /// Serializes a connection to a DTO for JSON storage
        /// </summary>
        private TerminalConnectionDto SerializeConnection(TerminalConnectionBase connection)
        {
            return new TerminalConnectionDto
            {
                Id = connection.Id,
                Name = connection.Name,
                Description = connection.Description,
                ConnectionType = connection.ConnectionType,
                LastConnected = connection.LastConnected,
                EmulationType = connection.EmulationType,
                TerminalWidth = connection.TerminalWidth,
                TerminalHeight = connection.TerminalHeight,
                AutoReconnect = connection.AutoReconnect,
                ConfigurationJson = JsonSerializer.Serialize(connection, connection.GetType())
            };
        }

        /// <summary>
        /// Deserializes a connection from a DTO
        /// </summary>
        private TerminalConnectionBase? DeserializeConnection(TerminalConnectionDto dto)
        {
            try
            {
                var type = dto.ConnectionType switch
                {
                    TerminalConnectionType.SSH => typeof(SshConnection),
                    TerminalConnectionType.Telnet => typeof(TelnetConnection),
                    TerminalConnectionType.RawTcp => typeof(RawTcpConnection),
                    TerminalConnectionType.Serial => typeof(SerialConnection),
                    TerminalConnectionType.LocalShell => typeof(LocalShellConnection),
                    TerminalConnectionType.DataStreamMonitor => typeof(DataStreamMonitorConnection),
                    _ => null
                };

                if (type == null) return null;

                return JsonSerializer.Deserialize(dto.ConfigurationJson, type) as TerminalConnectionBase;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// DTO for JSON serialization
        /// </summary>
        private class TerminalConnectionDto
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public TerminalConnectionType ConnectionType { get; set; }
            public DateTime LastConnected { get; set; }
            public TerminalEmulationType EmulationType { get; set; }
            public int TerminalWidth { get; set; }
            public int TerminalHeight { get; set; }
            public bool AutoReconnect { get; set; }
            public string ConfigurationJson { get; set; } = string.Empty;
        }
    }

    /// <summary>
    /// Information about available cloud connections
    /// </summary>
    public class CloudConnectionInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
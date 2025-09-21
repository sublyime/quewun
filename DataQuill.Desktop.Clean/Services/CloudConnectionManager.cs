using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DataQuillDesktop.Models.Storage;

namespace DataQuillDesktop.Services
{
    /// <summary>
    /// Service for managing cloud storage connections
    /// </summary>
    public class CloudConnectionManager
    {
        private static CloudConnectionManager? _instance;
        private static readonly object _lock = new object();

        private readonly ObservableCollection<CloudConnectionBase> _connections;
        private readonly string _connectionsFilePath;

        private CloudConnectionManager()
        {
            _connections = new ObservableCollection<CloudConnectionBase>();

            // Store connections in user's AppData folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "DataQuill");
            Directory.CreateDirectory(appFolder);
            _connectionsFilePath = Path.Combine(appFolder, "connections.json");

            LoadConnections();
        }

        public static CloudConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new CloudConnectionManager();
                    }
                }
                return _instance;
            }
        }

        public ObservableCollection<CloudConnectionBase> Connections => _connections;

        /// <summary>
        /// Adds a new connection
        /// </summary>
        public void AddConnection(CloudConnectionBase connection)
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
        /// Removes a connection
        /// </summary>
        public bool RemoveConnection(string connectionId)
        {
            var connection = _connections.FirstOrDefault(c => c.Id == connectionId);
            if (connection == null) return false;

            _connections.Remove(connection);
            SaveConnections();
            return true;
        }

        /// <summary>
        /// Updates an existing connection
        /// </summary>
        public bool UpdateConnection(CloudConnectionBase updatedConnection)
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
        public CloudConnectionBase? GetConnection(string connectionId)
        {
            return _connections.FirstOrDefault(c => c.Id == connectionId);
        }

        /// <summary>
        /// Gets all connections for a specific provider
        /// </summary>
        public IEnumerable<CloudConnectionBase> GetConnectionsByProvider(CloudProvider provider)
        {
            return _connections.Where(c => c.Provider == provider);
        }

        /// <summary>
        /// Tests a connection without saving it
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> TestConnectionAsync(CloudConnectionBase connection)
        {
            try
            {
                connection.Status = ConnectionStatus.Testing;

                // Validate the connection first
                if (!connection.IsValid())
                {
                    connection.Status = ConnectionStatus.Failed;
                    connection.ErrorMessage = "Invalid connection configuration";
                    return (false, "Invalid connection configuration");
                }

                // Simulate connection testing (in real implementation, this would test actual cloud connectivity)
                await Task.Delay(2000); // Simulate network delay

                // For now, we'll assume the connection is successful if the configuration is valid
                // In a real implementation, you would use the respective cloud SDKs to test connectivity
                var success = await SimulateCloudConnectionTest(connection);

                if (success)
                {
                    connection.Status = ConnectionStatus.Connected;
                    connection.LastConnected = DateTime.Now;
                    connection.ErrorMessage = string.Empty;
                    return (true, string.Empty);
                }
                else
                {
                    connection.Status = ConnectionStatus.Failed;
                    connection.ErrorMessage = "Connection test failed";
                    return (false, "Connection test failed");
                }
            }
            catch (Exception ex)
            {
                connection.Status = ConnectionStatus.Failed;
                connection.ErrorMessage = ex.Message;
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Simulates cloud connection testing (placeholder for actual implementation)
        /// </summary>
        private async Task<bool> SimulateCloudConnectionTest(CloudConnectionBase connection)
        {
            // In a real implementation, this would use the appropriate cloud SDK
            // For example:
            // - AWS: Use AWS SDK to list S3 buckets or EC2 instances
            // - Azure: Use Azure SDK to list storage accounts or resource groups
            // - GCP: Use Google Cloud SDK to list projects or storage buckets
            // - Oracle: Use OCI SDK to list compartments or buckets

            await Task.Delay(1000); // Simulate API call

            // For demo purposes, we'll randomly succeed/fail based on connection validity
            return connection.IsValid() && !string.IsNullOrEmpty(connection.Name);
        }

        /// <summary>
        /// Connects to a cloud provider and updates connection status
        /// </summary>
        public async Task<bool> ConnectAsync(string connectionId)
        {
            var connection = GetConnection(connectionId);
            if (connection == null) return false;

            connection.Status = ConnectionStatus.Connecting;

            var (success, errorMessage) = await TestConnectionAsync(connection);

            if (success)
            {
                SaveConnections(); // Save updated status
            }

            return success;
        }

        /// <summary>
        /// Disconnects from a cloud provider
        /// </summary>
        public void Disconnect(string connectionId)
        {
            var connection = GetConnection(connectionId);
            if (connection == null) return;

            connection.Status = ConnectionStatus.Disconnected;
            connection.ErrorMessage = string.Empty;
            SaveConnections();
        }

        /// <summary>
        /// Creates a new connection instance for the specified provider
        /// </summary>
        public CloudConnectionBase CreateConnection(CloudProvider provider)
        {
            return provider switch
            {
                CloudProvider.AWS => new AwsConnection { Name = "New AWS Connection" },
                CloudProvider.GoogleCloud => new GoogleCloudConnection { Name = "New Google Cloud Connection" },
                CloudProvider.Azure => new AzureConnection { Name = "New Azure Connection" },
                CloudProvider.Oracle => new OracleConnection { Name = "New Oracle Connection" },
                _ => throw new ArgumentException($"Unsupported provider: {provider}")
            };
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
                var connectionDtos = JsonSerializer.Deserialize<List<ConnectionDto>>(jsonContent);

                if (connectionDtos == null) return;

                foreach (var dto in connectionDtos)
                {
                    var connection = DeserializeConnection(dto);
                    if (connection != null)
                    {
                        // Reset status to disconnected on startup
                        connection.Status = ConnectionStatus.Disconnected;
                        _connections.Add(connection);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the application
                Console.WriteLine($"Error loading connections: {ex.Message}");
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
                Console.WriteLine($"Error saving connections: {ex.Message}");
            }
        }

        /// <summary>
        /// Serializes a connection to a DTO for JSON storage
        /// </summary>
        private ConnectionDto SerializeConnection(CloudConnectionBase connection)
        {
            return new ConnectionDto
            {
                Id = connection.Id,
                Name = connection.Name,
                Description = connection.Description,
                Provider = connection.Provider,
                LastConnected = connection.LastConnected,
                ConfigurationJson = JsonSerializer.Serialize(connection, connection.GetType())
            };
        }

        /// <summary>
        /// Deserializes a connection from a DTO
        /// </summary>
        private CloudConnectionBase? DeserializeConnection(ConnectionDto dto)
        {
            try
            {
                var type = dto.Provider switch
                {
                    CloudProvider.AWS => typeof(AwsConnection),
                    CloudProvider.GoogleCloud => typeof(GoogleCloudConnection),
                    CloudProvider.Azure => typeof(AzureConnection),
                    CloudProvider.Oracle => typeof(OracleConnection),
                    _ => null
                };

                if (type == null) return null;

                return JsonSerializer.Deserialize(dto.ConfigurationJson, type) as CloudConnectionBase;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// DTO for JSON serialization
        /// </summary>
        private class ConnectionDto
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public CloudProvider Provider { get; set; }
            public DateTime LastConnected { get; set; }
            public string ConfigurationJson { get; set; } = string.Empty;
        }
    }
}
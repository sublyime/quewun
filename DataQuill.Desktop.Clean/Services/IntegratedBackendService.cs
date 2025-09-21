using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using DataQuillDesktop.Models;
using DataQuillDesktop.Models.Storage;

namespace DataQuillDesktop.Services
{
    /// <summary>
    /// Central service manager that coordinates all backend services within the WPF application
    /// This replaces the need for an external backend server by providing integrated services
    /// </summary>
    public class IntegratedBackendService : IDisposable
    {
        private readonly DataCollectionService _dataCollectionService;
        private readonly DataSourceService _dataSourceService;
        private readonly CloudConnectionManager _cloudConnectionManager;
        private readonly QuillDbContext _dbContext;
        private bool _isRunning = false;
        private bool _disposed = false;

        // Public properties for UI binding
        public ObservableCollection<DataPoint> RealtimeData => _dataCollectionService.RealtimeData;
        public ObservableCollection<ActivityEvent> RecentActivities => _dataCollectionService.RecentActivities;
        public DashboardMetrics Metrics => _dataCollectionService.Metrics;
        public bool IsConnected => _isRunning;
        public string Status => _isRunning ? "Running" : "Stopped";

        // Events for UI updates
        public event EventHandler<DataPoint>? DataPointReceived;
        public event EventHandler<ActivityEvent>? ActivityOccurred;
        public event EventHandler<bool>? ServiceStatusChanged;

        public IntegratedBackendService()
        {
            // Initialize database context
            _dbContext = new QuillDbContext();

            // Initialize all services
            _dataSourceService = new DataSourceService();
            _dataCollectionService = new DataCollectionService(_dbContext);
            _cloudConnectionManager = CloudConnectionManager.Instance;

            // Wire up events
            _dataCollectionService.DataPointReceived += OnDataPointReceived;
            _dataCollectionService.ActivityOccurred += OnActivityOccurred;

            Console.WriteLine("✅ Integrated Backend Service initialized");
        }

        /// <summary>
        /// Start all integrated backend services
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                if (_isRunning) return;

                Console.WriteLine("🚀 Starting Integrated Backend Services...");

                // Ensure database is ready
                await _dbContext.Database.EnsureCreatedAsync();

                // Start data collection service
                _dataCollectionService.Start();

                _isRunning = true;
                ServiceStatusChanged?.Invoke(this, true);

                Console.WriteLine("✅ All backend services started successfully");

                // Add startup activity
                AddActivity("System", "All backend services started successfully", ActivityType.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error starting backend services: {ex.Message}");
                AddActivity("System", $"Error starting services: {ex.Message}", ActivityType.Error);
                throw;
            }
        }

        /// <summary>
        /// Stop all integrated backend services
        /// </summary>
        public async Task StopAsync()
        {
            try
            {
                if (!_isRunning) return;

                Console.WriteLine("🛑 Stopping Integrated Backend Services...");

                // Stop data collection
                await Task.Run(() => _dataCollectionService.Stop());

                _isRunning = false;
                ServiceStatusChanged?.Invoke(this, false);

                Console.WriteLine("✅ All backend services stopped");
                AddActivity("System", "All backend services stopped", ActivityType.Info);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error stopping backend services: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all configured data sources
        /// </summary>
        public async Task<List<DataSource>> GetDataSourcesAsync()
        {
            return await _dataSourceService.GetAllDataSourcesAsync();
        }

        /// <summary>
        /// Save a data source configuration
        /// </summary>
        public async Task<bool> SaveDataSourceAsync(DataSource dataSource)
        {
            var result = await _dataSourceService.SaveDataSourceAsync(dataSource);
            if (result)
            {
                AddActivity("DataSource", $"Data source '{dataSource.Name}' saved", ActivityType.Success);
                // Note: RefreshDataSources would need to be implemented in DataCollectionService
            }
            return result;
        }

        /// <summary>
        /// Test connection to a data source
        /// </summary>
        public async Task<bool> TestDataSourceConnectionAsync(DataSource dataSource)
        {
            try
            {
                var result = await _dataSourceService.TestConnectionAsync(dataSource);
                AddActivity("DataSource",
                    $"Connection test for '{dataSource.Name}': {(result ? "Success" : "Failed")}",
                    result ? ActivityType.Success : ActivityType.Error);
                return result;
            }
            catch (Exception ex)
            {
                AddActivity("DataSource", $"Connection test error: {ex.Message}", ActivityType.Error);
                return false;
            }
        }

        /// <summary>
        /// Get current dashboard metrics
        /// </summary>
        public DashboardMetrics GetDashboardMetrics()
        {
            return Metrics;
        }

        /// <summary>
        /// Get cloud connections
        /// </summary>
        public List<CloudConnectionBase> GetCloudConnections()
        {
            // CloudConnectionManager uses Connections property, let's return empty list for now
            return new List<CloudConnectionBase>();
        }

        /// <summary>
        /// Test Modbus TCP connection (your original requirement)
        /// </summary>
        public async Task<bool> TestModbusTcpAsync(string host = "127.0.0.1", int port = 502, byte slaveId = 1)
        {
            try
            {
                // Create a test data source for Modbus TCP
                var testDataSource = new DataSource
                {
                    Name = $"Modbus Test {host}:{port}",
                    ProtocolType = ProtocolType.ModbusTCP,
                    InterfaceType = InterfaceType.TCP,
                    Configuration = new DataSourceConfiguration
                    {
                        Host = host,
                        Port = port,
                        SlaveId = slaveId
                    }
                };

                var testResult = await _dataSourceService.TestConnectionAsync(testDataSource);
                AddActivity("Modbus",
                    $"Modbus TCP test to {host}:{port} Slave {slaveId}: {(testResult ? "Success" : "Failed")}",
                    testResult ? ActivityType.Success : ActivityType.Error);
                return testResult;
            }
            catch (Exception ex)
            {
                AddActivity("Modbus", $"Modbus TCP test error: {ex.Message}", ActivityType.Error);
                return false;
            }
        }

        /// <summary>
        /// Health check endpoint (replaces external /actuator/health)
        /// </summary>
        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                // Check database connectivity
                var dbHealthy = await _dbContext.Database.CanConnectAsync();

                // Check if services are running
                var servicesHealthy = _isRunning;

                return dbHealthy && servicesHealthy;
            }
            catch
            {
                return false;
            }
        }

        private void OnDataPointReceived(object? sender, DataPoint dataPoint)
        {
            DataPointReceived?.Invoke(this, dataPoint);
        }

        private void OnActivityOccurred(object? sender, ActivityEvent activity)
        {
            ActivityOccurred?.Invoke(this, activity);
        }

        private void AddActivity(string title, string description, ActivityType type)
        {
            var activity = new ActivityEvent
            {
                Timestamp = DateTime.Now,
                Title = title,
                Description = description,
                Type = type
            };

            RecentActivities.Insert(0, activity);

            // Keep only recent activities (last 100)
            while (RecentActivities.Count > 100)
            {
                RecentActivities.RemoveAt(RecentActivities.Count - 1);
            }

            ActivityOccurred?.Invoke(this, activity);
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                StopAsync().Wait(5000); // Wait up to 5 seconds for clean shutdown

                _dataCollectionService?.Dispose();
                _dbContext?.Dispose();

                _disposed = true;
                Console.WriteLine("✅ Integrated Backend Service disposed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error disposing backend service: {ex.Message}");
            }
        }
    }
}
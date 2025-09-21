using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using DataQuillDesktop.Models;
using Microsoft.EntityFrameworkCore;
using FluentModbus;

namespace DataQuillDesktop.Services
{
    public class DataSourceService
    {
        private readonly QuillDbContext? _context;
        private readonly List<DataSource> _inMemoryDataSources;
        private bool _useDatabaseMode;

        public DataSourceService()
        {
            _inMemoryDataSources = new List<DataSource>();
            try
            {
                _context = new QuillDbContext();
                // Test database connection
                _context.Database.CanConnect();
                _useDatabaseMode = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection failed, using in-memory mode: {ex.Message}");
                _context = null;
                _useDatabaseMode = false;
            }
        }

        public async Task<List<DataSource>> GetAllDataSourcesAsync()
        {
            try
            {
                if (_useDatabaseMode && _context != null)
                {
                    return await _context.DataSources.ToListAsync();
                }
                else
                {
                    // Return in-memory data sources
                    return new List<DataSource>(_inMemoryDataSources);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving data sources: {ex.Message}");
                return new List<DataSource>();
            }
        }

        public async Task<DataSource?> GetDataSourceByIdAsync(int id)
        {
            try
            {
                if (_context == null)
                    return null;

                return await _context.DataSources.FindAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data source {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SaveDataSourceAsync(DataSource dataSource)
        {
            try
            {
                if (_useDatabaseMode && _context != null)
                {
                    if (dataSource.Id == 0)
                    {
                        _context.DataSources.Add(dataSource);
                    }
                    else
                    {
                        _context.DataSources.Update(dataSource);
                    }

                    await _context.SaveChangesAsync();
                }
                else
                {
                    // In-memory mode
                    if (dataSource.Id == 0)
                    {
                        // Assign a new ID
                        dataSource.Id = _inMemoryDataSources.Count > 0 ? _inMemoryDataSources.Max(ds => ds.Id) + 1 : 1;
                        _inMemoryDataSources.Add(dataSource);
                    }
                    else
                    {
                        // Update existing
                        var existing = _inMemoryDataSources.FirstOrDefault(ds => ds.Id == dataSource.Id);
                        if (existing != null)
                        {
                            var index = _inMemoryDataSources.IndexOf(existing);
                            _inMemoryDataSources[index] = dataSource;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data source: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteDataSourceAsync(int id)
        {
            try
            {
                if (_useDatabaseMode && _context != null)
                {
                    var dataSource = await _context.DataSources.FindAsync(id);
                    if (dataSource != null)
                    {
                        _context.DataSources.Remove(dataSource);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }
                else
                {
                    // In-memory mode
                    var dataSource = _inMemoryDataSources.FirstOrDefault(ds => ds.Id == id);
                    if (dataSource != null)
                    {
                        _inMemoryDataSources.Remove(dataSource);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting data source {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> TestConnectionAsync(DataSource dataSource)
        {
            try
            {
                // Basic validation first
                if (string.IsNullOrEmpty(dataSource.Name))
                    throw new ArgumentException("Data source name is required");

                // Test connection based on interface and protocol type
                switch (dataSource.InterfaceType)
                {
                    case InterfaceType.File:
                        return await TestFileConnectionAsync(dataSource);

                    case InterfaceType.TCP:
                        return await TestTcpConnectionAsync(dataSource);

                    case InterfaceType.UDP:
                        return await TestUdpConnectionAsync(dataSource);

                    case InterfaceType.Serial:
                        return await TestSerialConnectionAsync(dataSource);

                    case InterfaceType.USB:
                        return await TestUsbConnectionAsync(dataSource);

                    default:
                        throw new NotSupportedException($"Interface type {dataSource.InterfaceType} is not supported");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> TestFileConnectionAsync(DataSource dataSource)
        {
            var config = dataSource.Configuration;

            if (string.IsNullOrEmpty(config.FilePath))
                throw new ArgumentException("File path is required");

            if (!File.Exists(config.FilePath))
                throw new FileNotFoundException($"File not found: {config.FilePath}");

            // Try to read the file
            using var fileStream = File.OpenRead(config.FilePath);
            var buffer = new byte[1024];
            var bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, buffer.Length));

            if (bytesRead > 0)
                Console.WriteLine($"Successfully read {bytesRead} bytes from file");

            return true;
        }

        private async Task<bool> TestTcpConnectionAsync(DataSource dataSource)
        {
            var config = dataSource.Configuration;

            if (string.IsNullOrEmpty(config.Host))
                throw new ArgumentException("Host is required for TCP connection");

            if (config.Port <= 0)
                throw new ArgumentException("Valid port number is required for TCP connection");

            // Check if this is a Modbus TCP connection
            if (dataSource.ProtocolType == Models.ProtocolType.ModbusTCP)
            {
                return await TestModbusTcpConnectionAsync(dataSource);
            }

            // For other TCP protocols, use basic TCP connection test
            using var client = new System.Net.Sockets.TcpClient();
            var connectTask = client.ConnectAsync(config.Host, config.Port);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(config.Timeout));

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask == timeoutTask)
                throw new TimeoutException($"Connection to {config.Host}:{config.Port} timed out");

            if (connectTask.IsFaulted)
                throw connectTask.Exception?.InnerException ?? new Exception("TCP connection failed");

            return client.Connected;
        }

        private async Task<bool> TestModbusTcpConnectionAsync(DataSource dataSource)
        {
            var config = dataSource.Configuration;

            Console.WriteLine($"Testing Modbus TCP connection to {config.Host}:{config.Port}, Slave ID: {config.SlaveId}");

            try
            {
                await Task.Delay(100); // Simulate connection delay

                using var client = new ModbusTcpClient();

                // Set timeout
                client.ReadTimeout = config.Timeout * 1000; // Convert to milliseconds
                client.WriteTimeout = config.Timeout * 1000;

                Console.WriteLine($"Connecting to Modbus TCP server...");

                // Connect to the Modbus TCP server
                client.Connect(new System.Net.IPEndPoint(
                    System.Net.IPAddress.Parse(config.Host),
                    config.Port));

                Console.WriteLine($"Connected! Testing communication with slave {config.SlaveId}...");

                // Try to read a holding register to verify communication
                // Reading 1 register starting from address 0
                var result = client.ReadHoldingRegisters<ushort>(
                    unitIdentifier: (byte)config.SlaveId,
                    startingAddress: 0,
                    count: 1);

                Console.WriteLine($"Successfully read holding register 0: {result[0]}");
                Console.WriteLine($"Modbus TCP connection test SUCCESSFUL!");

                return true;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Console.WriteLine($"Socket connection failed: {ex.Message}");
                throw new Exception($"Cannot connect to {config.Host}:{config.Port} - {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"Modbus operation timed out: {ex.Message}");
                throw new Exception($"Modbus TCP timeout - check if slave {config.SlaveId} is responding");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Modbus TCP test failed: {ex.Message}");
                throw new Exception($"Modbus TCP communication failed: {ex.Message}");
            }
        }

        private async Task<bool> TestUdpConnectionAsync(DataSource dataSource)
        {
            var config = dataSource.Configuration;

            if (string.IsNullOrEmpty(config.Host))
                throw new ArgumentException("Host is required for UDP connection");

            if (config.Port <= 0)
                throw new ArgumentException("Valid port number is required for UDP connection");

            using var client = new System.Net.Sockets.UdpClient();
            try
            {
                client.Connect(config.Host, config.Port);

                // Send a test packet
                var testData = System.Text.Encoding.UTF8.GetBytes("TEST");
                await client.SendAsync(testData, testData.Length);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"UDP connection test failed: {ex.Message}");
            }
        }

        private async Task<bool> TestSerialConnectionAsync(DataSource dataSource)
        {
            var config = dataSource.Configuration;

            if (string.IsNullOrEmpty(config.PortName))
                throw new ArgumentException("Port name is required for Serial connection");

            try
            {
                using var serialPort = new System.IO.Ports.SerialPort(
                    config.PortName,
                    config.BaudRate,
                    (System.IO.Ports.Parity)Enum.Parse(typeof(System.IO.Ports.Parity), config.Parity),
                    config.DataBits,
                    (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), config.StopBits));

                serialPort.Open();
                await Task.Delay(100); // Brief delay to ensure port is ready
                serialPort.Close();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Serial connection test failed: {ex.Message}");
            }
        }

        private async Task<bool> TestUsbConnectionAsync(DataSource dataSource)
        {
            // USB connection testing would require specific USB device libraries
            // For now, just return true as a placeholder
            await Task.Delay(100);
            return true;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
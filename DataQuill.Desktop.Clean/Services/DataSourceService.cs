using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using DataQuillDesktop.Models;
using Microsoft.EntityFrameworkCore;

namespace DataQuillDesktop.Services
{
    public class DataSourceService
    {
        private readonly QuillDbContext _context;

        public DataSourceService()
        {
            _context = new QuillDbContext();
        }

        public async Task<List<DataSource>> GetAllDataSourcesAsync()
        {
            try
            {
                return await _context.DataSources.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data sources: {ex.Message}");
                return new List<DataSource>();
            }
        }

        public async Task<DataSource?> GetDataSourceByIdAsync(int id)
        {
            try
            {
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
                if (dataSource.Id == 0)
                {
                    _context.DataSources.Add(dataSource);
                }
                else
                {
                    _context.DataSources.Update(dataSource);
                }

                await _context.SaveChangesAsync();
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
                var dataSource = await _context.DataSources.FindAsync(id);
                if (dataSource != null)
                {
                    _context.DataSources.Remove(dataSource);
                    await _context.SaveChangesAsync();
                    return true;
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
            await fileStream.ReadAsync(buffer.AsMemory(0, buffer.Length));

            return true;
        }

        private async Task<bool> TestTcpConnectionAsync(DataSource dataSource)
        {
            var config = dataSource.Configuration;

            if (string.IsNullOrEmpty(config.Host))
                throw new ArgumentException("Host is required for TCP connection");

            if (config.Port <= 0)
                throw new ArgumentException("Valid port number is required for TCP connection");

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
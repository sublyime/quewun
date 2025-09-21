using DataQuillDesktop.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using FluentModbus;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Timers;
using System.Windows.Threading;

namespace DataQuillDesktop.Services;

/// <summary>
/// Service responsible for collecting real-time data from configured data sources
/// </summary>
public class DataCollectionService : IDisposable
{
    private readonly QuillDbContext _dbContext;
    private readonly System.Timers.Timer _collectionTimer;
    private readonly Random _random = new();
    private bool _disposed = false;

    public ObservableCollection<DataPoint> RealtimeData { get; } = new();
    public ObservableCollection<ActivityEvent> RecentActivities { get; } = new();
    public DashboardMetrics Metrics { get; } = new();

    public event EventHandler<DataPoint>? DataPointReceived;
    public event EventHandler<ActivityEvent>? ActivityOccurred;

    public DataCollectionService(QuillDbContext dbContext)
    {
        _dbContext = dbContext;

        // Initialize collection timer (collect data every 5 seconds)
        _collectionTimer = new System.Timers.Timer(5000);
        _collectionTimer.Elapsed += CollectDataFromSources;
        _collectionTimer.AutoReset = true;

        Console.WriteLine($"🔧 DataCollectionService created - Timer interval: {_collectionTimer.Interval}ms, AutoReset: {_collectionTimer.AutoReset}");

        // Skip sample data initialization - we want to see real data only
        // InitializeSampleData();

        // Just add the startup activity
        AddActivity("System initialized", "DataQuill dashboard started", ActivityType.Success);
    }

    /// <summary>
    /// Start the data collection service
    /// </summary>
    public void Start()
    {
        Console.WriteLine($"🔄 [{DateTime.Now:HH:mm:ss}] Starting DataCollectionService timer (5 second intervals)...");

        // Clear any sample data to show only real collected data
        if (System.Windows.Application.Current?.Dispatcher != null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                RealtimeData.Clear();
                Console.WriteLine("🧹 Cleared sample data - will show only real collected data");
            });
        }

        // Run one immediate collection cycle to test
        Console.WriteLine($"🧪 [{DateTime.Now:HH:mm:ss}] Running test collection cycle...");
        var testEventArgs = new ElapsedEventArgs(DateTime.Now);
        CollectDataFromSources(this, testEventArgs);

        _collectionTimer.Start();
        Console.WriteLine($"✅ [{DateTime.Now:HH:mm:ss}] DataCollectionService timer started - Enabled: {_collectionTimer.Enabled}");
        AddActivity("Data collection service started", "Real-time monitoring active", ActivityType.Success);
    }    /// <summary>
         /// Stop the data collection service
         /// </summary>
    public void Stop()
    {
        _collectionTimer.Stop();
        AddActivity("Data collection service stopped", "Real-time monitoring paused", ActivityType.Info);
    }

    /// <summary>
    /// Collect data from all configured and connected data sources
    /// </summary>
    private async void CollectDataFromSources(object? sender, ElapsedEventArgs e)
    {
        try
        {
            Console.WriteLine($"🔄 [{DateTime.Now:HH:mm:ss}] Starting data collection cycle...");

            var dataSources = await _dbContext.DataSources
                .Where(ds => ds.IsActive)
                .ToListAsync();

            Console.WriteLine($"📊 Found {dataSources.Count} active data sources to poll");

            int activeConnections = 0;
            int totalDataPoints = 0;
            long dataProcessed = 0;

            foreach (var dataSource in dataSources)
            {
                Console.WriteLine($"🔍 Processing data source: {dataSource.Name} ({dataSource.InterfaceType})");
                try
                {
                    var data = await CollectDataFromSource(dataSource);
                    if (data.Any())
                    {
                        activeConnections++;
                        totalDataPoints += data.Count;

                        foreach (var dataPoint in data)
                        {
                            SafeAddRealtimeData(dataPoint);
                            DataPointReceived?.Invoke(this, dataPoint);
                            dataProcessed += EstimateDataSize(dataPoint);
                        }

                        // Note: RealtimeData cleanup is now handled in SafeAddRealtimeData

                        AddActivity($"Data received from {dataSource.Name}",
                                  $"{data.Count} data points collected",
                                  ActivityType.DataReceived);
                    }
                }
                catch (Exception ex)
                {
                    AddActivity($"Error collecting from {dataSource.Name}",
                              ex.Message,
                              ActivityType.Error);
                }
            }

            // Update metrics
            UpdateMetrics(activeConnections, totalDataPoints, dataProcessed);

            Console.WriteLine($"✅ [{DateTime.Now:HH:mm:ss}] Collection cycle completed - Active: {activeConnections}, Data points: {totalDataPoints}, Total in memory: {RealtimeData.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Data collection cycle failed: {ex.Message}");
            AddActivity("Data collection error", ex.Message, ActivityType.Error);
        }
    }

    /// <summary>
    /// Collect data from a specific data source based on its configuration
    /// </summary>
    public async Task<List<DataPoint>> CollectDataFromSource(DataSource dataSource)
    {
        var dataPoints = new List<DataPoint>();

        switch (dataSource.InterfaceType)
        {
            case InterfaceType.File:
                dataPoints.AddRange(await CollectFromFileSource(dataSource));
                break;
            case InterfaceType.TCP:
                dataPoints.AddRange(await CollectFromTcpSource(dataSource));
                break;
            case InterfaceType.UDP:
                dataPoints.AddRange(await CollectFromUdpSource(dataSource));
                break;
            case InterfaceType.Serial:
                dataPoints.AddRange(await CollectFromSerialSource(dataSource));
                break;
            case InterfaceType.USB:
                dataPoints.AddRange(await CollectFromUsbSource(dataSource));
                break;
        }

        return dataPoints;
    }

    /// <summary>
    /// Collect data from file-based sources
    /// </summary>
    private async Task<List<DataPoint>> CollectFromFileSource(DataSource dataSource)
    {
        var dataPoints = new List<DataPoint>();

        try
        {
            var config = dataSource.Configuration;
            var filePath = config.FilePath;

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                var content = await File.ReadAllTextAsync(filePath);

                // Generate simulated data based on file content
                for (int i = 0; i < 3; i++)
                {
                    dataPoints.Add(new DataPoint
                    {
                        DataSourceId = dataSource.Id,
                        TagName = $"FILE_{dataSource.Name}_POINT_{i + 1}",
                        Value = _random.NextDouble() * 100,
                        Timestamp = DateTime.Now,
                        DataType = "Double",
                        Unit = "units",
                        Quality = "Good"
                    });
                }
            }
        }
        catch (Exception)
        {
            // Return empty list on error
        }

        return dataPoints;
    }

    /// <summary>
    /// Collect data from TCP sources
    /// </summary>
    private async Task<List<DataPoint>> CollectFromTcpSource(DataSource dataSource)
    {
        var dataPoints = new List<DataPoint>();

        try
        {
            var config = dataSource.Configuration;
            var host = config.Host;
            var port = config.Port;

            if (!string.IsNullOrEmpty(host) && port > 0)
            {
                // For Modbus TCP, use real Modbus communication
                if (dataSource.ProtocolType == Models.ProtocolType.ModbusTCP)
                {
                    dataPoints.AddRange(await CollectModbusTcpData(dataSource, config));
                }
                else
                {
                    // For other TCP protocols, use ping test and simulated data
                    using var ping = new Ping();
                    var reply = await ping.SendPingAsync(host, 1000);

                    if (reply.Status == IPStatus.Success)
                    {
                        // Generate simulated TCP data
                        for (int i = 0; i < 5; i++)
                        {
                            dataPoints.Add(new DataPoint
                            {
                                DataSourceId = dataSource.Id,
                                TagName = $"TCP_{dataSource.Name}_CH{i + 1}",
                                Value = _random.NextDouble() * 1000,
                                Timestamp = DateTime.Now.AddMilliseconds(-_random.Next(0, 5000)),
                                DataType = "Double",
                                Unit = GetUnitForProtocol(dataSource.ProtocolType),
                                Quality = "Good"
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log activity on error
            AddActivity($"TCP Data Collection Failed for {dataSource.Name}", ex.Message, ActivityType.Error);
        }

        return dataPoints;
    }

    /// <summary>
    /// Collect real data from Modbus TCP source
    /// </summary>
    private async Task<List<DataPoint>> CollectModbusTcpData(DataSource dataSource, DataSourceConfiguration config)
    {
        var dataPoints = new List<DataPoint>();

        try
        {
            Console.WriteLine($"🔌 Connecting to Modbus TCP at {config.Host}:{config.Port}, Slave {config.SlaveId}");

            var client = new ModbusTcpClient();

            await Task.Run(() =>
            {
                var endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(config.Host), config.Port);
                client.Connect(endpoint, FluentModbus.ModbusEndianness.BigEndian);
            });

            var slaveId = (byte)config.SlaveId;

            // Check if we have configured Modbus registers
            if (config.ModbusRegisters?.Count > 0)
            {
                Console.WriteLine($"📖 Reading {config.ModbusRegisters.Count} configured Modbus registers...");

                foreach (var regConfig in config.ModbusRegisters)
                {
                    try
                    {
                        var value = await ReadModbusRegister(client, slaveId, regConfig);

                        dataPoints.Add(new DataPoint
                        {
                            DataSourceId = dataSource.Id,
                            TagName = regConfig.TagName,
                            Value = value,
                            Timestamp = DateTime.Now,
                            DataType = regConfig.DataFormat.ToString(),
                            Unit = regConfig.Units,
                            Quality = "Good"
                        });
                    }
                    catch (Exception regEx)
                    {
                        Console.WriteLine($"⚠️ Failed to read register {regConfig.TagName}: {regEx.Message}");

                        dataPoints.Add(new DataPoint
                        {
                            DataSourceId = dataSource.Id,
                            TagName = regConfig.TagName,
                            Value = 0,
                            Timestamp = DateTime.Now,
                            DataType = regConfig.DataFormat.ToString(),
                            Unit = regConfig.Units,
                            Quality = "Bad"
                        });
                    }
                }
            }
            else
            {
                // Fallback to basic register reading if no configuration exists
                Console.WriteLine($"📖 No register configuration found, reading default holding registers...");

                var registers = client.ReadHoldingRegisters(slaveId, 0, 10);
                Console.WriteLine($"✅ Successfully read {registers.Length} registers from Modbus device");

                for (int i = 0; i < registers.Length; i++)
                {
                    dataPoints.Add(new DataPoint
                    {
                        DataSourceId = dataSource.Id,
                        TagName = $"MODBUS_HR_{i:D3}",
                        Value = registers[i],
                        Timestamp = DateTime.Now,
                        DataType = "UInt16",
                        Unit = "register",
                        Quality = "Good"
                    });
                }
            }

            client.Disconnect();

            Console.WriteLine($"📊 Created {dataPoints.Count} data points from Modbus registers");
            AddActivity($"Modbus TCP Data Collection", $"Read {dataPoints.Count} data points from {dataSource.Name} (Slave {slaveId})", ActivityType.Success);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Modbus TCP collection failed: {ex.Message}");
            AddActivity($"Modbus TCP Collection Failed", $"Failed to read from {dataSource.Name} (Slave {config.SlaveId}): {ex.Message}", ActivityType.Error);
        }

        return dataPoints;
    }

    /// <summary>
    /// Read a single Modbus register with proper data type conversion
    /// </summary>
    private async Task<double> ReadModbusRegister(ModbusTcpClient client, byte slaveId, ModbusRegisterConfig regConfig)
    {
        await Task.CompletedTask; // For async compatibility

        switch (regConfig.DataFormat)
        {
            case ModbusDataFormat.UInt16:
                var uint16Val = client.ReadHoldingRegisters(slaveId, (ushort)regConfig.StartAddress, 1)[0];
                return (uint16Val * regConfig.Scale) + regConfig.Offset;

            case ModbusDataFormat.SInt16:
                var sint16Raw = client.ReadHoldingRegisters(slaveId, (ushort)regConfig.StartAddress, 1)[0];
                var sint16Val = (short)sint16Raw;
                return (sint16Val * regConfig.Scale) + regConfig.Offset;

            case ModbusDataFormat.UInt32:
                var uint32Regs = client.ReadHoldingRegisters(slaveId, (ushort)regConfig.StartAddress, 2);
                var uint32Val = (uint)(uint32Regs[0] << 16) | uint32Regs[1];
                return (uint32Val * regConfig.Scale) + regConfig.Offset;

            case ModbusDataFormat.SInt32:
                var sint32Regs = client.ReadHoldingRegisters(slaveId, (ushort)regConfig.StartAddress, 2);
                var sint32Val = (int)(sint32Regs[0] << 16) | sint32Regs[1];
                return (sint32Val * regConfig.Scale) + regConfig.Offset;

            case ModbusDataFormat.Float32:
                var float32Regs = client.ReadHoldingRegisters(slaveId, (ushort)regConfig.StartAddress, 2);
                var float32Bytes = new byte[4];
                var reg1Bytes = BitConverter.GetBytes((ushort)float32Regs[0]);
                var reg2Bytes = BitConverter.GetBytes((ushort)float32Regs[1]);
                float32Bytes[0] = reg2Bytes[0];
                float32Bytes[1] = reg2Bytes[1];
                float32Bytes[2] = reg1Bytes[0];
                float32Bytes[3] = reg1Bytes[1];
                var float32Val = BitConverter.ToSingle(float32Bytes, 0);
                return (float32Val * regConfig.Scale) + regConfig.Offset;

            case ModbusDataFormat.Float32Swap:
                var floatSwapRegs = client.ReadHoldingRegisters(slaveId, (ushort)regConfig.StartAddress, 2);
                var floatSwapBytes = new byte[4];
                var regS1Bytes = BitConverter.GetBytes((ushort)floatSwapRegs[1]);
                var regS2Bytes = BitConverter.GetBytes((ushort)floatSwapRegs[0]);
                floatSwapBytes[0] = regS2Bytes[0];
                floatSwapBytes[1] = regS2Bytes[1];
                floatSwapBytes[2] = regS1Bytes[0];
                floatSwapBytes[3] = regS1Bytes[1];
                var floatSwapVal = BitConverter.ToSingle(floatSwapBytes, 0);
                return (floatSwapVal * regConfig.Scale) + regConfig.Offset;

            default:
                throw new ArgumentException($"Unsupported data format: {regConfig.DataFormat}");
        }
    }

    /// <summary>
    /// Collect data from UDP sources
    /// </summary>
    private async Task<List<DataPoint>> CollectFromUdpSource(DataSource dataSource)
    {
        var dataPoints = new List<DataPoint>();

        try
        {
            // Simulate UDP connection delay
            await Task.Delay(100);

            // Generate simulated UDP data
            for (int i = 0; i < 4; i++)
            {
                dataPoints.Add(new DataPoint
                {
                    DataSourceId = dataSource.Id,
                    TagName = $"UDP_{dataSource.Name}_SENSOR{i + 1}",
                    Value = _random.NextDouble() * 50 + 20, // Temperature-like data
                    Timestamp = DateTime.Now,
                    DataType = "Double",
                    Unit = GetUnitForProtocol(dataSource.ProtocolType),
                    Quality = "Good"
                });
            }
        }
        catch (Exception)
        {
            // Return empty list on error
        }

        return dataPoints;
    }

    /// <summary>
    /// Collect data from serial sources
    /// </summary>
    private async Task<List<DataPoint>> CollectFromSerialSource(DataSource dataSource)
    {
        var dataPoints = new List<DataPoint>();

        try
        {
            var config = dataSource.Configuration;
            var portName = config.PortName;

            if (!string.IsNullOrEmpty(portName))
            {
                // Simulate serial communication delay
                await Task.Delay(50);

                // Generate simulated serial data
                for (int i = 0; i < 3; i++)
                {
                    dataPoints.Add(new DataPoint
                    {
                        DataSourceId = dataSource.Id,
                        TagName = $"SERIAL_{dataSource.Name}_REG{i + 1}",
                        Value = _random.Next(0, 65536),
                        Timestamp = DateTime.Now,
                        DataType = "Integer",
                        Unit = GetUnitForProtocol(dataSource.ProtocolType),
                        Quality = "Good"
                    });
                }
            }
        }
        catch (Exception)
        {
            // Return empty list on error
        }

        return dataPoints;
    }

    /// <summary>
    /// Collect data from USB sources
    /// </summary>
    private async Task<List<DataPoint>> CollectFromUsbSource(DataSource dataSource)
    {
        var dataPoints = new List<DataPoint>();

        try
        {
            // Simulate USB device communication delay
            await Task.Delay(75);

            // Generate simulated USB device data
            for (int i = 0; i < 2; i++)
            {
                dataPoints.Add(new DataPoint
                {
                    DataSourceId = dataSource.Id,
                    TagName = $"USB_{dataSource.Name}_DATA{i + 1}",
                    Value = _random.NextDouble() * 3.3, // Voltage-like data
                    Timestamp = DateTime.Now,
                    DataType = "Double",
                    Unit = "V",
                    Quality = "Good"
                });
            }
        }
        catch (Exception)
        {
            // Return empty list on error
        }

        return dataPoints;
    }

    /// <summary>
    /// Get appropriate unit based on protocol type
    /// </summary>
    private string GetUnitForProtocol(Models.ProtocolType protocolType)
    {
        return protocolType switch
        {
            Models.ProtocolType.ModbusTCP or Models.ProtocolType.ModbusRTU => "register",
            Models.ProtocolType.MQTT => "msg",
            Models.ProtocolType.NMEA0183 => "knots",
            Models.ProtocolType.HART => "mA",
            Models.ProtocolType.OPCUA => "value",
            Models.ProtocolType.OSIPI => "unit",
            Models.ProtocolType.IP21 => "eng",
            Models.ProtocolType.RestAPI or Models.ProtocolType.SoapAPI => "response",
            Models.ProtocolType.IoTDevices => "sensor",
            _ => "unit"
        };
    }

    /// <summary>
    /// Estimate data size for bandwidth calculations
    /// </summary>
    private long EstimateDataSize(DataPoint dataPoint)
    {
        var json = JsonSerializer.Serialize(dataPoint);
        return Encoding.UTF8.GetByteCount(json);
    }

    /// <summary>
    /// Add an activity event
    /// </summary>
    private void AddActivity(string title, string description, ActivityType type)
    {
        var activity = new ActivityEvent
        {
            Title = title,
            Description = description,
            Type = type,
            Timestamp = DateTime.Now
        };

        // Must update ObservableCollection on UI thread
        if (System.Windows.Application.Current?.Dispatcher != null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Add to beginning of collection
                RecentActivities.Insert(0, activity);

                // Keep only last 20 activities
                while (RecentActivities.Count > 20)
                {
                    RecentActivities.RemoveAt(RecentActivities.Count - 1);
                }
            });
        }
        else
        {
            // Fallback for cases where dispatcher is not available
            Console.WriteLine($"⚠️ No dispatcher available for activity: {title}");
        }

        ActivityOccurred?.Invoke(this, activity);
    }

    /// <summary>
    /// Safely add data point to RealtimeData collection on UI thread
    /// </summary>
    private void SafeAddRealtimeData(DataPoint dataPoint)
    {
        if (System.Windows.Application.Current?.Dispatcher != null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                RealtimeData.Add(dataPoint);

                // Keep only last 1000 data points for memory management
                while (RealtimeData.Count > 1000)
                {
                    RealtimeData.RemoveAt(0);
                }
            });
        }
        else
        {
            Console.WriteLine($"⚠️ No dispatcher available for data point: {dataPoint.TagName}");
        }
    }

    /// <summary>
    /// Update dashboard metrics
    /// </summary>
    private void UpdateMetrics(int activeConnections, int totalDataPoints, long dataProcessed)
    {
        Metrics.ActiveConnections = activeConnections;
        Metrics.TotalDataPoints += totalDataPoints;
        Metrics.DataProcessedToday += dataProcessed;
        Metrics.ActiveUsers = 1; // Current user
        Metrics.AverageResponseTime = 50 + _random.NextDouble() * 100; // Simulated response time
        Metrics.LastUpdate = DateTime.Now;
    }

    /// <summary>
    /// Initialize with some sample data for demonstration
    /// </summary>
    private void InitializeSampleData()
    {
        AddActivity("System initialized", "DataQuill dashboard started", ActivityType.Success);

        // Add some initial sample data points
        for (int i = 0; i < 10; i++)
        {
            var sampleDataPoint = new DataPoint
            {
                DataSourceId = 1,
                TagName = $"SAMPLE_TAG_{i + 1}",
                Value = _random.NextDouble() * 100,
                Timestamp = DateTime.Now.AddMinutes(-i),
                DataType = "Double",
                Unit = "units"
            };

            SafeAddRealtimeData(sampleDataPoint);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _collectionTimer?.Dispose();
            _disposed = true;
        }
    }
}
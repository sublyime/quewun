using Microsoft.EntityFrameworkCore;
using DataQuillDesktop.Models;
using DataQuillDesktop.Services;
using DataQuillDesktop;
using System.Text.Json;

// Configure to use SQLite
DatabaseConfigurationService.SetProvider(DatabaseConfigurationService.DatabaseProvider.SQLite);

Console.WriteLine("🔍 Checking database for Modbus configurations...");

using var context = new QuillDbContext();

// Ensure database is created
context.Database.EnsureCreated();

// Check for data sources
var dataSources = await context.DataSources
    .Include(ds => ds.Configuration)
    .ToListAsync();

Console.WriteLine($"📊 Found {dataSources.Count} data sources in database:");

foreach (var dataSource in dataSources)
{
    Console.WriteLine($"  - {dataSource.Name} ({dataSource.ProtocolType})");

    if (dataSource.Configuration?.ModbusRegisters?.Count > 0)
    {
        Console.WriteLine($"    Modbus registers: {dataSource.Configuration.ModbusRegisters.Count}");
        foreach (var reg in dataSource.Configuration.ModbusRegisters)
        {
            Console.WriteLine($"      {reg.TagName}: Address {reg.StartAddress}, Format {reg.DataFormat}, Scale {reg.Scale}, Units {reg.Units}");
        }
    }
    else
    {
        Console.WriteLine($"    No Modbus register configuration found");
    }
}

// If no data sources exist, create sample configuration
if (dataSources.Count == 0)
{
    Console.WriteLine("🔧 Creating sample Modbus configuration...");

    var modbusSource = new DataSource
    {
        Name = "Sample Modbus TCP Device",
        Description = "Example Modbus TCP device with configured registers",
        InterfaceType = InterfaceType.TCP,
        ProtocolType = ProtocolType.ModbusTCP,
        IsActive = true,
        CreatedAt = DateTime.Now,
        LastUpdated = DateTime.Now,
        Configuration = new DataSourceConfiguration
        {
            Host = "127.0.0.1",
            Port = 502,
            SlaveId = 1,
            Timeout = 5000,
            ModbusRegisters = new List<ModbusRegisterConfig>
            {
                new() { TagName = "Temperature", StartAddress = 0, DataFormat = ModbusDataFormat.SInt16, Scale = 0.1, Units = "°C", Description = "Ambient temperature" },
                new() { TagName = "Pressure", StartAddress = 1, DataFormat = ModbusDataFormat.UInt16, Scale = 1.0, Units = "PSI", Description = "System pressure" },
                new() { TagName = "Flow_Rate", StartAddress = 2, DataFormat = ModbusDataFormat.Float32, Scale = 1.0, Units = "GPM", Description = "Flow rate in gallons per minute" },
                new() { TagName = "Motor_Speed", StartAddress = 4, DataFormat = ModbusDataFormat.UInt32, Scale = 1.0, Units = "RPM", Description = "Motor rotation speed" },
                new() { TagName = "Power_Factor", StartAddress = 6, DataFormat = ModbusDataFormat.Float32Swap, Scale = 1.0, Units = "%", Description = "Power factor percentage" }
            }
        }
    };

    context.DataSources.Add(modbusSource);
    await context.SaveChangesAsync();

    Console.WriteLine("✅ Sample Modbus configuration created successfully!");
}

Console.WriteLine("✅ Database check completed. Press any key to exit...");
Console.ReadKey();
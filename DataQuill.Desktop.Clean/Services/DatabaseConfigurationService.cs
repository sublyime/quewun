using Microsoft.EntityFrameworkCore;
using DataQuillDesktop.Models;
using System;
using System.Linq;

namespace DataQuillDesktop.Services
{
    public class DatabaseConfigurationService
    {
        private static string? _connectionString;
        private static DatabaseProvider _provider = DatabaseProvider.SQLite;

        public enum DatabaseProvider
        {
            SQLite,
            PostgreSQL
        }

        public static void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                switch (_provider)
                {
                    case DatabaseProvider.PostgreSQL:
                        var pgConnectionString = _connectionString ?? "Host=localhost;Database=quilldb;Username=quilluser;Password=ala1nna";
                        optionsBuilder.UseNpgsql(pgConnectionString);
                        Console.WriteLine("✅ Using PostgreSQL database");
                        break;

                    case DatabaseProvider.SQLite:
                    default:
                        var sqliteConnectionString = _connectionString ?? "Data Source=quilldb.sqlite";
                        optionsBuilder.UseSqlite(sqliteConnectionString);
                        Console.WriteLine("✅ Using SQLite database (fallback)");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database configuration failed: {ex.Message}");
                // Fallback to SQLite in memory
                optionsBuilder.UseSqlite("Data Source=:memory:");
                Console.WriteLine("🔄 Using in-memory SQLite as last resort");
            }
        }

        public static bool TestConnection()
        {
            try
            {
                using var context = new QuillDbContext();
                context.Database.CanConnect();
                Console.WriteLine("✅ Database connection successful");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection failed: {ex.Message}");
                return false;
            }
        }

        public static void SetProvider(DatabaseProvider provider, string? connectionString = null)
        {
            _provider = provider;
            _connectionString = connectionString;
            Console.WriteLine($"Database provider set to: {provider}");
        }

        public static void EnsureDatabaseCreated()
        {
            try
            {
                using var context = new QuillDbContext();
                context.Database.EnsureCreated();
                Console.WriteLine("✅ Database schema ensured");

                // Set up sample Modbus configurations if none exist
                SetupSampleModbusConfigurations(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to create database schema: {ex.Message}");
            }
        }

        private static void SetupSampleModbusConfigurations(QuillDbContext context)
        {
            try
            {
                // Check if we already have Modbus data sources
                var existingModbus = context.DataSources
                    .Any(ds => ds.ProtocolType == DataQuillDesktop.Models.ProtocolType.ModbusTCP);

                if (!existingModbus)
                {
                    Console.WriteLine("🔧 Setting up sample Modbus TCP configuration...");

                    var modbusSource = new DataQuillDesktop.Models.DataSource
                    {
                        Name = "Sample Modbus TCP Device",
                        Description = "Example Modbus TCP device with configured registers",
                        InterfaceType = DataQuillDesktop.Models.InterfaceType.TCP,
                        ProtocolType = DataQuillDesktop.Models.ProtocolType.ModbusTCP,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now,
                        Configuration = new DataQuillDesktop.Models.DataSourceConfiguration
                        {
                            Host = "127.0.0.1",
                            Port = 502,
                            SlaveId = 1,
                            Timeout = 5000,
                            ModbusRegisters = new List<DataQuillDesktop.Models.ModbusRegisterConfig>
                            {
                                new() { TagName = "Temperature", StartAddress = 0, DataFormat = DataQuillDesktop.Models.ModbusDataFormat.SInt16, Scale = 0.1, Units = "°C", Description = "Ambient temperature" },
                                new() { TagName = "Pressure", StartAddress = 1, DataFormat = DataQuillDesktop.Models.ModbusDataFormat.UInt16, Scale = 1.0, Units = "PSI", Description = "System pressure" },
                                new() { TagName = "Flow_Rate", StartAddress = 2, DataFormat = DataQuillDesktop.Models.ModbusDataFormat.Float32, Scale = 1.0, Units = "GPM", Description = "Flow rate in gallons per minute" },
                                new() { TagName = "Motor_Speed", StartAddress = 4, DataFormat = DataQuillDesktop.Models.ModbusDataFormat.UInt32, Scale = 1.0, Units = "RPM", Description = "Motor rotation speed" },
                                new() { TagName = "Power_Factor", StartAddress = 6, DataFormat = DataQuillDesktop.Models.ModbusDataFormat.Float32Swap, Scale = 1.0, Units = "%", Description = "Power factor percentage" }
                            }
                        }
                    };

                    context.DataSources.Add(modbusSource);
                    context.SaveChanges();

                    Console.WriteLine("✅ Sample Modbus TCP configuration created");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to setup sample Modbus configurations: {ex.Message}");
            }
        }
    }
}
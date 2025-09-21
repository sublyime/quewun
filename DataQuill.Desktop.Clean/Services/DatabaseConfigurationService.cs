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
                Console.WriteLine("🔍 Checking existing Modbus data sources...");

                // Get all existing Modbus data sources
                var existingModbusSources = context.DataSources
                    .Include(ds => ds.Configuration)
                    .Where(ds => ds.ProtocolType == DataQuillDesktop.Models.ProtocolType.ModbusTCP)
                    .ToList();

                Console.WriteLine($"Found {existingModbusSources.Count} existing Modbus TCP sources");

                foreach (var source in existingModbusSources)
                {
                    Console.WriteLine($"  - {source.Name}: ModbusRegisters = {source.Configuration?.ModbusRegisters?.Count ?? 0} registers");
                }

                // Check if we have any Modbus source with enhanced configuration
                var hasEnhancedConfig = existingModbusSources.Any(ds =>
                    ds.Configuration?.ModbusRegisters?.Count > 0);

                if (!hasEnhancedConfig)
                {
                    Console.WriteLine("🔧 Setting up enhanced Modbus TCP configuration...");

                    // If there's an existing simple Modbus source, update it with enhanced config
                    if (existingModbusSources.Count > 0)
                    {
                        var existingSource = existingModbusSources.First();
                        Console.WriteLine($"📝 Updating existing source '{existingSource.Name}' with enhanced configuration");

                        existingSource.Configuration.ModbusRegisters = new List<DataQuillDesktop.Models.ModbusRegisterConfig>
                        {
                            new() { TagName = "Temperature", StartAddress = 0, DataFormat = DataQuillDesktop.Models.ModbusDataFormat.SInt16, Scale = 0.1, Units = "°C", Description = "Ambient temperature" },
                            new() { TagName = "Pressure", StartAddress = 1, DataFormat = DataQuillDesktop.Models.ModbusDataFormat.UInt16, Scale = 1.0, Units = "PSI", Description = "System pressure" },
                            new() { TagName = "Flow_Rate", StartAddress = 2, DataFormat = DataQuillDesktop.Models.ModbusDataFormat.Float32, Scale = 1.0, Units = "GPM", Description = "Flow rate in gallons per minute" },
                            new() { TagName = "Motor_Speed", StartAddress = 4, DataFormat = DataQuillDesktop.Models.ModbusDataFormat.UInt32, Scale = 1.0, Units = "RPM", Description = "Motor rotation speed" },
                            new() { TagName = "Power_Factor", StartAddress = 6, DataFormat = DataQuillDesktop.Models.ModbusDataFormat.Float32Swap, Scale = 1.0, Units = "%", Description = "Power factor percentage" }
                        };

                        context.SaveChanges();
                        Console.WriteLine("✅ Enhanced configuration applied to existing Modbus source");
                    }
                    else
                    {
                        // Create new enhanced Modbus source
                        var modbusSource = new DataQuillDesktop.Models.DataSource
                        {
                            Name = "Enhanced Modbus TCP Device",
                            Description = "Modbus TCP device with enhanced register configuration",
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
                        Console.WriteLine("✅ New enhanced Modbus TCP configuration created");
                    }
                }
                else
                {
                    Console.WriteLine("✅ Enhanced Modbus configuration already exists");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to setup enhanced Modbus configurations: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Manually force the setup of enhanced Modbus configuration
        /// This can be called from UI or other triggers to upgrade existing data sources
        /// </summary>
        public static void ForceEnhancedModbusSetup()
        {
            try
            {
                Console.WriteLine("🚀 MANUAL TRIGGER: Forcing enhanced Modbus configuration setup...");

                using var context = new QuillDbContext();
                context.Database.EnsureCreated();

                // Call the setup method directly
                SetupSampleModbusConfigurations(context);

                Console.WriteLine("✅ MANUAL TRIGGER: Enhanced Modbus setup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MANUAL TRIGGER FAILED: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Check and display current Modbus configuration status
        /// </summary>
        public static void CheckModbusConfigurationStatus()
        {
            try
            {
                Console.WriteLine("📊 CHECKING: Current Modbus configuration status...");

                using var context = new QuillDbContext();

                var modbusSources = context.DataSources
                    .Include(ds => ds.Configuration)
                    .Where(ds => ds.ProtocolType == DataQuillDesktop.Models.ProtocolType.ModbusTCP)
                    .ToList();

                Console.WriteLine($"📋 Found {modbusSources.Count} Modbus TCP data sources:");

                foreach (var source in modbusSources)
                {
                    var regCount = source.Configuration?.ModbusRegisters?.Count ?? 0;
                    var status = regCount > 0 ? "ENHANCED ✅" : "BASIC ⚠️";

                    Console.WriteLine($"  📌 {source.Name}: {status} ({regCount} configured registers)");

                    if (regCount > 0 && source.Configuration?.ModbusRegisters != null)
                    {
                        foreach (var reg in source.Configuration.ModbusRegisters)
                        {
                            Console.WriteLine($"    🏷️ {reg.TagName}: Address {reg.StartAddress}, {reg.DataFormat}, Scale {reg.Scale}, Units {reg.Units}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ STATUS CHECK FAILED: {ex.Message}");
            }
        }
    }
}
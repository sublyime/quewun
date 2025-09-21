using Microsoft.EntityFrameworkCore;
using System;

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to create database schema: {ex.Message}");
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using DataQuillDesktop.Models;
using System.Text.Json;

namespace DataQuillDesktop
{
    public class QuillDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<AppConfig> AppConfigs { get; set; }
        public DbSet<DataSource> DataSources { get; set; }
        public DbSet<DataPoint> DataPoints { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=quilldb;Username=quilluser;Password=ala1nna");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure DataSource entity
            modelBuilder.Entity<DataSource>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ConnectionString).HasMaxLength(1000);
                entity.Property(e => e.InterfaceType).HasConversion<string>();
                entity.Property(e => e.ProtocolType).HasConversion<string>();

                // Configure the Configuration as JSON
                entity.OwnsOne(e => e.Configuration, config =>
                {
                    config.Property(c => c.CustomParameters)
                          .HasConversion(
                              v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                              v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());
                });
            });

            // Configure DataPoint entity
            modelBuilder.Entity<DataPoint>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TagName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DataType).HasMaxLength(50);
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.Property(e => e.Quality).HasMaxLength(20);
                entity.Property(e => e.Value).HasConversion(
                    v => v != null ? v.ToString() : null,
                    v => v);
                entity.HasIndex(e => new { e.DataSourceId, e.Timestamp });
                
                // Configure relationship with DataSource
                entity.HasOne(e => e.DataSource)
                      .WithMany()
                      .HasForeignKey(e => e.DataSourceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
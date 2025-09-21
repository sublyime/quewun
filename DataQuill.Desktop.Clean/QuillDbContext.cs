using Microsoft.EntityFrameworkCore;
using DataQuillDesktop.Models;

namespace DataQuillDesktop
{
    public class QuillDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<AppConfig> AppConfigs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=quilldb;Username=quilluser;Password=ala1nna");
            }
        }
    }
}
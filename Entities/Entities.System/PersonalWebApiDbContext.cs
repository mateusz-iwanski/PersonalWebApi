using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PersonalWebApi.Entities.System
{
    public class PersonalWebApiDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public PersonalWebApiDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("SQLiteConnection");
                optionsBuilder.UseSqlite(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(r => r.Name).IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.Name).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.HasOne(u => u.Role)
                      .WithMany()
                      .HasForeignKey(u => u.RoleId);
            });
        }
    }
}

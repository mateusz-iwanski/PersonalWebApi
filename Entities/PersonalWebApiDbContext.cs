using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PersonalWebApi.Entities
{
    public class PersonalWebApiDbContext : DbContext
    {
        private readonly string _connectionString;

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public PersonalWebApiDbContext()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _connectionString = configuration.GetConnectionString("SQLiteConnection");
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }
}

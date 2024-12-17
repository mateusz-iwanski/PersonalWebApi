using Microsoft.AspNetCore.Identity;
using PersonalWebApi.Entities;
using PersonalWebApi.Exceptions;

namespace PersonalWebApi.Seeder
{
    public class UserSeeder
    {
        private readonly PersonalWebApiDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserSeeder(PersonalWebApiDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        /// <summary>
        /// Check if basic customer roles exist in the database and add them if they don't
        /// </summary>
        public void SeedBasic()
        {
            if (_context.Users.Count() == 0)
            {

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                string? name = configuration.GetSection("UserSettings:Administrator:Username").Value ??
                                throw new SettingsException("Can't read UserSettings:Administrator:Username settings from settings.json");

                string? email = configuration.GetSection("UserSettings:Administrator:Email").Value ??
                                throw new SettingsException("Can't read UserSettings:Administrator:Email settings from settings.json");

                string? passwordHash = configuration.GetSection("UserSettings:Administrator:PasswordHash").Value ??
                                throw new SettingsException("Can't read UserSettings:Administrator:PasswordHash settings from settings.json");

                var roleId = _context.Roles.FirstOrDefault(a => a.Name == "Administrator").Id; // Role seeder should be run before user seeder
                
                var user = new User
                {
                    Name = name,
                    Email = email,
                    RoleId = roleId
                };

                var newPasswordHash = _passwordHasher.HashPassword(user, passwordHash);
                user.PasswordHash = newPasswordHash;

                if (_context.Users.FirstOrDefault(a => a.Name == "Administrator") == null)
                    _context.Users.Add(user);

                _context.SaveChanges();
            }
        }
    }
}

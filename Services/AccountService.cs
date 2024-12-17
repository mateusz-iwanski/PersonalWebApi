using Microsoft.AspNetCore.Identity;
using PersonalWebApi.Entities;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models;

namespace PersonalWebApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly PersonalWebApiDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountService(PersonalWebApiDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.id == id);
            if (user == null)
                return false;
            
            _context.Users.Remove(user);
            _context.SaveChanges();

            return true;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return _context.Users.ToList();
        }

        public async Task RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            var newUser = new User
            {
                Email = registerUserDto.Email,
                Name = registerUserDto.Name,
                RoleId = registerUserDto.RoleId,
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, registerUserDto.Password);

            _context.Users.Add(newUser);
            _context.SaveChanges();
        }

        public async Task<bool> ChangeAdminPassword(String newPassword, String passwordVerification)
        {
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

            // get the password from the settings.json file
            string? passVerification = configuration.GetSection("UserSettings:Administrator:PasswordVerification").Value ??
                throw new SettingsException("Can't read UserSettings:Administrator:PasswordVerification settings from settings.json");

            if (passVerification != passwordVerification)
                return false;

            var admin = _context.Users.FirstOrDefault(u => u.Role.Name == "Administrator");

            admin.PasswordHash = _passwordHasher.HashPassword(admin, newPassword);

            return false;
        }
    }
}

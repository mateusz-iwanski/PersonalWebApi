using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalWebApi.Entities.System;
using PersonalWebApi.Exceptions;
using PersonalWebApi.Models.System;

namespace PersonalWebApi.Services.System
{
    /// <summary>
    /// Service for managing user accounts and roles.
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly PersonalWebApiDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="passwordHasher">The password hasher.</param>
        public AccountService(PersonalWebApiDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Adds a new role to the database.
        /// </summary>
        /// <param name="role">The role to be added.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddRolesAsync(RoleCreateDto role)
        {
            var newRole = new Role
            {
                Name = role.Name,
            };

            await _context.Roles.AddAsync(newRole);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves all roles from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of roles.</returns>
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        /// <summary>
        /// Deletes a role from the database.
        /// </summary>
        /// <param name="id">The ID of the role to be deleted.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        public async Task<bool> DeleteRoleAsync(int id)
        {
            // check if exists
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
                return false;

            // role can't be deleted if users have set this role
            var users = await _context.Users.FirstOrDefaultAsync(u => u.RoleId == id);
            if (users != null)
                return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="id">The ID of the user to be deleted.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.id == id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of users.</returns>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Registers a new user in the database.
        /// </summary>
        /// <param name="registerUserDto">The user details to be registered.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RegisterUserAsync(RegisterUserDto registerUserDto)
        {

            var newUser = new User
            {
                Email = registerUserDto.Email,
                Name = registerUserDto.Name,
                RoleId = registerUserDto.RoleId,
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, registerUserDto.Password);

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Changes the password of a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose password is to be changed.</param>
        /// <param name="newPassword">The new password to be set.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        public async Task<bool> ChangeUserPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.id == userId);
            if (user == null)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Changes the password of the administrator.
        /// </summary>
        /// <param name="newPassword">The new password to be set.</param>
        /// <param name="passwordVerification">The password verification string from the settings.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        public async Task<bool> ChangeAdminPasswordAsync(string newPassword, string passwordVerification)
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

            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role.Name == "Administrator");

            admin.PasswordHash = _passwordHasher.HashPassword(admin, newPassword);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}

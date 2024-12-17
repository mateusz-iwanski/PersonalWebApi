using PersonalWebApi.Entities.System;
using PersonalWebApi.Models.System;

namespace PersonalWebApi.Services.System
{
    public interface IAccountService
    {
        Task<bool> ChangeUserPasswordAsync(int userId, string newPassword);
        Task AddRolesAsync(RoleCreateDto role);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<bool> DeleteRoleAsync(int id);
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task RegisterUserAsync(RegisterUserDto registerUserDto);
        Task<bool> ChangeAdminPasswordAsync(string newPassword, string passwordVerification);
    }
}
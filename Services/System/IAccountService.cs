using PersonalWebApi.Entities.System;
using PersonalWebApi.Models.System;

namespace PersonalWebApi.Services.System
{
    public interface IAccountService
    {
        Task ChangeUserPasswordAsync(int userId, string newPassword);
        Task AddRolesAsync(RoleCreateDto role);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task DeleteRoleAsync(int id);
        Task DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task RegisterUserAsync(RegisterUserDto registerUserDto);
        Task ChangeAdminPasswordAsync(string newPassword, string passwordVerification);
        ResponseLoginDto GenerateJwt(LoginDto loginDto);
        Task ChangeAdminEmailAsync(string newEmail);
    }
}
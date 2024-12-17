using PersonalWebApi.Entities;
using PersonalWebApi.Models;

namespace PersonalWebApi.Services
{
    public interface IAccountService
    {
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task RegisterUserAsync(RegisterUserDto registerUserDto);
        Task<bool> ChangeAdminPassword(String newPassword, String passwordVerification);
    }
}
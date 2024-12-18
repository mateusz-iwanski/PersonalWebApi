using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalWebApi.Entities.System;
using PersonalWebApi.Models.System;
using PersonalWebApi.Services.System;
using System.ComponentModel.DataAnnotations;
using static PersonalWebApi.Controllers.System.ApiSystemAccountController;

namespace PersonalWebApi.Controllers.System
{
    /// <summary>
    /// Controller for managing accounts, roles, and users.
    /// </summary>
    [Route("api/system/account")]
    [ApiController]
    public class ApiSystemAccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public record AdminPasswordChange([MinLength(8)] string newPassword, [Required] string passwordVerification);
        public record UserPasswordChange([Required] int userId, [MinLength(8)] string newPassword);

        public ApiSystemAccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Deletes a role by ID.
        /// </summary>
        /// <param name="id">The ID of the role to delete.</param>
        /// <returns>An ActionResult indicating the result of the operation.</returns>
        [HttpDelete("delete-role/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> DeleteRoleAsync([FromRoute] int id)
        {
            await _accountService.DeleteRoleAsync(id);
            return Ok();
        }

        /// <summary>
        /// Adds a new role.
        /// </summary>
        /// <param name="role">The role details.</param>
        /// <returns>An ActionResult indicating the result of the operation.</returns>
        [HttpPost("add-role")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> AddRoleAsync([FromBody] RoleCreateDto role)
        {
            await _accountService.AddRolesAsync(role);
            return Ok();
        }

        /// <summary>
        /// Retrieves all roles.
        /// </summary>
        /// <returns>A list of roles.</returns>
        [HttpGet("roles")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Role>>> GetAllRolesAsync()
        {
            var roles = await _accountService.GetAllRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Deletes a user by ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>An ActionResult indicating the result of the operation.</returns>
        /// <remarks>
        /// You can't delete the Administrator user (ID = 1).
        /// </remarks>
        [HttpDelete("delete-user/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> DeleteUserAsync([FromRoute] int id)
        {
            await _accountService.DeleteUserAsync(id);
            return Ok();
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of users.</returns>
        [HttpGet("users")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersAsync()
        {
            var users = await _accountService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="registerUserDto">The user registration details.</param>
        /// <returns>An ActionResult indicating the result of the operation.</returns>
        /// <remarks>
        /// You can't add RoleId = 1 (Administrator) for user. Only admin user 
        /// </remarks>
        [HttpPost("register-user")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> RegisterUserAsync([FromBody] RegisterUserDto registerUserDto)
        {
            await _accountService.RegisterUserAsync(registerUserDto);
            return Ok();
        }

        /// <summary>
        /// Changes the password for a user.
        /// </summary>
        /// <param name="userPasswordChange">The user ID and new password details.</param>
        /// <returns>An ActionResult indicating the result of the operation.</returns>
        [HttpPost("change-user-password")]
        [Authorize]
        public async Task<ActionResult> ChangeUserPassword([FromBody] UserPasswordChange userPasswordChange)
        {
            await _accountService.ChangeUserPasswordAsync(userPasswordChange.userId, userPasswordChange.newPassword);
            return Ok();
        }

        /// <summary>
        /// Changes the password for the admin user.
        /// </summary>
        /// <param name="passwordChange">The new password and its verification.</param>
        /// <returns>An ActionResult indicating the result of the operation.</returns>
        [HttpPost("change-admin-passwords")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> ChangeAdminPassword([FromBody] AdminPasswordChange passwordChange)
        {
            await _accountService.ChangeAdminPasswordAsync(passwordChange.newPassword, passwordChange.passwordVerification);
            return Ok();
        }

        /// <summary>
        /// Login API user
        /// </summary>
        [HttpPost("login")]        
        public ActionResult Login([FromBody] LoginDto loginDto)
        {
            string token = _accountService.GenerateJwt(loginDto);
            return Ok(token);
        }
    }
}

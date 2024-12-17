using Microsoft.AspNetCore.Mvc;
using PersonalWebApi.Entities.System;
using PersonalWebApi.Models.System;
using PersonalWebApi.Services.System;
using System.ComponentModel.DataAnnotations;
using static PersonalWebApi.Controllers.System.AccountController;

namespace PersonalWebApi.Controllers.System
{
    /// <summary>
    /// Controller for managing accounts, roles, and users.
    /// </summary>
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public record AdminPasswordChange([MinLength(8)] string newPassword, [Required] string passwordVerification);
        public record UserPasswordChange([Required] int userId, [MinLength(8)] string newPassword);

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Deletes a role by ID.
        /// </summary>
        /// <param name="id">The ID of the role to delete.</param>
        /// <returns>An ActionResult indicating the result of the operation.</returns>
        [HttpDelete("delete-role/{id}")]
        public async Task<ActionResult> DeleteRoleAsync([FromRoute] int id)
        {
            var response = await _accountService.DeleteRoleAsync(id);
            if (!response)
                return BadRequest("The role was not found or there are users who have set this role.");
            return Ok();
        }

        /// <summary>
        /// Adds a new role.
        /// </summary>
        /// <param name="role">The role details.</param>
        /// <returns>An ActionResult indicating the result of the operation.</returns>
        [HttpPost("add-role")]
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
        public async Task<ActionResult> DeleteUserAsync([FromRoute] int id)
        {
            if (id == 1)
                return BadRequest("You can't delete the admin user");

            var response = await _accountService.DeleteUserAsync(id);
            if (!response)
                return BadRequest("User not found.");

            return Ok();
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of users.</returns>
        [HttpGet("users")]
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
        public async Task<ActionResult> RegisterUserAsync([FromBody] RegisterUserDto registerUserDto)
        {
            await _accountService.RegisterUserAsync(registerUserDto);

            return Ok();
        }

        // change user password
        [HttpPost("change-user-password")]
        public async Task<ActionResult> ChangeUserPassword([FromBody] UserPasswordChange userPasswordChange)
        {
            if (userPasswordChange.userId == 1)
                return BadRequest("You can't change the admin password, use change-admin-passwords.");

            var response = await _accountService.ChangeUserPasswordAsync(userPasswordChange.userId, userPasswordChange.newPassword);
            // if the password verification failed
            if (!response)
                return BadRequest("User not found.");
            return Ok();
        }

        /// <summary>
        /// Changes the password for the admin user.
        /// </summary>
        /// <param name="passwordChange">The new password and its verification.</param>
        /// <returns>An ActionResult indicating the result of the operation.</returns>
        [HttpPost("change-admin-passwords")]
        public async Task<ActionResult> ChangeAdminPassword([FromBody] AdminPasswordChange passwordChange)
        {
            var response = await _accountService.ChangeAdminPasswordAsync(passwordChange.newPassword, passwordChange.passwordVerification);

            // if the password verification failed
            if (!response)
                return BadRequest("Check password verification.");

            return Ok();
        }
    }
}

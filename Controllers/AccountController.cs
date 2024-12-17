using Microsoft.AspNetCore.Mvc;
using PersonalWebApi.Entities;
using PersonalWebApi.Models;
using PersonalWebApi.Services;
using static PersonalWebApi.Controllers.AccountController;

namespace PersonalWebApi.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public record PasswordChange(String newPassword, string passwordVerification);

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpDelete("delete-user/{id}")]
        public async Task<ActionResult> DeleteUserAsync([FromRoute] int id)
        {
            if (id == 1)
                return BadRequest("You can't delete Administrator user");

            var response = await _accountService.DeleteUserAsync(id);
            if (!response)
                return BadRequest("User not found");

            return Ok();
        }

        // get all users
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
        [HttpPost("register")]
        public async Task<ActionResult> RegisterUserAsync([FromBody] RegisterUserDto registerUserDto)
        {
            await _accountService.RegisterUserAsync(registerUserDto);

            return Ok();
        }

        [HttpPost("change-admin-passwords")]
        public async Task<ActionResult> ChangeAdminPassword([FromBody] PasswordChange passwordChange)
        {
            var response = await _accountService.ChangeAdminPassword(passwordChange.newPassword, passwordChange.passwordVerification);

            // if the password verification failed
            if (!response)
                return BadRequest("Check password verification");

            return Ok();
        }
    }
}

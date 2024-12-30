
using FluentValidation;
using PersonalWebApi.Entities.System;
using PersonalWebApi.Models.System;

namespace PersonalWebApi.Validations.System
{
    /// <summary>
    /// Validator for RegisterUserDto.
    /// </summary>
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        private readonly PersonalWebApiDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterUserDtoValidator"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public RegisterUserDtoValidator(PersonalWebApiDbContext context)
        {
            _context = context;

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .Must(email => !_context.Users.Any(u => u.Email.Trim() == email.Trim()))
                    .WithMessage("Email already exists.");
            RuleFor(x => x.Name)
                .NotEmpty()
                .Must(name => !_context.Users.Any(u => u.Name.Trim() == name.Trim()))
                    .WithMessage("Name already exists.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.RoleId)
                .NotEmpty()
                .Must(roleId => roleId != 1)
                    .WithMessage("RoleId cannot be 1 because it is an administrator role.")
                .Must((roleId) =>
                {
                    return _context.Roles.Any(c => c.Id == roleId);
                })
                .WithMessage("The user role does not exist.");
        }
    }

}

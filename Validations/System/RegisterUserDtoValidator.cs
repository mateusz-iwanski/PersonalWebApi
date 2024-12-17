
using FluentValidation;
using PersonalWebApi.Entities.System;
using PersonalWebApi.Models.System;

namespace PersonalWebApi.Validations.System
{
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        private readonly PersonalWebApiDbContext _context;

        public RegisterUserDtoValidator(PersonalWebApiDbContext context)
        {
            _context = context;

            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Name).NotEmpty();
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

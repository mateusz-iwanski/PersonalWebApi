
using FluentValidation;
using PersonalWebApi.Models;

namespace PersonalWebApi.Validations
{
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.RoleId)
                .NotEmpty()
                .Must(roleId => roleId != 1)
                    .WithMessage("Role ID cannot be 1 because it is an administrator role.");
        }
    }

}

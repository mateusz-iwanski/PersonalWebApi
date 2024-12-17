using FluentValidation;
using PersonalWebApi.Entities.System;
using PersonalWebApi.Models.System;

namespace PersonalWebApi.Validations.System
{
    public class RoleCreateDtoValidator : AbstractValidator<RoleCreateDto>
    {
        private readonly PersonalWebApiDbContext _context;

        public RoleCreateDtoValidator(PersonalWebApiDbContext context)
        {
            _context = context;

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(20)
                .Must((name) =>
                {
                    return !_context.Roles.Any(c => c.Name.Trim() == name.Trim());
                })
                .WithMessage("The role already exists.");
        }
    }
}

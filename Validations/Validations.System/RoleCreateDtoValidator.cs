using FluentValidation;
using PersonalWebApi.Entities.System;
using PersonalWebApi.Models.System;

namespace PersonalWebApi.Validations.System
{
    /// <summary>
    /// Validator for RoleCreateDto.
    /// </summary>
    public class RoleCreateDtoValidator : AbstractValidator<RoleCreateDto>
    {
        private readonly PersonalWebApiDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleCreateDtoValidator"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
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

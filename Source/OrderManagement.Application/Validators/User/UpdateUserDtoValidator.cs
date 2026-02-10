using FluentValidation;
using OrderManagement.Communication.Dtos.User;

namespace OrderManagement.Application.Validators.User;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Password)
            .Must(p => !string.IsNullOrWhiteSpace(p))
            .WithMessage("Password cannot be empty.")
            .MaximumLength(100)
            .When(x => x.Password is not null);

        RuleFor(x => x.Role)
           .NotEmpty().WithMessage("Role cannot be empty.")
           .When(x => x.Role != null);

    }
}

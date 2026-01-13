using FluentValidation;
using OrderManagement.Communication.Dtos.Order;

namespace OrderManagement.Application.Validators.Order;


public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
{
    public UpdateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId cannot be empty.")
            .When(x => x.CustomerId.HasValue);

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.Status));
    }

}

using FluentValidation;
using OrderManagement.Communication.Dtos.Product;

namespace OrderManagement.Application.Validators.Product;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
            .When(x => x.StockQuantity.HasValue);

    }
}

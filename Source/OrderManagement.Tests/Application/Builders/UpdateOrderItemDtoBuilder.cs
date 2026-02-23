using Bogus;
using OrderManagement.Communication.Dtos.OrderItem;

namespace OrderManagement.Tests.Application.Builders;

public class UpdateOrderItemDtoBuilder
{
    private Guid? _productId;
    private int? _quantity;
    private decimal? _unitPrice;
    private readonly Faker _faker;

    public UpdateOrderItemDtoBuilder()
    {
        _faker = new Faker("pt_BR");
        _productId = Guid.NewGuid();
        _quantity = _faker.Random.Int(1, 10);
        _unitPrice = _faker.Random.Decimal(1, 500);
    }

    public UpdateOrderItemDtoBuilder WithProductId(Guid? productId)
    {
        _productId = productId;
        return this;
    }

    public UpdateOrderItemDtoBuilder WithoutProductId()
    {
        _productId = null;
        return this;
    }

    public UpdateOrderItemDtoBuilder WithQuantity(int? quantity)
    {
        _quantity = quantity;
        return this;
    }

    public UpdateOrderItemDtoBuilder WithUnitPrice(decimal? unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    public UpdateOrderItemDto Build()
    {
        return new UpdateOrderItemDto
        {
            ProductId = _productId,
            Quantity = _quantity,
            UnitPrice = _unitPrice
        };
    }
}

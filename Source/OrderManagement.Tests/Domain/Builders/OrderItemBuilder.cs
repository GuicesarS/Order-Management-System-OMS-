using Bogus;
using OrderManagement.Domain.Entities.OrderItem;
using OrderManagement.Domain.Exception;
using System;

public class OrderItemBuilder
{
	
    private readonly Faker _faker;

    private Guid _orderId;
    private Guid _productId;
    private int _quantity;
    private decimal _unitPrice;

    public OrderItemBuilder()
    {
        _faker = new Faker("pt_BR");

        _orderId = Guid.NewGuid();
        _productId = Guid.NewGuid();
        _quantity = _faker.PickRandom(1, 100);
        _unitPrice = _faker.PickRandom(1, 100);
    }

    public OrderItemBuilder WithOrderId(Guid orderId)
    {
        _orderId = orderId;
        return this;
    }

    public OrderItemBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public OrderItemBuilder WithQuantity(int quantity)
    {
        if (quantity < 1)
            throw new DomainValidationException("Quantity must be at least 1.");

        _quantity = quantity;
        return this;
    }

    public OrderItemBuilder WithUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 1)
            throw new DomainValidationException("UnitPrice must be greater than 0.");

        _unitPrice = unitPrice;
        return this;
    }

    public OrderItem Build()
        => new OrderItem(_orderId, _productId, _quantity, _unitPrice);

}

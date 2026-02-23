using Bogus;
using OrderManagement.Communication.Dtos.OrderItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Tests.Application.Builders;

public class CreateOrderItemDtoBuilder
{
    private Guid _productId;
    private int _quantity;
    private decimal _unitPrice;
    private readonly Faker _faker;

    public CreateOrderItemDtoBuilder()
    {
        _faker = new Faker("pt_BR");
        _productId = Guid.NewGuid();
        _quantity = _faker.Random.Int(1, 10);
        _unitPrice = _faker.Random.Decimal(1, 500);
    }

    public CreateOrderItemDtoBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public CreateOrderItemDtoBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public CreateOrderItemDtoBuilder WithUnitPrice(decimal unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    public CreateOrderItemDto Build()
    {
        return new CreateOrderItemDto
        {
            ProductId = _productId,
            Quantity = _quantity,
            UnitPrice = _unitPrice
        };
    }
}
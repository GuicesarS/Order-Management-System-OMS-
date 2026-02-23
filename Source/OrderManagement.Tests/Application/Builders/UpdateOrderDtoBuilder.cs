using Bogus;
using OrderManagement.Communication.Dtos.Order;
using OrderManagement.Communication.Dtos.OrderItem;

namespace OrderManagement.Tests.Application.Builders;

public class UpdateOrderDtoBuilder
{
    private Guid? _customerId;
    private string? _status;
    private List<UpdateOrderItemDto>? _items;
    private readonly Faker _faker;

    public UpdateOrderDtoBuilder()
    {
        _faker = new Faker("pt_BR");
        _customerId = Guid.NewGuid();
        _status = null;
        _items = null;
    }

    public UpdateOrderDtoBuilder WithCustomerId(Guid? customerId)
    {
        _customerId = customerId;
        return this;
    }

    public UpdateOrderDtoBuilder WithoutCustomerId()
    {
        _customerId = null;
        return this;
    }

    public UpdateOrderDtoBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public UpdateOrderDtoBuilder WithItems(List<UpdateOrderItemDto> items)
    {
        _items = items;
        return this;
    }

    public UpdateOrderDto Build()
    {
        return new UpdateOrderDto
        {
            CustomerId = _customerId,
            Status = _status,
            Items = _items
        };
    }
}

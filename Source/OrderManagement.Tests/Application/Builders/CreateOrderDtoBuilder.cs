using Bogus;
using OrderManagement.Communication.Dtos.Order;
using OrderManagement.Communication.Dtos.OrderItem;

namespace OrderManagement.Tests.Application.Builders;

public class CreateOrderDtoBuilder
{
    private Guid _customerId;
    private List<CreateOrderItemDto> _items;
    private readonly Faker _faker;

    public CreateOrderDtoBuilder()
    {
        _faker = new Faker("pt_BR");
        _customerId = Guid.NewGuid();
        _items = new List<CreateOrderItemDto>
        {
            new CreateOrderItemDtoBuilder().Build()
        };
    }

    public CreateOrderDtoBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public CreateOrderDtoBuilder WithItems(List<CreateOrderItemDto> items)
    {
        _items = items;
        return this;
    }

    public CreateOrderDtoBuilder WithEmptyItems()
    {
        _items = new List<CreateOrderItemDto>();
        return this;
    }

    public CreateOrderDto Build()
    {
        return new CreateOrderDto
        {
            CustomerId = _customerId,
            Items = _items
        };
    }
}

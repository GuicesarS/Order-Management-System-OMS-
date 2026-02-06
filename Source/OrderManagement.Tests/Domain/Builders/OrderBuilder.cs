using Bogus;
using OrderManagement.Domain.Entities.Order;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Tests.Domain.Builders;

public class OrderBuilder
{
    private readonly Faker _faker;

    private Guid _customerId;
    private int _quantity;
    private decimal _unitPrice;

    public OrderBuilder()
    {
        _faker = new Faker("pt_BR");
        _customerId = Guid.NewGuid();
        _quantity = _faker.PickRandom(1, 100);
        _unitPrice = _faker.PickRandom<decimal>(1, 100);
    }

    public Order Build() => new Order(_customerId);

    public Order BuildWithEmptyCustomerId() => new Order(Guid.Empty);

    public Order BuildPaidOrder()
    {
        var order = Build();

        order.AddItem(Guid.NewGuid(), _quantity, _unitPrice);

        order.MarkAsPaid();

        return order;
    }

    public Order BuildShippedOrder()
    {
        var order = Build();

        order.AddItem(Guid.NewGuid(), _quantity, _unitPrice);

        order.MarkAsPaid();

        order.MarkAsShipped();

        return order;
    }

    public Order BuildCancelledOrder()
    {
        var order = Build();

        order.MarkAsCancelled();

        return order;

    }

}



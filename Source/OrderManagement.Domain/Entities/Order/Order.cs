using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities.Order;

public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private readonly List<OrderItem.OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem.OrderItem> Items => _items.AsReadOnly();

    protected Order() { } // Ef Core

    public Order(Guid customerId)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;

    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity < 1)
            throw new ArgumentException("Quantity must be at least 1.");
        if (unitPrice < 0)
            throw new ArgumentException("UnitPrice must be greater or equal to 0.");

        var item = new OrderItem.OrderItem(Id, productId, quantity, unitPrice);

        _items.Add(item);

        TotalAmount = _items.Sum(i => i.LineTotal);

    }

    public void MarkAsPaid()
    {
        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;
    }
}

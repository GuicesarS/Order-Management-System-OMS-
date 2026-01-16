using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities.Order;

public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private readonly List<OrderItem.OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem.OrderItem> Items => _items.AsReadOnly();

    protected Order() { } // EF Core

    public Order(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.");

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

        RecalculateTotal();
    }

    public void UpdateItem(Guid productId, int quantity, decimal unitPrice)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            throw new ArgumentException("Item not found in order.");

        item.UpdateQuantity(quantity);
        item.UpdateUnitPrice(unitPrice);

        RecalculateTotal();
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            throw new ArgumentException("Item not found in order.");

        _items.Remove(item);

        RecalculateTotal();
    }

    public void MarkAsPaid()
    {
        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;
    }
    public void MarkAsCancelled() => Status = OrderStatus.Cancelled;
    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException("Only paid orders can be shipped.");
        
        Status = OrderStatus.Shipped;
    }
   
    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.LineTotal);
    }
}
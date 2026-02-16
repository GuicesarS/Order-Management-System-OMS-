using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exception;

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
            throw new DomainValidationException("CustomerId is required.");

        Id = Guid.NewGuid();
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        ValidateOrderIsPending();

        if (quantity < 1)
            throw new DomainValidationException("Quantity must be at least 1.");
        if (unitPrice <= 0)
            throw new DomainValidationException("UnitPrice must be greater than 0.");

        var item = new OrderItem.OrderItem(Id, productId, quantity, unitPrice);
        _items.Add(item);
        RecalculateTotal();
    }

    public void UpdateItem(Guid productId, int quantity, decimal unitPrice)
    {
        ValidateOrderIsPending();

        if (quantity < 1)
            throw new DomainValidationException("Quantity must be at least 1.");
        if (unitPrice <= 0)
            throw new DomainValidationException("UnitPrice must be greater than 0.");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            throw new DomainValidationException("Item not found in order.");

        item.UpdateQuantity(quantity);
        item.UpdateUnitPrice(unitPrice);
        RecalculateTotal();
    }

    public void RemoveItem(Guid productId)
    {
        ValidateOrderIsPending();

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            throw new DomainValidationException("Item not found in order.");

        _items.Remove(item);
        RecalculateTotal();
    }

    public void MarkAsPaid()
    {
        if (Status == OrderStatus.Cancelled)
            throw new DomainValidationException("Cannot pay a cancelled order.");
        if (Status == OrderStatus.Shipped)
            throw new DomainValidationException("Cannot pay a shipped order.");
        if (Status == OrderStatus.Paid)
            throw new DomainValidationException("Order is already paid.");

        if (_items.Count == 0)
            throw new DomainValidationException("Cannot pay an order without items.");

        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkAsCancelled()
    {
        if (Status == OrderStatus.Shipped)
            throw new DomainValidationException("Cannot cancel a shipped order.");
        if (Status == OrderStatus.Cancelled)
            throw new DomainValidationException("Order is already cancelled.");

        Status = OrderStatus.Cancelled;
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Paid)
            throw new DomainValidationException("Only paid orders can be shipped.");

        if (_items.Count == 0)
            throw new DomainValidationException("Cannot ship an order without items.");

        Status = OrderStatus.Shipped;
    }

    private void ValidateOrderIsPending()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainValidationException("Cannot modify items of a non-pending order.");
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.LineTotal);
    }
}
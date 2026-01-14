namespace OrderManagement.Domain.Entities.OrderItem;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    protected OrderItem() { } // Ef Core

    public OrderItem(Guid orderId, Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity < 1)
            throw new ArgumentException("Quantity must be at least 1.");
        if (unitPrice < 0)
            throw new ArgumentException("UnitPrice must be greater or equal to 0.");

        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = quantity * unitPrice;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity < 1)
            throw new ArgumentException("Quantity must be at least 1.");
        Quantity = quantity;
    }

    public void UpdateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new ArgumentException("UnitPrice must be greater or equal to 0.");
        UnitPrice = unitPrice;
    }


}

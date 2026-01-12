namespace OrderManagement.Communication.Dtos.OrderItem;

public class UpdateOrderItemDto
{
    public Guid? ProductId { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}

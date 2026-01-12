using OrderManagement.Communication.Dtos.OrderItem;

namespace OrderManagement.Communication.Dtos.Order;

public class UpdateOrderDto
{
    public Guid? CustomerId { get; set; }
    public string? Status { get; set; }
    public List<UpdateOrderItemDto>? Items { get; set; }
}

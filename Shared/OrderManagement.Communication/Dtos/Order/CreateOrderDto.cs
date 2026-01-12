using OrderManagement.Communication.Dtos.OrderItem;

namespace OrderManagement.Communication.Dtos.Order;

public class CreateOrderDto
{
    public Guid CustomerId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = [];
}

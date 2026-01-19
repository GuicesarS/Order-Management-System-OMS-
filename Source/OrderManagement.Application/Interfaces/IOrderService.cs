using OrderManagement.Application.Common;
using OrderManagement.Communication.Dtos.Order;
using OrderManagement.Communication.Responses;

namespace OrderManagement.Application.Interfaces;

public interface IOrderService
{
    Task<Result<OrderResponse>> Create(CreateOrderDto orderDto);
    Task<Result<OrderResponse>> Update(Guid id, UpdateOrderDto updateOrderDto);
    Task<Result<OrderResponse>> GetOrderById(Guid id);
    Task<Result<IEnumerable<OrderResponse>>> GetAll();
    Task<Result<bool>> Delete(Guid id);
}

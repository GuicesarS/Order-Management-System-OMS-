using OrderManagement.Application.Common;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Order;

namespace OrderManagement.Application.Interfaces;

public interface IOrderService
{
    Task<Result<OrderResponse>> Create(Order order);
    Task<Result<OrderResponse>> Update(Guid id, Order order);
    Task<Result<OrderResponse>> GetOrderById(Guid id);
    Task<Result<IEnumerable<OrderResponse>>> GetAll();
    Task<Result<bool>> Delete(Guid id);
}

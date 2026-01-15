using OrderManagement.Application.Common;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Order;

namespace OrderManagement.Application.Interfaces;

public interface IOrderService
{
    Result<OrderResponse> Create(Order order);
    Result<OrderResponse> Update(Guid id, Order order);
    Result<OrderResponse> GetOrderById(Guid id);
    Result<IEnumerable<OrderResponse>> GetAll();
    Result<bool> Delete(Guid id);
}

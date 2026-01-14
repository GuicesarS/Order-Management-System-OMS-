using OrderManagement.Application.Common;
using OrderManagement.Communication.Dtos.Order;
using OrderManagement.Communication.Responses;

namespace OrderManagement.Application.Interfaces;

public interface IOrderService
{
    Result<OrderResponse> Create(CreateOrderDto createOrderDto);
    Result<OrderResponse> Update(Guid id, UpdateOrderDto updateOrderDto);
    Result<OrderResponse> GetById(Guid id);
    Result<IEnumerable<OrderResponse>> GetAll();
    Result<bool> Delete(Guid id);
}

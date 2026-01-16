using OrderManagement.Application.Common;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Interfaces;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Order;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ICustomMapper _mapper;

    public OrderService(IOrderRepository repository, ICustomMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<OrderResponse>> Create(Order order)
    {
       await _repository.AddAsync(order);

       var orderResponse = _mapper.Map<Order, OrderResponse>(order);
       return Result<OrderResponse>.Ok(orderResponse);

    }
    public async Task<Result<OrderResponse>> Update(Guid id, Order order)
    {
       var existingOrder = await _repository.GetOrderByIdAsync(id);
      
        if(existingOrder is null)
            return Result<OrderResponse>.Failure("Order not found.");

        if (order.Status == OrderStatus.Paid)
          existingOrder.MarkAsPaid();
        else if (order.Status == OrderStatus.Shipped)
          existingOrder.MarkAsShipped();
        else if (order.Status == OrderStatus.Cancelled)
            existingOrder.MarkAsCancelled();

        foreach (var item in order.Items)
        {
            var existingItem = existingOrder
                .Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem is not null)
                existingOrder.UpdateItem(item.ProductId, item.Quantity, item.UnitPrice);
            else
                existingOrder.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }

        await _repository.UpdateAsync(existingOrder);
        var result = _mapper.Map<Order, OrderResponse>(existingOrder);

        return Result<OrderResponse>.Ok(result);
    }
    public async Task<Result<bool>> Delete(Guid id)
    {
        var existingOrder = await _repository.GetOrderByIdAsync(id);

        if (existingOrder is null)
            return Result<bool>.Failure("Order not found.");

        await _repository.DeleteAsync(id);
        return Result<bool>.Ok(true);

    }

    public async Task<Result<IEnumerable<OrderResponse>>> GetAll()
    {
        var orders = await _repository.GetAllAsync();
        var ordersResponse = _mapper.Map<IEnumerable<Order>, IEnumerable<OrderResponse>>(orders);

        return Result<IEnumerable<OrderResponse>>.Ok(ordersResponse);
    }

    public async Task<Result<OrderResponse>> GetOrderById(Guid id)
    {
        var order = await _repository.GetOrderByIdAsync(id);

         if (order is null)
              return Result<OrderResponse>.Failure("Order not found.");

         var orderResponse = _mapper.Map<Order, OrderResponse>(order);
         return Result<OrderResponse>.Ok(orderResponse);
    }
}

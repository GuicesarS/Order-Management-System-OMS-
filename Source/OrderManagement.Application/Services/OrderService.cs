using OrderManagement.Application.Common;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Validators.Order;
using OrderManagement.Communication.Dtos.Order;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Order;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }
    public Result<OrderResponse> Create(CreateOrderDto createOrderDto)
    {
        var validator = new CreateOrderDtoValidator();
        var validationResult = validator.Validate(createOrderDto);

        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors
                .Select(error => error.ErrorMessage)
                .ToList();

            var combinedErrorMessage = string.Join("; ", errorMessages);

            return Result<OrderResponse>.Failure(combinedErrorMessage);

        }

        var order = new Order(createOrderDto.CustomerId);

        foreach (var dtoItems in createOrderDto.Items)
        {
            order.AddItem(dtoItems.ProductId, dtoItems.Quantity, dtoItems.UnitPrice);
        }

        _repository.Add(order);

        return Result<OrderResponse>.Ok(MapToOrderResponse(order));


    }
    public Result<OrderResponse> Update(Guid id, UpdateOrderDto updateOrderDto)
    {
        var validator = new UpdateOrderDtoValidator();
        var validationResult = validator.Validate(updateOrderDto);

        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors
                .Select(error => error.ErrorMessage)
                .ToList();

            var combinedErrorMessage = string.Join("; ", errorMessages);

            return Result<OrderResponse>.Failure(combinedErrorMessage);

        }

        var order = _repository.GetOrderById(id);

        if (order is null)
           return Result<OrderResponse>.Failure("Order not found.");

        if (!string.IsNullOrEmpty(updateOrderDto.Status))
        {
            if (updateOrderDto.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                order.MarkAsPaid();
            else if (updateOrderDto.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                order.MarkAsCancelled(); 
        }

        // Atualizar status
        if (!string.IsNullOrEmpty(updateOrderDto.Status))
        {
            if (updateOrderDto.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                order.MarkAsPaid();
            else if (updateOrderDto.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                order.MarkAsCancelled();
        }

        // Atualizar itens
        if (updateOrderDto.Items is not null)
        {
            foreach (var itemDto in updateOrderDto.Items)
            {
                var existingItem = order.Items.FirstOrDefault(i => i.ProductId == itemDto.ProductId);
                if (existingItem is not null)
                {
                    order.UpdateItem(itemDto.ProductId!.Value, itemDto.Quantity!.Value, itemDto.UnitPrice!.Value);
                }
                else if (itemDto.ProductId != Guid.Empty && itemDto.Quantity.HasValue && itemDto.UnitPrice.HasValue)
                {
                    order.AddItem(itemDto.ProductId!.Value, itemDto.Quantity.Value, itemDto.UnitPrice.Value);
                }
            }
        }

        _repository.Update(order);

        return Result<OrderResponse>.Ok(MapToOrderResponse(order));
    }

    public Result<OrderResponse> GetById(Guid id)
    {
        var order = _repository.GetOrderById(id);

        if (order is null)
            return Result<OrderResponse>.Failure("Order not found.");

        return Result<OrderResponse>.Ok(MapToOrderResponse(order));
    }

    public Result<IEnumerable<OrderResponse>> GetAll()
    {
       var orders = _repository.GetAll();
       var orderResponses = orders.Select(MapToOrderResponse);

       return Result<IEnumerable<OrderResponse>>.Ok(orderResponses);
    }

    public Result<bool> Delete(Guid id)
    {
        var order = _repository.GetOrderById(id);

        if (order is null)
            return Result<bool>.Failure("Order not found.");

        _repository.Delete(id);

        return Result<bool>.Ok(true);
    }

    public static OrderResponse MapToOrderResponse(Order order) =>
        new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            Items = order.Items.Select(item => new OrderItemResponse
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
}

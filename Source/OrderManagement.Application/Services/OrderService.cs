using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Exceptions;
using OrderManagement.Application.Interfaces;
using OrderManagement.Communication.Dtos.Order;
using OrderManagement.Communication.Dtos.OrderItem;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Order;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exception;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        ICustomMapper mapper,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderResponse>> Create(CreateOrderDto orderDto)
    {
        _logger.LogInformation("Creating a new order for customer {CustomerId}", orderDto.CustomerId);

        var customer = await _customerRepository.GetCustomerById(orderDto.CustomerId);

        if (customer is null)
        {
            _logger.LogWarning("Customer with id {CustomerId} does not exist.", orderDto.CustomerId);
            throw new ValidationException($"Customer with id {orderDto.CustomerId} does not exist.");
        }

        foreach (var item in orderDto.Items)
        {
            _logger.LogInformation("Validating product {ProductId} for order.", item.ProductId);

            var product = await _productRepository.GetProductById(item.ProductId);

            if (product is null)
            {
                _logger.LogWarning("Product with id {ProductId} does not exist.", item.ProductId);
                throw new ValidationException($"Product with id {item.ProductId} does not exist.");
            }
        }

        var order = new Order(orderDto.CustomerId);

        foreach(var item in orderDto.Items)
        {
            _logger.LogInformation("Adding product {ProductId} to order.", item.ProductId);
            order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }

        await _repository.AddAsync(order);

        _logger.LogInformation("Order {OrderId} created successfully.", order.Id);

        var orderResponse = _mapper.Map<Order, OrderResponse>(order);
        return Result<OrderResponse>.Ok(orderResponse);

    }
    public async Task<Result<OrderResponse>> Update(Guid id, UpdateOrderDto updateOrderDto)
    {
        _logger.LogInformation("Updating order {OrderId}", id);

        var customer = ValidateCustomerId(updateOrderDto);

        await ValidateCustomerExists(customer);

        await ValidateProducts(updateOrderDto.Items);

        var existingOrder = await GetExistingOrder(id);

        UpdateStatusIfProvided(updateOrderDto, existingOrder);

        UpdateOrderItems(updateOrderDto, existingOrder);

        await _repository.UpdateAsync(existingOrder);

        _logger.LogInformation("Order {OrderId} updated successfully.", id);

        var result = _mapper.Map<Order, OrderResponse>(existingOrder);

        return Result<OrderResponse>.Ok(result);

    }
    public async Task<Result<bool>> Delete(Guid id)
    {
        _logger.LogInformation("Deleting order {OrderId}", id);

        var existingOrder = await _repository.GetOrderByIdAsync(id);

        if (existingOrder is null)
        {
            _logger.LogWarning("Order with id {OrderId} was not found.", id);
            throw new ValidationException($"Order with id: {id} was not found.");
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation("Order {OrderId} deleted successfully.", id);

        return Result<bool>.Ok(true);

    }
    public async Task<Result<IEnumerable<OrderResponse>>> GetAll()
    {
        _logger.LogInformation("Retrieving all orders.");

        var orders = await _repository.GetAllAsync();
        var ordersResponse = _mapper.Map<IEnumerable<Order>, IEnumerable<OrderResponse>>(orders);

        _logger.LogInformation("Retrieved {OrderCount} orders.", ordersResponse.Count());

        return Result<IEnumerable<OrderResponse>>.Ok(ordersResponse);
    }
    public async Task<Result<OrderResponse>> GetOrderById(Guid id)
    {

        _logger.LogInformation("Retrieving order {OrderId}", id);

        var order = await _repository.GetOrderByIdAsync(id);

        if (order is null)
        {
            _logger.LogWarning("Order with id {OrderId} was not found.", id);
            throw new ValidationException($"Order with id: {id} was not found.");
        }

        var orderResponse = _mapper.Map<Order, OrderResponse>(order);

        _logger.LogInformation("Order {OrderId} retrieved successfully.", id);

        return Result<OrderResponse>.Ok(orderResponse);
    }

    private static void ApplyStatus(Order order, OrderStatus status)
    {
        switch (status)
        {
            case OrderStatus.Pending:
                throw new DomainValidationException("Cannot change order back to Pending status.");
            case OrderStatus.Paid:
                order.MarkAsPaid();
                break;
            case OrderStatus.Shipped:
                order.MarkAsShipped();
                break;
            case OrderStatus.Cancelled:
                order.MarkAsCancelled();
                break;
            default:
                throw new DomainValidationException("Invalid order status.");
        }
    }
    private Guid ValidateCustomerId(UpdateOrderDto updateOrderDto)
    {
        if (!updateOrderDto.CustomerId.HasValue)
        {
            _logger.LogWarning("CustomerId is required to update an order.");
            throw new ValidationException("CustomerId is required to update an order.");
        }
        return updateOrderDto.CustomerId.Value;
    }
    private async Task ValidateCustomerExists (Guid customerId)
    {
        var customer = await _customerRepository.GetCustomerById(customerId);

        if (customer is null)
        {
            _logger.LogWarning("Customer with id {CustomerId} does not exist.", customerId);
            throw new ValidationException($"Customer with id {customerId} does not exist.");
        }
    }
    private async Task ValidateProducts(IEnumerable<UpdateOrderItemDto> itemsDto)
    {
        if (itemsDto is null)
        {
            _logger.LogInformation("No order items to validate.");
            return;
        }

        foreach (var item in itemsDto)
        {
            _logger.LogInformation("Validating product {ProductId} for order update.", item.ProductId);

            if (!item.ProductId.HasValue)
            {
                _logger.LogWarning("ProductId is required to update an order item.");
                throw new ValidationException("ProductId is required to update an order item.");
            }

            var product = await _productRepository.GetProductById(item.ProductId.Value);

            if (product is null)
            {
                _logger.LogWarning("Product with id {ProductId} does not exist.", item.ProductId);
                throw new ValidationException($"Product with id {item.ProductId.Value} does not exist.");
            }
        }
    }
    private async Task<Order> GetExistingOrder(Guid id)
    {
        var existingOrder = await _repository.GetOrderByIdAsync(id);

        if (existingOrder is null)
        {
            _logger.LogWarning("Order with id {OrderId} was not found.", id);
            throw new ValidationException($"Order with id: {id} was not found.");
        }

        return existingOrder;
    }
    private void UpdateStatusIfProvided(UpdateOrderDto dto, Order order)
    {
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!Enum.TryParse<OrderStatus>(
                    dto.Status,
                    ignoreCase: true,
                    out var status))
            {
                _logger.LogWarning("Invalid order status: {Status}", dto.Status);
                throw new ValidationException("Invalid order status");
            }

            ApplyStatus(order, status);
        }
    }
    private void UpdateOrderItems(UpdateOrderDto dto, Order order)
    {
        if (dto.Items is null)
            return;

        foreach (var item in dto.Items)
        {
            if (!item.ProductId.HasValue)
                throw new ValidationException("ProductId is required.");

            var existingItem = order.Items
                .FirstOrDefault(i => i.ProductId == item.ProductId);

            if (existingItem is null)
                throw new ValidationException($"Product {item.ProductId} not found in order.");

            var unitPrice = item.UnitPrice ?? existingItem.UnitPrice;
            var quantity = item.Quantity ?? existingItem.Quantity;

            if (item.Quantity.HasValue)
                existingItem.UpdateQuantity(item.Quantity.Value);

            if (item.UnitPrice.HasValue)
                existingItem.UpdateUnitPrice(item.UnitPrice.Value);
        }
    }
}

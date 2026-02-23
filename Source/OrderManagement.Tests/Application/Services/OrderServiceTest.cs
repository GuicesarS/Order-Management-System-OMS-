using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Exceptions;
using OrderManagement.Application.Services;
using OrderManagement.Communication.Dtos.OrderItem;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Customer;
using OrderManagement.Domain.Entities.Order;
using OrderManagement.Domain.Entities.Product;
using OrderManagement.Domain.Exception;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Tests.Application.Builders;
using OrderManagement.Tests.Application.ExtensionMethods;
using OrderManagement.Tests.Domain.Builders;

namespace OrderManagement.Tests.Application.Services;

public class OrderServiceTest
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICustomMapper> _mapperMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;

    private readonly OrderService _orderService;

    public OrderServiceTest()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<ICustomMapper>();
        _loggerMock = new Mock<ILogger<OrderService>>();

        _orderService = new OrderService(
            _orderRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateOrder_ShouldBeSuccessful()
    {
        var customer = new CustomerBuilder().Build();
        var productId = Guid.NewGuid();
        var product = new ProductBuilder().Build();

        var itemDto = new CreateOrderItemDtoBuilder()
            .WithProductId(productId)
            .Build();

        var orderDto = new CreateOrderDtoBuilder()
            .WithCustomerId(customer.Id)
            .WithItems(new List<CreateOrderItemDto> { itemDto })
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(customer.Id))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(r => r.GetProductById(productId))
            .ReturnsAsync(product);

        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<Order, OrderResponse>(It.IsAny<Order>()))
            .Returns((Order order) => new OrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt
            });

        var result = await _orderService.Create(orderDto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CustomerId.Should().Be(customer.Id);
        result.Data.Id.Should().NotBe(Guid.Empty);

        _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        _loggerMock.VerifyLog($"Order {result.Data.Id} created successfully.");
    }

    [Fact]
    public async Task CreateOrder_ShouldThrowValidationException_WhenCustomerNotFound()
    {
        var orderDto = new CreateOrderDtoBuilder().Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(orderDto.CustomerId))
            .ReturnsAsync((Customer?)null);

        Func<Task> act = async () => await _orderService.Create(orderDto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"Customer with id {orderDto.CustomerId} does not exist.");
    }

    [Fact]
    public async Task CreateOrder_ShouldThrowValidationException_WhenProductNotFound()
    {
        var customer = new CustomerBuilder().Build();
        var invalidProductId = Guid.NewGuid();

        var itemDto = new CreateOrderItemDtoBuilder()
            .WithProductId(invalidProductId)
            .Build();

        var orderDto = new CreateOrderDtoBuilder()
            .WithCustomerId(customer.Id)
            .WithItems(new List<CreateOrderItemDto> { itemDto })
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(customer.Id))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(r => r.GetProductById(invalidProductId))
            .ReturnsAsync((Product?)null);

        Func<Task> act = async () => await _orderService.Create(orderDto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"Product with id {invalidProductId} does not exist.");
    }

    [Fact]
    public async Task UpdateOrder_ShouldBeSuccessful()
    {
        var customer = new CustomerBuilder().Build();
        var productId = Guid.NewGuid();
        var product = new ProductBuilder().Build();

        var existingOrder = new Order(customer.Id);
        existingOrder.AddItem(productId, 2, 50m);

        var updateItemDto = new UpdateOrderItemDtoBuilder()
            .WithProductId(productId)
            .WithQuantity(5)
            .Build();

        var updateDto = new UpdateOrderDtoBuilder()
            .WithCustomerId(customer.Id)
            .WithItems(new List<UpdateOrderItemDto> { updateItemDto })
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(customer.Id))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(r => r.GetProductById(productId))
            .ReturnsAsync(product);

        _orderRepositoryMock
            .Setup(r => r.GetOrderByIdAsync(existingOrder.Id))
            .ReturnsAsync(existingOrder);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(existingOrder))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<Order, OrderResponse>(existingOrder))
            .Returns(new OrderResponse
            {
                Id = existingOrder.Id,
                CustomerId = existingOrder.CustomerId,
                Status = existingOrder.Status.ToString(),
                CreatedAt = existingOrder.CreatedAt
            });

        var result = await _orderService.Update(existingOrder.Id, updateDto);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingOrder.Id);

        _orderRepositoryMock.Verify(r => r.UpdateAsync(existingOrder), Times.Once);
        _loggerMock.VerifyLog($"Order {existingOrder.Id} updated successfully.");
    }

    [Fact]
    public async Task UpdateOrder_ShouldThrowValidationException_WhenCustomerIdIsNull()
    {
        var updateDto = new UpdateOrderDtoBuilder()
            .WithoutCustomerId()
            .Build();

        Func<Task> act = async () => await _orderService.Update(Guid.NewGuid(), updateDto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("CustomerId is required to update an order.");
    }

    [Fact]
    public async Task UpdateOrder_ShouldThrowValidationException_WhenCustomerNotFound()
    {
        var invalidCustomerId = Guid.NewGuid();

        var updateDto = new UpdateOrderDtoBuilder()
            .WithCustomerId(invalidCustomerId)
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(invalidCustomerId))
            .ReturnsAsync((Customer?)null);

        Func<Task> act = async () => await _orderService.Update(Guid.NewGuid(), updateDto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"Customer with id {invalidCustomerId} does not exist.");
    }

    [Fact]
    public async Task UpdateOrder_ShouldThrowValidationException_WhenOrderNotFound()
    {
        var customer = new CustomerBuilder().Build();
        var invalidOrderId = Guid.NewGuid();

        var updateDto = new UpdateOrderDtoBuilder()
            .WithCustomerId(customer.Id)
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(customer.Id))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetOrderByIdAsync(invalidOrderId))
            .ReturnsAsync((Order?)null);

        Func<Task> act = async () => await _orderService.Update(invalidOrderId, updateDto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"Order with id: {invalidOrderId} was not found.");
    }

    [Fact]
    public async Task UpdateOrder_ShouldThrowValidationException_WhenProductNotFound()
    {
        var customer = new CustomerBuilder().Build();
        var invalidProductId = Guid.NewGuid();

        var updateItemDto = new UpdateOrderItemDtoBuilder()
            .WithProductId(invalidProductId)
            .Build();

        var updateDto = new UpdateOrderDtoBuilder()
            .WithCustomerId(customer.Id)
            .WithItems(new List<UpdateOrderItemDto> { updateItemDto })
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(customer.Id))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(r => r.GetProductById(invalidProductId))
            .ReturnsAsync((Product?)null);

        Func<Task> act = async () => await _orderService.Update(Guid.NewGuid(), updateDto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"Product with id {invalidProductId} does not exist.");
    }

    [Fact]
    public async Task UpdateOrder_ShouldThrowValidationException_WhenProductIdIsNullInItemDto()
    {
        var customer = new CustomerBuilder().Build();

        var updateItemDto = new UpdateOrderItemDtoBuilder()
            .WithoutProductId()
            .Build();

        var updateDto = new UpdateOrderDtoBuilder()
            .WithCustomerId(customer.Id)
            .WithItems(new List<UpdateOrderItemDto> { updateItemDto })
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(customer.Id))
            .ReturnsAsync(customer);

        Func<Task> act = async () => await _orderService.Update(Guid.NewGuid(), updateDto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("ProductId is required to update an order item.");
    }

    [Fact]
    public async Task UpdateOrder_ShouldThrowValidationException_WhenStatusIsInvalid()
    {
        var customer = new CustomerBuilder().Build();
        var existingOrder = new OrderBuilder().Build();

        var updateDto = new UpdateOrderDtoBuilder()
            .WithCustomerId(customer.Id)
            .WithStatus("StatusInvalido")
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(customer.Id))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetOrderByIdAsync(existingOrder.Id))
            .ReturnsAsync(existingOrder);

        Func<Task> act = async () => await _orderService.Update(existingOrder.Id, updateDto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Invalid order status");
    }

    [Fact]
    public async Task UpdateOrder_ShouldThrowDomainValidationException_WhenStatusIsSetBackToPending()
    {
        var customer = new CustomerBuilder().Build();
        var existingOrder = new OrderBuilder().Build();

        var updateDto = new UpdateOrderDtoBuilder()
            .WithCustomerId(customer.Id)
            .WithStatus("Pending")
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(customer.Id))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetOrderByIdAsync(existingOrder.Id))
            .ReturnsAsync(existingOrder);

        Func<Task> act = async () => await _orderService.Update(existingOrder.Id, updateDto);

        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("Cannot change order back to Pending status.");
    }

    [Fact]
    public async Task UpdateOrder_ShouldUpdateStatusToPaid_WhenStatusIsPaid()
    {
        var customer = new CustomerBuilder().Build();

        var existingOrder = new OrderBuilder().Build();
        existingOrder.AddItem(Guid.NewGuid(), 1, 100m);

        var updateDto = new UpdateOrderDtoBuilder()
            .WithCustomerId(customer.Id)
            .WithStatus("Paid")
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(customer.Id))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetOrderByIdAsync(existingOrder.Id))
            .ReturnsAsync(existingOrder);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(existingOrder))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<Order, OrderResponse>(existingOrder))
            .Returns(new OrderResponse
            {
                Id = existingOrder.Id,
                CustomerId = existingOrder.CustomerId,
                Status = existingOrder.Status.ToString(),
                CreatedAt = existingOrder.CreatedAt
            });

        var result = await _orderService.Update(existingOrder.Id, updateDto);

        result.Success.Should().BeTrue();
        _orderRepositoryMock.Verify(r => r.UpdateAsync(existingOrder), Times.Once);
    }

    [Fact]
    public async Task UpdateOrder_ShouldUpdateStatusToShipped_WhenStatusIsShipped()
    {
        var customer = new CustomerBuilder().Build();

        var existingOrder = new OrderBuilder().BuildPaidOrder();

        var updateDto = new UpdateOrderDtoBuilder()
            .WithCustomerId(customer.Id)
            .WithStatus("Shipped")
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(customer.Id))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetOrderByIdAsync(existingOrder.Id))
            .ReturnsAsync(existingOrder);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(existingOrder))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<Order, OrderResponse>(existingOrder))
            .Returns(new OrderResponse
            {
                Id = existingOrder.Id,
                CustomerId = existingOrder.CustomerId,
                Status = existingOrder.Status.ToString(),
                CreatedAt = existingOrder.CreatedAt
            });

        var result = await _orderService.Update(existingOrder.Id, updateDto);

        result.Success.Should().BeTrue();
        _orderRepositoryMock.Verify(r => r.UpdateAsync(existingOrder), Times.Once);
    }

    [Fact]
    public async Task UpdateOrder_ShouldUpdateStatusToCancelled_WhenStatusIsCancelled()
    {
        var customer = new CustomerBuilder().Build();

        var existingOrder = new OrderBuilder().Build();

        var updateDto = new UpdateOrderDtoBuilder()
            .WithCustomerId(customer.Id)
            .WithStatus("Cancelled")
            .Build();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerById(customer.Id))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetOrderByIdAsync(existingOrder.Id))
            .ReturnsAsync(existingOrder);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(existingOrder))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<Order, OrderResponse>(existingOrder))
            .Returns(new OrderResponse
            {
                Id = existingOrder.Id,
                CustomerId = existingOrder.CustomerId,
                Status = existingOrder.Status.ToString(),
                CreatedAt = existingOrder.CreatedAt
            });

        var result = await _orderService.Update(existingOrder.Id, updateDto);

        result.Success.Should().BeTrue();
        _orderRepositoryMock.Verify(r => r.UpdateAsync(existingOrder), Times.Once);
    }


    [Fact]
    public async Task DeleteOrder_ShouldBeSuccessful()
    {
        var existingOrder = new OrderBuilder().Build();

        _orderRepositoryMock
            .Setup(r => r.GetOrderByIdAsync(existingOrder.Id))
            .ReturnsAsync(existingOrder);

        _orderRepositoryMock
            .Setup(r => r.DeleteAsync(existingOrder.Id))
            .Returns(Task.CompletedTask);

        var result = await _orderService.Delete(existingOrder.Id);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();

        _orderRepositoryMock.Verify(r => r.DeleteAsync(existingOrder.Id), Times.Once);
        _loggerMock.VerifyLog($"Deleting order {existingOrder.Id}");
        _loggerMock.VerifyLog($"Order {existingOrder.Id} deleted successfully.");
    }

    [Fact]
    public async Task DeleteOrder_ShouldThrowValidationException_WhenOrderNotFound()
    {
        var invalidId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(r => r.GetOrderByIdAsync(invalidId))
            .ReturnsAsync((Order?)null);

        Func<Task> act = async () => await _orderService.Delete(invalidId);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"Order with id: {invalidId} was not found.");
    }


    [Fact]
    public async Task GetAllOrders_ShouldBeSuccessful()
    {
        var orders = new List<Order>
        {
            new OrderBuilder().Build(),
            new OrderBuilder().Build()
        };

        _orderRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(orders);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<Order>, IEnumerable<OrderResponse>>(It.IsAny<IEnumerable<Order>>()))
            .Returns((IEnumerable<Order> o) => o.Select(order => new OrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt
            }));

        var result = await _orderService.GetAll();

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data!.Count().Should().Be(2);

        _loggerMock.VerifyLog($"Retrieved {result.Data!.Count()} orders.");
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturnEmptyList_WhenNoOrdersExist()
    {
        _orderRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Order>());

        _mapperMock
            .Setup(m => m.Map<IEnumerable<Order>, IEnumerable<OrderResponse>>(It.IsAny<IEnumerable<Order>>()))
            .Returns(Enumerable.Empty<OrderResponse>());

        var result = await _orderService.GetAll();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }


    [Fact]
    public async Task GetOrderById_ShouldBeSuccessful()
    {
        var existingOrder = new OrderBuilder().Build();

        _orderRepositoryMock
            .Setup(r => r.GetOrderByIdAsync(existingOrder.Id))
            .ReturnsAsync(existingOrder);

        _mapperMock
            .Setup(m => m.Map<Order, OrderResponse>(It.IsAny<Order>()))
            .Returns((Order order) => new OrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt
            });

        var result = await _orderService.GetOrderById(existingOrder.Id);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingOrder.Id);
        result.Data.CustomerId.Should().Be(existingOrder.CustomerId);

        _loggerMock.VerifyLog($"Order {existingOrder.Id} retrieved successfully.");
    }

    [Fact]
    public async Task GetOrderById_ShouldThrowValidationException_WhenOrderNotFound()
    {
        var invalidId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(r => r.GetOrderByIdAsync(invalidId))
            .ReturnsAsync((Order?)null);

        Func<Task> act = async () => await _orderService.GetOrderById(invalidId);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"Order with id: {invalidId} was not found.");
    }
}
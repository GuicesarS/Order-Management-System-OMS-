using Castle.Core.Resource;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Exceptions;
using OrderManagement.Application.Services;
using OrderManagement.Communication.Dtos.Customer;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Customer;
using OrderManagement.Domain.Exception;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Tests.Application.Builders;
using OrderManagement.Tests.Application.ExtensionMethods;
using OrderManagement.Tests.Domain.Builders;

namespace OrderManagement.Tests.Application.Services;

public class CustomerServiceTest
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICustomMapper> _mapperMock;
    private readonly Mock<ILogger<CustomerService>> _loggerMock;

    private readonly CustomerService _customerService;

    public CustomerServiceTest()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _mapperMock = new Mock<ICustomMapper>();
        _loggerMock = new Mock<ILogger<CustomerService>>();

        _customerService = new CustomerService(
            _customerRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateCustomer_ShouldBeSuccessfull()
    {
        var customerDto = new CreateCustomerDtoBuilder().Build();

        _customerRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Customer>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<Customer, CustomerResponse>(It.IsAny<Customer>()))
            .Returns((Customer customer) =>
                new CustomerResponse
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email.Value,
                    Phone = customer.Phone.Value,
                    Address = customer.Address,
                    CreatedAt = customer.CreatedAt
                });

        var result = await _customerService.Create(customerDto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.Name.Should().Be(customerDto.Name);
        result.Data.Email.Should().Be(customerDto.Email);
        result.Data.Phone.Should().Be(customerDto.Phone);
        result.Data.Address.Should().Be(customerDto.Address);
        result.Data.Id.Should().NotBe(Guid.Empty);

        _customerRepositoryMock.Verify(r =>
            r.AddAsync(It.IsAny<Customer>()), Times.Once);

        _loggerMock.VerifyLog($"Customer created with ID: {result.Data.Id}");
    }

    [Fact]
    public async Task CreateCustomer_ShouldThrowException_WhenEmailIsInvalid()
    {
        var customerDto = new CreateCustomerDtoBuilder()
            .WithInvalidEmail()
            .Build();

        Func<Task> act = async () => await _customerService.Create(customerDto);

        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("Email format is invalid.");
    }

    [Fact]
    public async Task UpdateCustomer_ShouldBeSuccessfull()
    {
        var existingCustomer = new CustomerBuilder().Build();
        var updateDto = new UpdateCustomerDtoBuilder().Build();

        _customerRepositoryMock
            .Setup(repo => repo.GetCustomerById(existingCustomer.Id))
            .ReturnsAsync(existingCustomer);

        _customerRepositoryMock
            .Setup(repo => repo.UpdateAsync(existingCustomer))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(map => map.Map<Customer, CustomerResponse>(existingCustomer))
            .Returns(new CustomerResponse
            {
                Id = existingCustomer.Id,
                Name = existingCustomer.Name,
                Email = existingCustomer.Email.Value,
                Phone = existingCustomer.Phone.Value,
                Address = existingCustomer.Address,
                CreatedAt = existingCustomer.CreatedAt
            });

        var result = await _customerService.Update(existingCustomer.Id, updateDto);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingCustomer.Id);

        _customerRepositoryMock.Verify(r =>
        r.UpdateAsync(existingCustomer), Times.Once);

        _loggerMock.VerifyLog($"Customer with ID: {existingCustomer.Id} updated successfully");
    }

    [Fact]
    public async Task UpdateCustomer_ShouldThrowException_WhenCustomerNotFound()
    {
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateCustomerDtoBuilder().Build();

        _customerRepositoryMock
            .Setup(repo => repo.GetCustomerById(invalidId))
            .ReturnsAsync((Customer?)null);

        Func<Task> act = async () => await _customerService.Update(invalidId, updateDto);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Customer with id: {invalidId} was not found.");
    }

    [Fact]
    public async Task DeleteCustomer_ShouldBeSuccessfull()
    {
        var existingCustomer = new CustomerBuilder().Build();

        _customerRepositoryMock
            .Setup(repo => repo.GetCustomerById(existingCustomer.Id))
            .ReturnsAsync(existingCustomer);

        _customerRepositoryMock
            .Setup(repo => repo.DeleteAsync(existingCustomer.Id))
            .Returns(Task.CompletedTask);

        var result = await _customerService.Delete(existingCustomer.Id);

        result.Success.Should().BeTrue();

        _customerRepositoryMock.Verify(repo =>
        repo.DeleteAsync(existingCustomer.Id), Times.Once);

        _loggerMock.VerifyLog($"Deleting customer with ID: {existingCustomer.Id}");
        _loggerMock.VerifyLog($"Customer with ID: {existingCustomer.Id} deleted successfully");

    }

    [Fact]
    public async Task DeleteCustomer_ShouldThrowException_WhenCustomerNotFound()
    {
        var id = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(repo => repo.GetCustomerById(id))
            .ReturnsAsync((Customer?)null);

        Func<Task> act = async () => await _customerService.Delete(id);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Customer with id: {id} was not found.");

    }

    [Fact]
    public async Task GetAllCustomers_ShouldBeSuccessful()
    {
        var customers = new List<Customer>
            {
                new Customer(
                    "João Silva",
                    Email.Create("joao@email.com"),
                    Phone.Create("5511999999999"),
                    "Rua A"
                ),
                new Customer(
                    "Maria Souza",
                    Email.Create("maria@email.com"),
                    Phone.Create("5531999999999"),
                    "Rua B"
                )
            };

        _customerRepositoryMock
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(customers);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<Customer>, IEnumerable<CustomerResponse>>(It.IsAny<IEnumerable<Customer>>()))
            .Returns((IEnumerable<Customer> c) => c.Select(c => new CustomerResponse
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email.Value,
                Phone = c.Phone.Value,
                Address = c.Address,
                CreatedAt = c.CreatedAt
            }));

        var result = await _customerService.GetAll();

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data!.Count().Should().Be(2);
        result.Data!.Select(c => c.Name).Should().Contain(new[] { "João Silva", "Maria Souza" });

        _loggerMock.VerifyLog($"Retrieved {result.Data!.Count()} customers");
    }

    [Fact]
    public async Task GetCustomerById_ShouldBeSuccessful()
    {
        var existingCustomer = new CustomerBuilder().Build();

        _customerRepositoryMock
            .Setup(repo => repo.GetCustomerById(existingCustomer.Id))
            .ReturnsAsync(existingCustomer);

        _mapperMock
            .Setup(m => m.Map<Customer, CustomerResponse>(It.IsAny<Customer>()))
            .Returns((Customer c) => new CustomerResponse
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email.Value,
                Phone = c.Phone.Value,
                Address = c.Address,
                CreatedAt = c.CreatedAt
            });

        var result = await _customerService.GetCustomerById(existingCustomer.Id);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingCustomer.Id);
        result.Data.Name.Should().Be(existingCustomer.Name);

        _loggerMock.VerifyLog($"Customer with ID: {existingCustomer.Id} retrieved successfully");
    }

    [Fact]
    public async Task GetCustomerById_ShouldThrowException_WhenCustomerNotFound()
    {
        var id = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(repo => repo.GetCustomerById(id))
            .ReturnsAsync((Customer?)null);

        Func<Task> act = async () => await _customerService.GetCustomerById(id);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Customer with id: {id} was not found.");
    }

}
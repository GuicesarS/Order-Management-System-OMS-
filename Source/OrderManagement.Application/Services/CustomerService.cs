using OrderManagement.Application.Common;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Exceptions;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Common.Extensions;
using OrderManagement.Communication.Dtos.Customer;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Customer;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace OrderManagement.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly ICustomMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ICustomerRepository repository, ICustomMapper mapper, ILogger<CustomerService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CustomerResponse>> Create(CreateCustomerDto customerDto)
    {
        _logger.LogInformation("Creating a new customer with email: {Email}", customerDto.Email);

        var customer = new Customer(
            customerDto.Name,
            Email.Create(customerDto.Email),
            customerDto.Phone,
            customerDto.Address);

        await _repository.AddAsync(customer);

        _logger.LogInformation("Customer created with ID: {CustomerId}", customer.Id);

        var response = _mapper.Map<Customer,CustomerResponse>(customer);

        return Result<CustomerResponse>.Ok(response);
    }
    public async Task<Result<CustomerResponse>> Update(Guid id, UpdateCustomerDto updateCustomerDto)
    {
        _logger.LogInformation("Updating customer with ID: {CustomerId}", id);

        var existingCustomer = await _repository.GetCustomerById(id);

        if (existingCustomer is null)
        {
            _logger.LogWarning("Customer with ID: {CustomerId} not found", id);
            throw new NotFoundException($"Customer with id: {id} was not found.");
        }

        var nameForUpdate = updateCustomerDto.Name.GetValueForUpdate();
        var emailForUpdate = updateCustomerDto.Email.GetValueForUpdate();
        var phoneForUpdate = updateCustomerDto.Phone.GetValueForUpdate();
        var addressForUpdate = updateCustomerDto.Address.GetValueForUpdate();

        existingCustomer.UpdateCustomerProfile(
            nameForUpdate,
            emailForUpdate is null ? null : Email.Create(emailForUpdate),
            phoneForUpdate,
            addressForUpdate);

        await _repository.UpdateAsync(existingCustomer);

        _logger.LogInformation("Customer with ID: {CustomerId} updated successfully", id);

        var response = _mapper.Map<Customer, CustomerResponse>(existingCustomer);
        return Result<CustomerResponse>.Ok(response);

    }
    public async Task<Result<bool>> Delete(Guid id)
    {
        _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);

        var existingCustomer = await _repository.GetCustomerById(id);

        if (existingCustomer is null)
        {
            _logger.LogWarning("Customer with ID: {CustomerId} not found", id);
            throw new NotFoundException($"Customer with id: {id} was not found.");
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation("Customer with ID: {CustomerId} deleted successfully", id);

        return Result<bool>.Ok(true);
    }
    public async Task<Result<IEnumerable<CustomerResponse>>> GetAll()
    {
        _logger.LogInformation("Retrieving all customers");

        var customers = await _repository.GetAllAsync();
        var customerResponses = _mapper.Map<IEnumerable<Customer>, IEnumerable<CustomerResponse>>(customers);
        
        _logger.LogInformation("Retrieved {CustomerCount} customers", customerResponses.Count());

        return Result<IEnumerable<CustomerResponse>>.Ok(customerResponses);
    }
    public async Task<Result<CustomerResponse>> GetCustomerById(Guid id)
    {
        _logger.LogInformation("Retrieving customer with ID: {CustomerId}", id);

        var existingCustomer = await _repository.GetCustomerById(id);

        if (existingCustomer is null)
        {
            _logger.LogWarning("Customer with ID: {CustomerId} not found", id);
            throw new NotFoundException($"Customer with id: {id} was not found.");
        }

        var customer = _mapper.Map<Customer, CustomerResponse>(existingCustomer);

        _logger.LogInformation("Customer with ID: {CustomerId} retrieved successfully", id);

        return Result<CustomerResponse>.Ok(customer);
    }
}

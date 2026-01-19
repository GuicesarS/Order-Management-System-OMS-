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

namespace OrderManagement.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly ICustomMapper _mapper;

    public CustomerService(ICustomerRepository repository, ICustomMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<CustomerResponse>> Create(CreateCustomerDto customerDto)
    {
        var customer = new Customer(
            customerDto.Name,
            Email.Create(customerDto.Email),
            customerDto.Phone,
            customerDto.Address);

        await _repository.AddAsync(customer);
        var response = _mapper.Map<Customer,CustomerResponse>(customer);

        return Result<CustomerResponse>.Ok(response);
    }
    public async Task<Result<CustomerResponse>> Update(Guid id, UpdateCustomerDto updateCustomerDto)
    {
        var existingCustomer = await _repository.GetCustomerById(id);

        if (existingCustomer is null)
            throw new NotFoundException($"Customer with id: {id} was not found.");

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

        var response = _mapper.Map<Customer, CustomerResponse>(existingCustomer);
        return Result<CustomerResponse>.Ok(response);

    }

    public async Task<Result<bool>> Delete(Guid id)
    {
        var existingCustomer = await _repository.GetCustomerById(id);

        if (existingCustomer is null)
            throw new NotFoundException($"Customer with id: {id} was not found.");

        await _repository.DeleteAsync(id);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<IEnumerable<CustomerResponse>>> GetAll()
    {
        var customers = await _repository.GetAllAsync();
        var customerResponses = _mapper.Map<IEnumerable<Customer>, IEnumerable<CustomerResponse>>(customers);
        return Result<IEnumerable<CustomerResponse>>.Ok(customerResponses);
    }

    public async Task<Result<CustomerResponse>> GetCustomerById(Guid id)
    {
        var existingCustomer = await _repository.GetCustomerById(id);

        if (existingCustomer is null)
            throw new NotFoundException($"Customer with id: {id} was not found.");

        var customer = _mapper.Map<Customer, CustomerResponse>(existingCustomer);
        return Result<CustomerResponse>.Ok(customer);
    }
}

using OrderManagement.Application.Common;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Interfaces;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Customer;
using OrderManagement.Domain.Interfaces;

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

    public async Task<Result<CustomerResponse>> Create(Customer customer)
    {
        await _repository.AddAsync(customer);
        var response = _mapper.Map<Customer,CustomerResponse>(customer);

        return Result<CustomerResponse>.Ok(response);
    }
    public async Task<Result<CustomerResponse>> Update(Guid id, Customer updateCustomer)
    {
        var existingCustomer = await _repository.GetCustomerById(id);

        if (existingCustomer is null)
            return Result<CustomerResponse>.Failure("Customer not found.");

        existingCustomer.ApplyChanges(
            updateCustomer.Name,
            updateCustomer.Email.Value,
            updateCustomer.Phone,
            updateCustomer.Address);

        await _repository.UpdateAsync(existingCustomer);

        var response = _mapper.Map<Customer, CustomerResponse>(existingCustomer);
        return Result<CustomerResponse>.Ok(response);

    }

    public async Task<Result<bool>> Delete(Guid id)
    {
        var existingCustomer = await _repository.GetCustomerById(id);

        if (existingCustomer is null)
            return Result<bool>.Failure("Customer not found.");

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
            return Result<CustomerResponse>.Failure("Customer not found.");

        var customer = _mapper.Map<Customer, CustomerResponse>(existingCustomer);
        return Result<CustomerResponse>.Ok(customer);
    }
}

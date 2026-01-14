using OrderManagement.Application.Common;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Validators.Customer;
using OrderManagement.Communication.Dtos.Customer;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Customer;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    public CustomerService(ICustomerRepository repository)
    {
      _repository = repository;
    }
    public Result<CustomerResponse> Create(CreateCustomerDto createCustomerDto)
    {
        var validator = new CreateCustomerDtoValidator();
        var resultValidation = validator.Validate(createCustomerDto);

        if(!resultValidation.IsValid)
            return Result<CustomerResponse>.Failure(resultValidation.Errors.First().ErrorMessage);

        var emailVo = Email.Create(createCustomerDto.Email);

        var customer = new Customer(
            createCustomerDto.Name, 
            emailVo, 
            createCustomerDto.Phone, 
            createCustomerDto.Address);
            
        _repository.Add(customer);

        return Result<CustomerResponse>.Ok(MapToCustomerResponse(customer));

    }
    public Result<CustomerResponse> Update(Guid id, UpdateCustomerDto updateCustomerDto)
    {
        var validator = new UpdateCustomerDtoValidator();
        var resultValidation = validator.Validate(updateCustomerDto);

        if(!resultValidation.IsValid)
            return Result<CustomerResponse>.Failure(resultValidation.Errors.First().ErrorMessage);

        var customer = _repository.GetCustomerById(id);

        if(customer is null)
            return Result<CustomerResponse>.Failure("Customer not found");

        customer.ApplyChanges(
            updateCustomerDto.Name, 
            updateCustomerDto.Email, 
            updateCustomerDto.Phone, 
            updateCustomerDto.Address);
        
        _repository.Update(customer);

        return Result<CustomerResponse>.Ok(MapToCustomerResponse(customer));
    }

    public Result<CustomerResponse> GetById(Guid id)
    {
        var customer = _repository.GetCustomerById(id);

        if (customer is null)
            return Result<CustomerResponse>.Failure("Customer not found");

        return Result<CustomerResponse>.Ok(MapToCustomerResponse(customer));
    }
    public Result<IEnumerable<CustomerResponse>> GetAll()
    {
        var customers = _repository.GetAll();
        var customerResponse = customers.Select(MapToCustomerResponse);

        return Result<IEnumerable<CustomerResponse>>.Ok(customerResponse);
       
    }
    public Result<bool> Delete(Guid id)
    {
        var customer = _repository.GetCustomerById(id);

        if (customer is null)
            return Result<bool>.Failure("Customer not found");

        _repository.Delete(id);

        return Result<bool>.Ok(true);
    }

    private static CustomerResponse MapToCustomerResponse(Customer customer) =>
        new CustomerResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email.ToString(),
            Phone = customer.Phone,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt
        };

}

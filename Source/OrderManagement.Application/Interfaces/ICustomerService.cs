using OrderManagement.Application.Common;
using OrderManagement.Communication.Dtos.Customer;
using OrderManagement.Communication.Responses;

namespace OrderManagement.Application.Interfaces;

public interface ICustomerService
{
    Task<Result<CustomerResponse>> Create(CreateCustomerDto customerDto);
    Task<Result<CustomerResponse>> Update(Guid id, UpdateCustomerDto updateCustomerDto);
    Task<Result<CustomerResponse>> GetCustomerById(Guid id);
    Task<Result<IEnumerable<CustomerResponse>>> GetAll();
    Task<Result<bool>> Delete(Guid id);
}

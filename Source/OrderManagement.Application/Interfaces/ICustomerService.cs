using OrderManagement.Application.Common;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Customer;

namespace OrderManagement.Application.Interfaces;

public interface ICustomerService
{
    Task<Result<CustomerResponse>> Create(Customer customer);
    Task<Result<CustomerResponse>> Update(Guid id, Customer updateCustomer);
    Task<Result<CustomerResponse>> GetCustomerById(Guid id);
    Task<Result<IEnumerable<CustomerResponse>>> GetAll();
    Task<Result<bool>> Delete(Guid id);
}

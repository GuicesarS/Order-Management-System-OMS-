using OrderManagement.Application.Common;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Customer;

namespace OrderManagement.Application.Interfaces;

public interface ICustomerService
{
    Result<CustomerResponse> Create(Customer customer);
    Result<CustomerResponse> Update(Guid id, Customer customer);
    Result<CustomerResponse> GetCustomerById(Guid id);
    Result<IEnumerable<CustomerResponse>> GetAll();
    Result<bool> Delete(Guid id);
}

using OrderManagement.Application.Common;
using OrderManagement.Communication.Dtos.Customer;
using OrderManagement.Communication.Responses;

namespace OrderManagement.Application.Interfaces;

public interface ICustomerService
{
    Result<CustomerResponse> Create(CreateCustomerDto createCustomerDto);
    Result<CustomerResponse> Update(UpdateCustomerDto updateCustomerDto);
    Result<CustomerResponse> GetById(Guid id);
    Result<IEnumerable<CustomerResponse>> GetAll();
    Result<bool> Delete(Guid id);
}

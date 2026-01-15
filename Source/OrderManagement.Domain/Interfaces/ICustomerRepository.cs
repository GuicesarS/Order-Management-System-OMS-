using OrderManagement.Domain.Entities.Customer;

namespace OrderManagement.Domain.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetCustomerById(Guid customerId);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetCustomerByEmail(string email);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Guid id);
}

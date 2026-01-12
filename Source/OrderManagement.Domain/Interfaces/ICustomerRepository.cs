using OrderManagement.Domain.Entities.Customer;

namespace OrderManagement.Domain.Interfaces;

public interface ICustomerRepository
{
    Customer? GetCustomerById(Guid customerId);
    IEnumerable<Customer> GetAll();
    void Add(Customer customer);
    void Update(Customer customer);
    void Delete(Guid id);
}

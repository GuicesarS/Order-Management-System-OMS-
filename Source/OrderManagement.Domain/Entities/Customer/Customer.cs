using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Entities.Customer;
public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public string Phone { get; private set; }
    public string Address { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Customer() { } // Ef Core

    public Customer(string name, Email email, string phone, string address)
    {
        if(string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.");

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
        CreatedAt = DateTime.UtcNow;
    }
}

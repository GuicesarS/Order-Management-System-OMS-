using OrderManagement.Domain.Exception;
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
        ValidateRequired(name, nameof(Name));
        ValidateRequired(phone, nameof(Phone));
        ValidateRequired(address, nameof(Address));

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
        CreatedAt = DateTime.UtcNow;
    }

    private static void ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{fieldName} cannot be null or empty.");

        if (value.Equals("string", StringComparison.OrdinalIgnoreCase))
            throw new DomainValidationException($"{fieldName} format is invalid.");
    }
    public void UpdateCustomerProfile(string? name, Email? email, string? phone, string? address)
    {
        if(name is not null) UpdateName(name);
        if(email is not null) UpdateEmail(email);
        if(phone is not null) UpdatePhone(phone);
        if(address is not null) UpdateAddress(address);
    }
    public void UpdateName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new DomainValidationException("Name cannot be null or empty.");

        Name = name;
    }
    public void UpdateEmail(Email email) => Email = email ?? throw new DomainValidationException(nameof(email));
    public void UpdatePhone(string phone)
    {
        ValidateRequired(phone, nameof(Phone));

        Phone = phone;
    }
    public void UpdateAddress(string address)
    {
        ValidateRequired(address, nameof(Address));

        Address = address;
    }
}

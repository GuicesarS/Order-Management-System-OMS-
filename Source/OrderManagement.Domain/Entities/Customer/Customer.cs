using OrderManagement.Domain.Exception;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Entities.Customer;
public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Phone Phone { get; private set; }
    public string Address { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Customer() { } // Ef Core

    public Customer(string name, Email email, Phone phone, string address)
    {
        if (email is null)
            throw new DomainValidationException("Email is required.");

        if(phone is null)
            throw new DomainValidationException("Phone is required.");

        ValidateRequired(name, nameof(Name));
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
    public void UpdateCustomerProfile(string? name, Email? email,Phone? phone, string? address)
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
    public void UpdatePhone(Phone phone) => Phone = phone ?? throw new DomainValidationException(nameof(phone));
    public void UpdateAddress(string address)
    {
        ValidateRequired(address, nameof(Address));

        Address = address;
    }
}

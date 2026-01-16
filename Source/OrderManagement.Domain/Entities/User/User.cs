using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exception;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Entities.User;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public DateTime CreatedAt{ get; private set; }
    public UserRole Role { get; private set; }

    protected User() { } // Ef Core

    public User(string name, Email email, UserRole role)
    {

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Name cannot be empty.");

        if (email is null)
            throw new DomainValidationException("Email cannot be null.");

        if (!Enum.IsDefined(typeof(UserRole), role))
            throw new DomainValidationException(
                $"The role value '{role}' is not valid. " +
                $"Allowed roles are: {string.Join(", ", Enum.GetNames(typeof(UserRole)))}."
            );


        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        CreatedAt = DateTime.UtcNow;
        Role = role;
    }

    public void UpdateUser(string name, Email email, UserRole role)
    {
        UpdateName(name);
        UpdateEmail(email);
        UpdateRole(role);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Name cannot be empty.");

        Name = name;
    }

    public void UpdateEmail(Email email)
    {
        Email = email ?? throw new DomainValidationException(nameof(email));
    }

    public void UpdateRole(UserRole role)
    {
        if (!Enum.IsDefined(typeof(UserRole), role))
            throw new DomainValidationException(
                $"The role value '{role}' is not valid. " +
                $"Allowed roles are: {string.Join(", ", Enum.GetNames(typeof(UserRole)))}."
            );


        Role = role;
    }
    

}

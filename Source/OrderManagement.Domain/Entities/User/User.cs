using OrderManagement.Domain.Enums;
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
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        CreatedAt = DateTime.UtcNow;
        Role = role;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.");

        Name = name;
    }

    public void UpdateEmail(Email email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public void UpdateRole(UserRole role)
    {
        Role = role;
    }

}

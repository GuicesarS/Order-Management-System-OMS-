using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exception;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Entities.User;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTime CreatedAt{ get; private set; }
    public UserRole Role { get; private set; }

    protected User() { } // Ef Core

    public User(string name, Email email, string passwordHash, UserRole role)
    {

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Name cannot be empty.");

        if (email is null)
            throw new DomainValidationException("Email cannot be null.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainValidationException("Password hash cannot be empty.");

        if (!Enum.IsDefined(typeof(UserRole), role))
            throw new DomainValidationException(
                $"The role value '{role}' is not valid. " +
                $"Allowed roles are: {string.Join(", ", Enum.GetNames(typeof(UserRole)))}."
            );


        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
        Role = role;
    }

    public void UpdateUser(string name, Email email, string passwordHash, UserRole role)
    {
        UpdateName(name);
        UpdateEmail(email);
        ChangePassword(passwordHash);
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
        if (email is null)
            throw new DomainValidationException("Email cannot be null.");

        Email = email;
    }

    public void ChangePassword(string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new DomainValidationException("Password hash cannot be empty.");

        PasswordHash = newPassword;
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

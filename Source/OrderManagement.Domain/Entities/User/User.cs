using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Entities.User;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public DateTime CreatedAt{ get; private set; }

    protected User() { } // Ef Core

    public User(string name, Email email)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }
}

namespace OrderManagement.Domain.Entities.User;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public DateTime CreatedAt{ get; set; }

    protected User() { } // Ef Core

    public User(string nome, string email)
    {
        Id = Guid.NewGuid();
        Name = nome;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }
}

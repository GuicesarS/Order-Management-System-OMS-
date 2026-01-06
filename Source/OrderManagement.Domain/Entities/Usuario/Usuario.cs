namespace OrderManagement.Domain.Entities.Usuario;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public DateTime Criacao { get; set; }

    protected Usuario() { } // Ef Core

    public Usuario(string nome, string email)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
        Criacao = DateTime.UtcNow;
    }
}

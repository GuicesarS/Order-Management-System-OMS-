using OrderManagement.Domain.Entities.User;

namespace OrderManagement.Domain.Interfaces;

public interface IUserRepository
{
    User? GetUserById(Guid userId);
    IEnumerable<User> GetAll();
    void Add(User order);
    void Update(User order);
    void Delete(Guid id);
}

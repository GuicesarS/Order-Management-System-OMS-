using OrderManagement.Domain.Entities.User;

namespace OrderManagement.Domain.Interfaces;

public interface IUserRepository
{
    User? GetOrderById(Guid userId);
    IEnumerable<User> GetAll();
    void Add(User order);
    void Update(User order);
    void Delete(Guid id);
}

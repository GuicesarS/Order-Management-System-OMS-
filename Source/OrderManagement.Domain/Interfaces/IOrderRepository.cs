using OrderManagement.Domain.Entities.Order;

namespace OrderManagement.Domain.Interfaces;

public interface IOrderRepository
{
    Order? GetOrderById(Guid orderId);
    IEnumerable<Order> GetAll();
    void Add(Order order);
    void Update(Order order);
    void Delete(Guid id);
}

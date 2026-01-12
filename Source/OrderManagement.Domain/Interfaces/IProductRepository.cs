using OrderManagement.Domain.Entities.Product;

namespace OrderManagement.Domain.Interfaces;

public interface IProductRepository
{
    Product? GetProductById(Guid productId);
    IEnumerable<Product> GetAll();
    void Add(Product product);
    void Update(Product product);
    void Delete(Guid id);
}

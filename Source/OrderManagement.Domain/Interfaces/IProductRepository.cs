using OrderManagement.Domain.Entities.Product;

namespace OrderManagement.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetProductById(Guid productId);
    Task<IEnumerable<Product>> GetAllAsync();
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
}

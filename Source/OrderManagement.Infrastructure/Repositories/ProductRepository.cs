using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities.Product;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly OrderManagementDbContext _dbContext;

    public ProductRepository(OrderManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Product product)
    {
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
       var product = await _dbContext.Products.FindAsync(id);

       if(product != null)
       {
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
       }
        
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbContext.Products.AsNoTracking().ToListAsync();
    }

    public async Task<Product?> GetProductById(Guid productId)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == productId);
    }

    public async Task UpdateAsync(Product product)
    {
        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync();
    }
}

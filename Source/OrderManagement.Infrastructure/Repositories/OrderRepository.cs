using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities.Order;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderManagementDbContext _dbContext;

    public OrderRepository(OrderManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await _dbContext.Orders.FindAsync(id);
        if (order != null)
        {
            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderById(Guid orderId)
    {
        return await _dbContext.Orders
           .AsNoTracking()
           .Include(o => o.Items)
           .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task UpdateAsync(Order order)
    {
        _dbContext.Orders.Update(order);
         await _dbContext.SaveChangesAsync();
    }
}

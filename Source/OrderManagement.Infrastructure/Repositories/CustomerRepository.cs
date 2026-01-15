using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities.Customer;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly OrderManagementDbContext _dbContext;
    public CustomerRepository(OrderManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Customer customer)
    {
        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await _dbContext.Customers.FindAsync(id);
        if(customer != null)
        {
            _dbContext.Customers.Remove(customer);
            await _dbContext.SaveChangesAsync();
        }
        
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
       return await _dbContext.Customers.AsNoTracking().ToListAsync();
    }

    public async Task<Customer?> GetCustomerByEmail(string email)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email.Value == email);
    }

    public async Task<Customer?> GetCustomerById(Guid customerId)
    {
        return await _dbContext.Customers
             .AsNoTracking()
             .FirstOrDefaultAsync(c => c.Id == customerId);
    }

    public async Task UpdateAsync(Customer customer)
    {
        _dbContext.Customers.Update(customer);
        await _dbContext.SaveChangesAsync();

    }
}

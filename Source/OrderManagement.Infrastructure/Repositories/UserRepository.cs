using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly OrderManagementDbContext _dbContext;

    public UserRepository(OrderManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(User user)
    {
       await _dbContext.Users.AddAsync(user);
       await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _dbContext.Users.FindAsync(id);

        if (user != null)
        {
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }
         
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
         return await _dbContext.Users.AsNoTracking().ToListAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email.Value == email);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId);
    }

    public async Task UpdateAsync(User user)
    {
         _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }
}

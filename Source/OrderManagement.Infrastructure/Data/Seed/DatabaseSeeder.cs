using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Security.Cryptography;

namespace OrderManagement.Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public static void Seed(OrderManagementDbContext context)
    {
        if (!context.Users.Any())
        {
            var admin = new User(
                name: "Admin",
                email: Email.Create("admin@admin.com"),
                passwordHash: new PasswordHasher().Hash("123456"),
                role: UserRole.Admin
            );

            context.Users.Add(admin);
            context.SaveChanges();
        }
    }
}
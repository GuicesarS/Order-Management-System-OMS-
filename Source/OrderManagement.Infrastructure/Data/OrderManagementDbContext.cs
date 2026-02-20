using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Common.Extensions;
using OrderManagement.Domain.Entities.Customer;
using OrderManagement.Domain.Entities.Order;
using OrderManagement.Domain.Entities.OrderItem;
using OrderManagement.Domain.Entities.Product;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Infrastructure.Data;

public class OrderManagementDbContext : DbContext
{
       public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options) : base(options) { }
       public DbSet<User> Users { get; set; }
       public DbSet<Customer> Customers { get; set; }
       public DbSet<Product> Products { get; set; }
       public DbSet<Order> Orders { get; set; }
       public DbSet<OrderItem> OrderItems { get; set; }

       protected override void OnModelCreating(ModelBuilder modelBuilder)
       {

              modelBuilder.Entity<User>(builder =>
              {
                     builder.HasKey(u => u.Id);

                     builder.Property(u => u.Name)
                      .HasMaxLength(200)
                      .IsRequired();

                     builder.Property(u => u.PasswordHash)
                       .HasMaxLength(255)
                       .IsRequired();

                     builder.Property(u => u.CreatedAt)
                      .IsRequired();

                     builder.Property(u => u.Role)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsRequired();

                     builder.OwnsOne(u => u.Email, email =>
                     {
                         email.Property(e => e.Value)
                             .HasColumnName("Email")
                             .HasMaxLength(255)
                             .IsRequired();
                     });
              });


              modelBuilder.Entity<Customer>(builder =>
              {
                     builder.HasKey(c => c.Id);

                     builder.Property(c => c.Name)
                      .HasMaxLength(200)
                      .IsRequired();

                     builder.OwnsOne(c => c.Email, email =>
                     {
                         email.Property(e => e.Value)
                             .HasColumnName("Email")
                             .HasMaxLength(255)
                             .IsRequired();
                     });

                     builder.OwnsOne(c => c.Phone, phone =>
                     {
                         phone.Property(p => p.Value)
                             .HasColumnName("Phone")
                             .HasMaxLength(20)
                             .IsRequired();
                     });
              });


              modelBuilder.Entity<Product>(builder =>
              {
                     builder.HasKey(p => p.Id);

                     builder.Property(p => p.Name)
                      .HasMaxLength(200)
                      .IsRequired();

                     builder.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();
              });


              modelBuilder.Entity<Order>(builder =>
              {
                     builder.HasKey(o => o.Id);

                     builder.Property(o => o.TotalAmount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();


                     builder.HasOne<Customer>()
                      .WithMany()
                      .HasForeignKey(o => o.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);


                     builder.HasMany(o => o.Items)
                      .WithOne()
                      .HasForeignKey(i => i.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
              });


              modelBuilder.Entity<OrderItem>(builder =>
              {
                     builder.HasKey(i => i.Id);

                     builder.Property(i => i.Quantity)
                      .IsRequired();

                     builder.Property(i => i.UnitPrice)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();


                     builder.HasOne<Product>()
                      .WithMany()
                      .HasForeignKey(i => i.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
              });
       }
}
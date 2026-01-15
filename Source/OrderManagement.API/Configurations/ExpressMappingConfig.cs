using ExpressMapper;
using OrderManagement.Communication.Dtos.Customer;
using OrderManagement.Communication.Dtos.Order;
using OrderManagement.Communication.Dtos.OrderItem;
using OrderManagement.Communication.Dtos.Product;
using OrderManagement.Communication.Dtos.User;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Customer;
using OrderManagement.Domain.Entities.Order;
using OrderManagement.Domain.Entities.OrderItem;
using OrderManagement.Domain.Entities.Product;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.API.Configurations;

public static class ExpressMappingConfig
{
    public static void RegisterMappings()
    {
        // User
        Mapper.Register<CreateUserDto, User>()
            .Member(dest => dest.Id, src => Guid.NewGuid())
            .Member(dest => dest.Email, src => Email.Create(src.Email));
        Mapper.Register<User, UserResponse>()
            .Member(dest => dest.Email, src => src.Email.Value);

        // Customer
        Mapper.Register<CreateCustomerDto, Customer>()
            .Member(dest => dest.Id, src => Guid.NewGuid())
            .Member(dest => dest.Email, src => Email.Create(src.Email));
        Mapper.Register<Customer, CustomerResponse>()
            .Member(dest => dest.Email, src => src.Email.Value);

        // Product
        Mapper.Register<CreateProductDto, Product>()
            .Member(dest => dest.Id, src => Guid.NewGuid());
        Mapper.Register<Product, ProductResponse>();

        // Order
        Mapper.Register<CreateOrderDto, Order>()
            .Member(dest => dest.Id, src => Guid.NewGuid());
        Mapper.Register<Order, OrderResponse>();

        // OrderItem
        Mapper.Register<CreateOrderItemDto, OrderItem>()
            .Member(dest => dest.Id, src => Guid.NewGuid())
            .Member(dest => dest.LineTotal, src => src.Quantity * src.UnitPrice);
        Mapper.Register<OrderItem, OrderItemResponse>();
    }
}
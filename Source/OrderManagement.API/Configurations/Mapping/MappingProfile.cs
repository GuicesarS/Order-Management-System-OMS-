using AutoMapper;
using OrderManagement.Communication.Dtos.Customer;
using OrderManagement.Communication.Dtos.User;
using OrderManagement.Communication.Dtos.Product;
using OrderManagement.Communication.Dtos.Order;
using OrderManagement.Communication.Dtos.OrderItem;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Customer;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.Entities.Product;
using OrderManagement.Domain.Entities.Order;
using OrderManagement.Domain.Entities.OrderItem;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.API.Configurations.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // VALUE OBJECTS
        CreateMap<Email, string>()
            .ConvertUsing(src => src.Value);

        CreateMap<Phone, string>()
            .ConvertUsing(src => src.Value);

        CreateMap<string, Email>()
            .ConvertUsing(src => Email.Create(src));

        CreateMap<string, Phone>()
            .ConvertUsing(src => Phone.Create(src));

        // USER -> RESPONSE (enum Role and Email as string)
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        // CUSTOMER
        CreateMap<Customer, CustomerResponse>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone.Value));

        // PRODUCT
        CreateMap<Product, ProductResponse>();

        // ORDER
        CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // ORDER ITEM
        CreateMap<OrderItem, OrderItemResponse>();

        CreateMap<CreateOrderDto, Order>()
            .ConstructUsing(src => new Order(src.CustomerId));

        CreateMap<CreateOrderItemDto, OrderItem>()
            .ConstructUsing(src => new OrderItem(Guid.Empty, src.ProductId, src.Quantity, src.UnitPrice))
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()));
    }
}

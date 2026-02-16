using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Domain.Security.Cryptography;
using OrderManagement.Domain.Security.Token;
using OrderManagement.Infrastructure.Repositories;
using OrderManagement.Infrastructure.Security.Cryptography;
using OrderManagement.Infrastructure.Security.Token;

namespace OrderManagement.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();  
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        AddTokens(services, configuration);

        return services;
    }

    private static void AddTokens(IServiceCollection services, IConfiguration configuration)
    {
        var expirationTimeInMinutes = configuration.GetValue<int>("Settings:Jwt:ExpirationTimeMinutes");

        var signingKey = configuration.GetValue<string>("Settings:Jwt:SigningKey")
            ?? throw new InvalidOperationException("Signing key for JWT is not configured.");

        services.AddScoped<IAccessTokenGenerator>(provider =>
            new AccessTokenGenerator(expirationTimeInMinutes, signingKey));
    }
}

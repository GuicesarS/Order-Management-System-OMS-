using Bogus;
using OrderManagement.Communication.Dtos.Product;

namespace OrderManagement.Tests.Application.Builders;

public class CreateProductDtoBuilder
{
    private string _name;
    private string _sku;
    private decimal _price;
    private int _stockQuantity;
    private bool _isActive;
    private readonly Faker _faker;

    public CreateProductDtoBuilder()
    {
        _faker = new Faker("pt_BR");
        _name = _faker.Commerce.ProductName();
        _sku = _faker.PickRandom(1, 10).ToString();
        _price = _faker.Random.Decimal(1m, 100m);
        _stockQuantity = _faker.PickRandom(1, 100);
        _isActive = true;
    }

    public CreateProductDtoBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public CreateProductDtoBuilder WithStockQuantity(int stockQuantity)
    {
        _stockQuantity = stockQuantity;
        return this;
    }

    public CreateProductDtoBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public CreateProductDto Build()
    {
        return new CreateProductDto
        {
            Name = _name,
            Sku = _sku,
            Price = _price,
            StockQuantity = _stockQuantity,
            IsActive = _isActive
        };
    }
}

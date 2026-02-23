using Bogus;
using OrderManagement.Communication.Dtos.Product;

namespace OrderManagement.Tests.Application.Builders;

public class UpdateProductDtoBuilder
{
    private string? _name;
    private string? _sku;
    private decimal? _price;
    private int? _stockQuantity;
    private readonly Faker _faker;

    public UpdateProductDtoBuilder()
    {
        _faker = new Faker("pt_BR");
        _name = _faker.Commerce.ProductName();
        _sku = _faker.PickRandom(1, 10).ToString();
        _price = _faker.Random.Decimal(1m, 100m);
        _stockQuantity = _faker.PickRandom(1, 100);
    }

    public UpdateProductDtoBuilder WithPrice(decimal? price)
    {
        _price = price;
        return this;
    }

    public UpdateProductDtoBuilder WithStockQuantity(int? stockQuantity)
    {
        _stockQuantity = stockQuantity;
        return this;
    }

    public UpdateProductDto Build()
    {
        return new UpdateProductDto
        {
            Name = _name,
            Sku = _sku,
            Price = _price,
            StockQuantity = _stockQuantity
        };
    }
}
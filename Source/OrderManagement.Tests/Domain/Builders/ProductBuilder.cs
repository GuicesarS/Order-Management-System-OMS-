using Bogus;
using OrderManagement.Domain.Entities.Product;
using OrderManagement.Domain.Exception;

namespace OrderManagement.Tests.Domain.Builders;

public class ProductBuilder
{
    private Faker _faker;

    private string _name;
    private string _sku;
    private decimal _price;
    private int _stockQuantity;
    private bool _isActive;

    public ProductBuilder()
    {
        _faker = new Faker("pt_BR");
        _name = _faker.Commerce.ProductName();
        _sku = _faker.PickRandom(1,10).ToString();
        _price = _faker.Random.Decimal(1m, 100m);
        _stockQuantity = _faker.PickRandom(1, 100);
        _isActive = true;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        if (price <= 0m)
            throw new DomainValidationException("Price must be greater than 0.");

        _price = price;
        return this;
    }

    public ProductBuilder WithStockQuantity(int stockQuantity)
    {
        if (stockQuantity <= 0)
            throw new DomainValidationException("StockQuantity must be greater than 0.");

        _stockQuantity = stockQuantity;
        return this;
    }
    public Product Build()
        => new Product(_name, _sku, _price, _stockQuantity, _isActive);
}

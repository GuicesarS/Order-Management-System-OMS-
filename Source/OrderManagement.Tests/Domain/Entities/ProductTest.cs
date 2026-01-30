using FluentAssertions;
using OrderManagement.Domain.Exception;
using OrderManagement.Tests.Domain.Builders;

namespace OrderManagement.Tests.Domain.Entities;

public class ProductTest
{
    // Constructor Tests

    [Fact]
    public void Create_ShouldBeSuccessful()
    {
        var product = new ProductBuilder()
            .WithStockQuantity(3)
            .WithPrice(30m)
            .Build();

        product.Should().NotBeNull();
        product.Id.Should().NotBe(Guid.Empty);
        product.StockQuantity.Should().Be(3);
        product.Price.Should().Be(30m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowException_WhenQuantityIsInvalid(int quantity)
    {
        Action act = () => new ProductBuilder()
            .WithStockQuantity(quantity)
            .WithPrice(30m)
            .Build();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("StockQuantity must be greater than 0.");

    }

    [Fact]
    public void Create_ShouldThrowException_WhenPriceIsInvalid()
    {
        Action act = () => new ProductBuilder()
            .WithStockQuantity(7)
            .WithPrice(0m)
            .Build();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Price must be greater than 0.");
    }

    // Update Method

    [Fact]
    public void Update_ShouldBeSuccessful()
    {
        var product = new ProductBuilder().Build();

        product.UpdateName("New_Product_Name");
        product.UpdateSku("012");

        product.Id.Should().NotBe(Guid.Empty);
        product.Name.Should().Be("New_Product_Name");
        product.Sku.Should().Be("012");
    }

    // Update Name Method

    [Fact]
    public void UpdateName_ShouldBeSuccessful()
    {
        var product = new ProductBuilder().Build();

        product.UpdateName("Random_Product");

        product.Should().NotBeNull();
        product.Name.Should().Be("Random_Product");
    }

    [Fact]
    public void UpdateName_ShouldThrowException_WhenNameIsDefaultString()
    {
        var product = new ProductBuilder().Build();

        Action act = () => product.UpdateName("string");

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*format is invalid*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateName_ShouldThrowException_WhenNameIsInvalid(string name)
    {
       var product = new ProductBuilder().Build();

        Action act = () => product.UpdateName(name);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Name*");
    }

    // Update Sku Method

    [Fact]
    public void UpdateSku_ShouldBeSuccessful()
    {
        var product = new ProductBuilder().Build();

        product.UpdateSku("021");

        product.Should().NotBeNull();
        product.Sku.Should().Be("021");
    }

    [Fact]
    public void UpdateName_ShouldThrowException_WhenSkuIsDefaultString()
    {
        var product = new ProductBuilder().Build();

        Action act = () => product.UpdateSku("string");

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*format is invalid*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateSku_ShouldThrowException_WhenSkuIsInvalid(string sku)
    {
        var product = new ProductBuilder().Build();

        Action act = () => product.UpdateSku(sku);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Sku*");
    }

    // Update Price Method

    [Fact]
    public void UpdatePrice_ShouldBeSuccessful()
    {
        var product = new ProductBuilder().Build();

        product.UpdatePrice(20m);

        product.Should().NotBeNull();
        product.Price.Should().Be(20m);
    }

    [Fact]
    public void UpdatePrice_ShouldThrowException_WhenPriceIsInvalid()
    {
        var product = new ProductBuilder().Build();

        Action act = () => product.UpdatePrice(0);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Price*");
                
    }

    // Update Stock Quantity Method

    [Fact]
    public void UpdateStockQuantity_ShouldBeSuccessful()
    {
        var product = new ProductBuilder().Build();

        product.UpdateStockQuantity(12);

        product.Should().NotBeNull();
        product.StockQuantity.Should().Be(12);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-999)]
    public void UpdateStockQuantity_ShouldThrowException_WhenStockQuantityIsInvalid(int stockQuantity)
    {
        var product = new ProductBuilder().Build();

        Action act = () => product.UpdateStockQuantity(stockQuantity);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*StockQuantity*");
    }

    // Apply Changes Method

    [Fact]
    public void ApplyChanges_ShouldUpdateAllProvidedFields()
    {
        var product = new ProductBuilder().Build();

        product.ApplyChanges(
            name: "Updated Name",
            sku: "999",
            price: 99m,
            stockQuantity: 10);

        product.Name.Should().Be("Updated Name");
        product.Sku.Should().Be("999");
        product.Price.Should().Be(99m);
        product.StockQuantity.Should().Be(10);
    }

    [Fact]
    public void ApplyChanges_ShouldIgnoreNullFields()
    {
        var product = new ProductBuilder().Build();

        var originalName = product.Name;
        var originalSku = product.Sku;

        product.ApplyChanges(
            name: null,
            sku: null,
            price: null,
            stockQuantity: null);

        product.Name.Should().Be(originalName);
        product.Sku.Should().Be(originalSku);
    }

    [Fact]
    public void ApplyChanges_ShouldThrowException_WhenPriceIsInvalid()
    {
        var product = new ProductBuilder().Build();

        Action act = () => product.ApplyChanges(
            name: null,
            sku: null,
            price: 0m,
            stockQuantity: null);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Price*");
    }

}

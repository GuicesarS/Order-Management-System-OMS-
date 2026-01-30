using FluentAssertions;
using OrderManagement.Domain.Entities.OrderItem;
using OrderManagement.Domain.Exception;

namespace OrderManagement.Tests.Domain.Entities;

public class OrderItemTest
{
    // Constructor Tests

    [Fact]
    public void Create_ShouldBeSuccessful()
    {
        var orderItem = new OrderItemBuilder();

        var result = orderItem.Build();

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.OrderId.Should().NotBe(Guid.Empty);
        result.ProductId.Should().NotBe(Guid.Empty);
        result.LineTotal.Should().Be(result.Quantity * result.UnitPrice);
        
    }

    [Fact]
    public void Create_ShouldThrowException_WhenQuantityIsLessThanOne()
    {

        Action act = () => new OrderItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            0,
            10m);
          
        
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Quantity must be at least 1.");
        
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Create_ShouldThrowException_WhenUnitPriceIsLessThanOne(decimal unitPrice)
    {
        Action act = () => new OrderItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            3,
            unitPrice);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("UnitPrice must be greater than 0.");

    }

    // Calculate LineTotal Test

    [Fact]
    public void Create_ShouldCalculateLineTotalCorrectly()
    {
        var orderItem = new OrderItemBuilder()
              .WithQuantity(2)
              .WithUnitPrice(10m)
              .Build();

        orderItem.LineTotal.Should().Be(20m);
        orderItem.Id.Should().NotBe(Guid.Empty);

    }

    // Update Methods

    [Fact]
    public void Update_ShouldBeSuccessful()
    {
       
        var orderItem = new OrderItemBuilder()
            .WithQuantity(2)
            .WithUnitPrice(12m)
            .Build();

        var quantity = 4;
        var unitPrice = 10m;

        orderItem.UpdateQuantity(quantity);
        orderItem.UpdateUnitPrice(unitPrice);

        orderItem.Should().NotBeNull();
        orderItem.Id.Should().NotBe(Guid.Empty);
        orderItem.OrderId.Should().NotBe(Guid.Empty);
        orderItem.ProductId.Should().NotBe(Guid.Empty);
        orderItem.Quantity.Should().Be(quantity);
        orderItem.UnitPrice.Should().Be(unitPrice);
        orderItem.LineTotal.Should().Be(40m);
        
    }

    [Fact]
    public void Update_ShouldThrowException_WhenQuantityIsInvalid()
    {
        var orderItem = new OrderItemBuilder()
           .WithQuantity(2)
           .WithUnitPrice(12m)
           .Build();

        var quantity = 0;

        Action act = () => orderItem.UpdateQuantity(quantity);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Quantity must be at least 1.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_ShouldThrowException_WhenUnitPriceIsInvalid(int unitPrice)
    {
        var orderItem = new OrderItemBuilder()
           .WithQuantity(2)
           .WithUnitPrice(12m)
           .Build();

        Action act = () => orderItem.UpdateUnitPrice(unitPrice);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("UnitPrice must be greater than 0.");
    }
}

using FluentAssertions;
using OrderManagement.Domain.Entities.OrderItem;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exception;
using OrderManagement.Tests.Domain.Builders;

namespace OrderManagement.Tests.Domain.Entities;

public class OrderTest
{
    // Constructor Tests

    [Fact]
    public void Create_ShouldBeSuccessful()
    {
        var order = new OrderBuilder().Build();

        order.Should().NotBeNull();
        order.Id.Should().NotBe(Guid.Empty);
        order.CustomerId.Should().NotBe(Guid.Empty);
        order.Status.Should().Be(OrderStatus.Pending);
        order.CreatedAt.Should().NotBe(null);
    }

    [Fact]
    public void Create_ShouldThrowException_CustomerIdEmpty()
    {
        var order = new OrderBuilder();

        Action act = () => order.BuildWithEmptyCustomerId();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*CustomerId*");

    }

    // Item's Operation Methods 

    [Fact]
    public void AddItem_ShouldBeSuccessful()
    {
        var order = new OrderBuilder().Build();

        var productId = Guid.NewGuid();

        order.AddItem(productId: productId, quantity: 3, unitPrice: 2m);

        order.Should().NotBeNull();
        order.Id.Should().NotBe(Guid.Empty);
        order.Items.Count.Should().Be(1);
        order.TotalAmount.Should().Be(6m);
        
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenQuantityIsInvalid()
    {
        var order = new OrderBuilder().Build();

        var productId = Guid.NewGuid();

        Action act = () => order.AddItem(productId: productId, quantity: 0, unitPrice: 2m);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Quantity*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void AddItem_ShouldThrowException_WhenUnitPriceIsInvalid(int unitPrice)
    {
        var order = new OrderBuilder().Build();

        var productId = Guid.NewGuid();

        Action act = () => order.AddItem(productId: productId, quantity: 3, unitPrice: unitPrice);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*UnitPrice*");
    }

    [Fact]
    public void UpdateItem_ShouldBeSucessfull()
    {
        var order = new OrderBuilder().Build();

        var productId = Guid.NewGuid();

        order.AddItem(productId: productId, quantity: 1, unitPrice: 1m);

        order.UpdateItem(productId, quantity: 4, unitPrice: 3m);

        order.Items.Should().HaveCount(1);
        order.TotalAmount.Should().Be(12m);

    }

    [Fact]
    public void UpdateItem_ShouldThrowException_WhenItemNotFound()
    {
        var order = new OrderBuilder().Build();

        var productId = Guid.NewGuid();

        order.AddItem(productId: productId, quantity: 1, unitPrice: 1m);

        Action act = () => order.UpdateItem(productId: Guid.NewGuid(), quantity: 4, unitPrice:3m);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Item*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void UpdateItem_ShouldThrowException_WhenQuantityIsInvalid(int quantity)
    {
        var order = new OrderBuilder().Build();

        var productId = Guid.NewGuid();

        order.AddItem(productId: productId, quantity: 1, unitPrice: 1m);

        Action act = () => order.UpdateItem(productId: productId, quantity: quantity, unitPrice: 3m);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Quantity*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void UpdateItem_ShouldThrowException_WhenUnitPriceIsInvalid(decimal unitPrice)
    {
        var order = new OrderBuilder().Build();

        var productId = Guid.NewGuid();

        order.AddItem(productId: productId, quantity: 1, unitPrice: 1m);

        Action act = () => order.UpdateItem(productId: productId, quantity: 4, unitPrice: unitPrice);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*UnitPrice*");
    }

    [Fact]
    public void RemoveItem_ShouldBeSuccessful()
    {
        var order = new OrderBuilder().Build();

        var productId = Guid.NewGuid();

        order.AddItem(productId: productId, quantity: 2, unitPrice: 2m);

        order.RemoveItem(productId: productId);

        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_ShouldThrowException_WhenItemNotFound()
    {
        var order = new OrderBuilder().Build();

        var productId = Guid.NewGuid();

        order.AddItem(productId: productId, quantity: 1, unitPrice: 1m);

        Action act = () => order.RemoveItem(productId: Guid.NewGuid());

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*Item*");
    }

    // PaidStatus Methods

    [Fact]
    public void MarkAsPaid_ShouldBeSuccessful()
    {
        var order = new OrderBuilder().BuildPaidOrder();

        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsCancelled_ShouldBeSuccessful()
    {
        var order = new OrderBuilder().BuildCancelledOrder();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void MarkAsShipped_ShouldBeSuccessful()
    {
        var order = new OrderBuilder().BuildShippedOrder();

        order.Status.Should().Be(OrderStatus.Shipped);
    }

   [Fact]
    public void MarkAsShipped_ShouldThrowException_WhenStatusIsPending()
    {
        var order = new OrderBuilder().Build();

        Action act = () => order.MarkAsShipped();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*paid*");

        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void MarkAsShipped_ShouldThrowException_WhenStatusIsCancelled()
    {
        var order = new OrderBuilder().Build();

        order.MarkAsCancelled();

        Action act = () => order.MarkAsShipped();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*paid*");

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void MarkAsShipped_ShouldThrowException_WhenStatusIsAlreadyShipped()
    {
        var order = new OrderBuilder().Build();

        var productId = Guid.NewGuid();

        order.AddItem(productId: productId, quantity: 1, unitPrice: 20);

        order.MarkAsPaid();
        order.MarkAsShipped();

        Action act = () => order.MarkAsShipped();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*paid*");

        order.Status.Should().Be(OrderStatus.Shipped);
    }

    // Tests After changed Order Entity

    [Fact]
    public void AddItem_ShouldThrowException_WhenOrderIsPaid()
    {
        var order = new OrderBuilder().BuildPaidOrder();
       
        Action act = () => order.AddItem(Guid.NewGuid(), 1, 10.00m);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*non-pending*");

        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenOrderIsCancelled()
    {
        var order = new OrderBuilder().BuildCancelledOrder();

        Action act = () => order.AddItem(Guid.NewGuid(), 1, 10.00m);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*non-pending*");

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenOrderIsShipped()
    {
        var order = new OrderBuilder().BuildShippedOrder();

        Action act = () => order.AddItem(Guid.NewGuid(), 1, 10.00m);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*non-pending*");

        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void UpdateItem_ShouldThrowException_WhenOrderIsPaid()
    {
        var order = new OrderBuilder().Build();
        var productId = Guid.NewGuid();
        order.AddItem(productId, 1, 10.00m);
        order.MarkAsPaid();

        Action act = () => order.UpdateItem(productId, 2, 15.00m);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*non-pending*");
    }

    [Fact]
    public void UpdateItem_ShouldThrowException_WhenOrderIsCancelled()
    {
        var order = new OrderBuilder().Build();
        var productId = Guid.NewGuid();
        order.AddItem(productId, 1, 10.00m);
        order.MarkAsCancelled();

        Action act = () => order.UpdateItem(productId, 2, 15.00m);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*non-pending*");
    }

    [Fact]
    public void UpdateItem_ShouldThrowException_WhenOrderIsShipped()
    {
        var order = new OrderBuilder().Build();
        var productId = Guid.NewGuid();
        order.AddItem(productId, 1, 10.00m);
        order.MarkAsPaid();
        order.MarkAsShipped();

        Action act = () => order.UpdateItem(productId, 2, 15.00m);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*non-pending*");
    }

    [Fact]
    public void RemoveItem_ShouldThrowException_WhenOrderIsPaid()
    {
        var order = new OrderBuilder().Build();
        var productId = Guid.NewGuid();
        order.AddItem(productId, 1, 10.00m);
        order.MarkAsPaid();

        Action act = () => order.RemoveItem(productId);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*non-pending*");
    }

    [Fact]
    public void RemoveItem_ShouldThrowException_WhenOrderIsCancelled()
    {
        var order = new OrderBuilder().Build();
        var productId = Guid.NewGuid();
        order.AddItem(productId, 1, 10.00m);
        order.MarkAsCancelled();

        Action act = () => order.RemoveItem(productId);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*non-pending*");
    }

    [Fact]
    public void RemoveItem_ShouldThrowException_WhenOrderIsShipped()
    {
        var order = new OrderBuilder().Build();
        var productId = Guid.NewGuid();
        order.AddItem(productId, 1, 10.00m);
        order.MarkAsPaid();
        order.MarkAsShipped();

        Action act = () => order.RemoveItem(productId);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*non-pending*");
    }

    [Fact]
    public void MarkAsPaid_ShouldThrowException_WhenOrderIsCancelled()
    {
        var order = new OrderBuilder().Build();
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.MarkAsCancelled();

        Action act = () => order.MarkAsPaid();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*cancelled*");
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void MarkAsPaid_ShouldThrowException_WhenOrderIsShipped()
    {
        var order = new OrderBuilder().Build();
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.MarkAsPaid();
        order.MarkAsShipped();

        Action act = () => order.MarkAsPaid();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*shipped*");
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void MarkAsPaid_ShouldThrowException_WhenOrderIsAlreadyPaid()
    {
        var order = new OrderBuilder().Build();
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.MarkAsPaid();

        Action act = () => order.MarkAsPaid();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*already paid*");

        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_ShouldThrowException_WhenOrderHasNoItems()
    {
        var order = new OrderBuilder().Build();

        Action act = () => order.MarkAsPaid();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*without items*");

        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsCancelled_ShouldThrowException_WhenOrderIsShipped()
    {
        var order = new OrderBuilder().Build();
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.MarkAsPaid();
        order.MarkAsShipped();

        Action act = () => order.MarkAsCancelled();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*shipped*");

        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void MarkAsCancelled_ShouldThrowException_WhenOrderIsAlreadyCancelled()
    {
        var order = new OrderBuilder().Build();
        order.MarkAsCancelled();

        Action act = () => order.MarkAsCancelled();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*already cancelled*");

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void MarkAsShipped_ShouldThrowException_WhenOrderHasNoItems()
    {
        var order = new OrderBuilder().Build();

        Action act = () => order.MarkAsShipped();

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*paid*");
    }

    [Fact]
    public void MarkAsPaid_ShouldSetPaidAtToCurrentDateTime()
    {
        var order = new OrderBuilder().Build();
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        var beforePaid = DateTime.UtcNow;

        order.MarkAsPaid();

        order.PaidAt.Should().NotBeNull();
        order.PaidAt.Should().BeCloseTo(beforePaid, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsCancelled_ShouldKeepPaidAt_WhenAlreadyPaid()
    {
        var order = new OrderBuilder().Build();
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.MarkAsPaid();
        var paidAtBeforeCancellation = order.PaidAt;

        order.MarkAsCancelled();

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.PaidAt.Should().Be(paidAtBeforeCancellation);
        order.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsCancelled_ShouldKeepPaidAtNull_WhenNotPaid()
    {
        var order = new OrderBuilder().Build();

        order.MarkAsCancelled();

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.PaidAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldSetCreatedAtToCurrentDateTime()
    {
        var beforeCreation = DateTime.UtcNow;
        var order = new OrderBuilder().Build();
        var afterCreation = DateTime.UtcNow;

        order.CreatedAt.Should().BeCloseTo(beforeCreation, precision: TimeSpan.FromSeconds(1));
        order.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        order.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void AddMultipleItems_ShouldRecalculateTotalCorrectly()
    {
        var order = new OrderBuilder().Build();

        order.AddItem(Guid.NewGuid(), quantity: 2, unitPrice: 10.00m); 
        order.AddItem(Guid.NewGuid(), quantity: 3, unitPrice: 5.00m); 
        order.AddItem(Guid.NewGuid(), quantity: 1, unitPrice: 7.50m);   

        order.Items.Should().HaveCount(3);
        order.TotalAmount.Should().Be(42.50m);
    }

    [Fact]
    public void UpdateItem_ShouldRecalculateTotal_WithMultipleItems()
    {
        var order = new OrderBuilder().Build();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        order.AddItem(productId1, quantity: 2, unitPrice: 10.00m);  
        order.AddItem(productId2, quantity: 3, unitPrice: 5.00m);   
                
        order.UpdateItem(productId1, quantity: 5, unitPrice: 8.00m); 

        order.TotalAmount.Should().Be(55.00m); 
    }

    [Fact]
    public void RemoveItem_ShouldRecalculateTotal_WithMultipleItems()
    {
        var order = new OrderBuilder().Build();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        order.AddItem(productId1, quantity: 2, unitPrice: 10.00m);  
        order.AddItem(productId2, quantity: 3, unitPrice: 5.00m);

        order.RemoveItem(productId1);

        order.Items.Should().HaveCount(1);
        order.TotalAmount.Should().Be(15.00m);
    }

    [Fact]
    public void StatusFlow_ShouldFollowCorrectPath_FromPendingToShipped()
    {
        var order = new OrderBuilder().Build();
        order.AddItem(Guid.NewGuid(), 1, 50.00m);

        order.Status.Should().Be(OrderStatus.Pending);
        order.PaidAt.Should().BeNull();

        order.MarkAsPaid();
        order.Status.Should().Be(OrderStatus.Paid);
        order.PaidAt.Should().NotBeNull();

        var paidAt = order.PaidAt;

        order.MarkAsShipped();
        order.Status.Should().Be(OrderStatus.Shipped);
        order.PaidAt.Should().Be(paidAt);
    }
}

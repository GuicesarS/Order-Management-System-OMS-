using OrderManagement.Domain.Exception;

namespace OrderManagement.Domain.Entities.Product;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Sku { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    protected Product() { } // EF Core

    public Product(string name, string sku, decimal price, int stockQuantity, bool isActive)
    {
        ValidateRequired(name, nameof(Name));
        ValidateRequired(sku, nameof(Sku));
       
        if (price <= 0)
            throw new DomainValidationException("Price must be greater than 0.");

        if (stockQuantity <= 0)
            throw new DomainValidationException("StockQuantity must be greater than 0.");

        Id = Guid.NewGuid();
        Name = name;
        Sku = sku;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    private static void ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{fieldName} cannot be null or empty.");

        if (value.Equals("string", StringComparison.OrdinalIgnoreCase))
            throw new DomainValidationException($"{fieldName} format is invalid.");
    }
    public void ApplyChanges(string? name, string? sku, decimal? price, int? stockQuantity)
    {
        if (name is not null) UpdateName(name);
        if (sku is not null) UpdateSku(sku);
        if (price.HasValue) UpdatePrice(price.Value);
        if (stockQuantity.HasValue) UpdateStockQuantity(stockQuantity.Value);
    }

    public void UpdateName(string name)
    {
        ValidateRequired(name, nameof(Name));
        Name = name;
    }

    public void UpdateSku(string sku)
    {
       ValidateRequired(sku, nameof(Sku));
        Sku = sku;
    }

    public void UpdatePrice(decimal price)
    {
        if (price <= 0)
            throw new DomainValidationException("Price must be greater than 0.");

        Price = price;
    }

    public void UpdateStockQuantity(int stockQuantity)
    {
        if (stockQuantity <= 0)
            throw new DomainValidationException("StockQuantity must be greater than 0.");

        StockQuantity = stockQuantity;
    }
}

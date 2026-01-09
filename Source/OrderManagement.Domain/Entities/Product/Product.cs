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
    protected Product() { } // Ef Core

    public Product(string name,string sku,decimal price,int stockQuantity,bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("Sku is required.");
        if (price < 0)
            throw new ArgumentException("Price must be greater or equal to 0.");
        if (stockQuantity < 0)
            throw new ArgumentException("StockQuantity must be greater or equal to 0.");

        Id = Guid.NewGuid();
        Name = name;
        Sku = sku;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }
}

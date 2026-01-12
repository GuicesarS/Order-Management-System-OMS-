namespace OrderManagement.Communication.Dtos.Product;

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
    public bool? IsActive { get; set; }
}

namespace PitstopWeb.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    public string Brand { get; set; } = string.Empty;
    public int Stock { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public List<ProductCompatibility> Compatibility { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = new();
}

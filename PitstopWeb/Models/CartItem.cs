namespace PitstopWeb.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Image { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }

    public decimal LineTotal => Price * Quantity;
}

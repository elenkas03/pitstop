namespace PitstopWeb.Models;

public class ProductCompatibility
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int YearFrom { get; set; }
    public int YearTo { get; set; }
}

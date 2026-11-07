using PitstopWeb.Models;

namespace PitstopWeb.Models.ViewModels;

public class ProductFilterInput
{
    public string? Q { get; set; }
    public string? Category { get; set; }
    public List<string> Brand { get; set; } = new();
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
}

public class ProductListViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<string> AllBrands { get; set; } = new();
    public List<string> AllMakes { get; set; } = new();
    public ProductFilterInput Filters { get; set; } = new();
    public int Total { get; set; }
    public int TotalPages { get; set; }
}

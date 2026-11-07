using PitstopWeb.Models;

namespace PitstopWeb.Areas.Admin.Models;

public class DashboardViewModel
{
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public List<Product> LowStock { get; set; } = new();
}

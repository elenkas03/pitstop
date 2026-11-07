using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PitstopWeb.Areas.Admin.Models;
using PitstopWeb.Data;
using PitstopWeb.Models;

namespace PitstopWeb.Areas.Admin.Controllers;

public class DashboardController : AdminControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new DashboardViewModel
        {
            ProductCount = await _context.Products.CountAsync(),
            OrderCount = await _context.Orders.CountAsync(),
            Revenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered)
                .SumAsync(o => (decimal?)o.Total) ?? 0,
            LowStock = await _context.Products
                .Where(p => p.Stock <= 5)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync(),
        };

        return View(vm);
    }
}

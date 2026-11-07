using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PitstopWeb.Data;
using PitstopWeb.Models;

namespace PitstopWeb.Areas.Admin.Controllers;

public class OrdersController : AdminControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            order.Status = status;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}

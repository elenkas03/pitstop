using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PitstopWeb.Data;
using PitstopWeb.Models;
using PitstopWeb.Services;

namespace PitstopWeb.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CartService _cart;

    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CartService cart)
    {
        _context = context;
        _userManager = userManager;
        _cart = cart;
    }

    public async Task<IActionResult> Confirmation(int id, bool success = false)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Admin");

        if (user == null || (order.UserId != user.Id && !isAdmin))
        {
            return NotFound();
        }

        if (success)
        {
            _cart.Clear();
        }

        return View(order);
    }
}

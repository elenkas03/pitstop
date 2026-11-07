using Microsoft.AspNetCore.Mvc;
using PitstopWeb.Data;
using PitstopWeb.Services;

namespace PitstopWeb.Controllers;

public class CartController : Controller
{
    private readonly CartService _cart;
    private readonly ApplicationDbContext _context;

    public CartController(CartService cart, ApplicationDbContext context)
    {
        _cart = cart;
        _context = context;
    }

    public IActionResult Index()
    {
        return View(_cart.GetItems());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            _cart.AddItem(product, quantity);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        _cart.RemoveItem(productId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        _cart.UpdateQuantity(productId, quantity);
        return RedirectToAction(nameof(Index));
    }
}

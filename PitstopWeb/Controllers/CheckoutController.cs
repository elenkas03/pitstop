using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PitstopWeb.Data;
using PitstopWeb.Models;
using PitstopWeb.Models.ViewModels;
using PitstopWeb.Services;
using Stripe;
using Stripe.Checkout;

namespace PitstopWeb.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly CartService _cart;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public CheckoutController(
        CartService cart,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IConfiguration config)
    {
        _cart = cart;
        _context = context;
        _userManager = userManager;
        _config = config;
    }

    public IActionResult Index()
    {
        var items = _cart.GetItems();
        if (!items.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        return View(new CheckoutViewModel { Items = items });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        var items = _cart.GetItems();
        model.Items = items;

        if (!items.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var productIds = items.Select(i => i.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        foreach (var item in items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null || product.Stock < item.Quantity)
            {
                ModelState.AddModelError(string.Empty, $"Недостатъчна наличност за {item.Name}");
                return View(model);
            }
        }

        var order = new Order
        {
            UserId = user.Id,
            Status = OrderStatus.Pending,
            Total = items.Sum(i => i.LineTotal),
            ShippingName = model.ShippingName,
            ShippingAddress = model.ShippingAddress,
            ShippingCity = model.ShippingCity,
            Phone = model.Phone,
            Items = items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                PriceAtPurchase = i.Price,
            }).ToList(),
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var baseUrl = _config["App:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";

        var sessionOptions = new SessionCreateOptions
        {
            Mode = "payment",
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = items.Select(i => new SessionLineItemOptions
            {
                Quantity = i.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "eur",
                    UnitAmount = (long)Math.Round(i.Price * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions { Name = i.Name },
                },
            }).ToList(),
            SuccessUrl = $"{baseUrl}/Orders/Confirmation/{order.Id}?success=true",
            CancelUrl = $"{baseUrl}/Checkout",
            Metadata = new Dictionary<string, string> { { "orderId", order.Id.ToString() } },
        };

        try
        {
            var service = new SessionService();
            var session = await service.CreateAsync(sessionOptions);

            order.StripeSessionId = session.Id;
            await _context.SaveChangesAsync();

            return Redirect(session.Url);
        }
        catch (StripeException ex)
        {
            ModelState.AddModelError(string.Empty, $"Грешка при плащането: {ex.Message}");
            return View(model);
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        var webhookSecret = _config["Stripe:WebhookSecret"];

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);
        }
        catch (StripeException)
        {
            return BadRequest();
        }

        if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = stripeEvent.Data.Object as Session;
            if (session?.Metadata != null && session.Metadata.TryGetValue("orderId", out var orderIdStr) && int.TryParse(orderIdStr, out var orderId))
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order != null && order.Status == OrderStatus.Pending)
                {
                    order.Status = OrderStatus.Paid;
                    foreach (var item in order.Items)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Stock -= item.Quantity;
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }

        return Ok();
    }
}

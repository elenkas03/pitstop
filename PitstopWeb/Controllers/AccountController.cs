using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PitstopWeb.Data;
using PitstopWeb.Models;
using PitstopWeb.Models.ViewModels;

namespace PitstopWeb.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: true, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Грешен имейл или парола");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var existing = await _userManager.FindByEmailAsync(model.Email);
        if (existing != null)
        {
            ModelState.AddModelError(string.Empty, "Вече има регистрация с този имейл");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.Name,
            EmailConfirmed = true,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        await _signInManager.SignInAsync(user, isPersistent: true);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        var roles = await _userManager.GetRolesAsync(user);
        ViewBag.Role = roles.Contains("Admin") ? "Администратор" : "Клиент";
        ViewBag.OrderCount = await _context.Orders.CountAsync(o => o.UserId == user.Id);

        return View(user);
    }

    [Authorize]
    public async Task<IActionResult> Orders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }
}

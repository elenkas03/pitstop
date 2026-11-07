using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PitstopWeb.Data;
using PitstopWeb.Models;
using PitstopWeb.Services;

namespace PitstopWeb.Areas.Admin.Controllers;

public class ProductsController : AdminControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await CategoryOptionsAsync();
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, string? imagesText)
    {
        product.Slug = SlugHelper.Slugify(product.Name) + "-" + Guid.NewGuid().ToString("N")[..6];
        product.Images = ParseImages(imagesText);

        ModelState.Remove(nameof(Product.Slug));
        ModelState.Remove(nameof(Product.Category));
        ModelState.Remove(nameof(Product.Images));

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await CategoryOptionsAsync();
            return View(product);
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        ViewBag.Categories = await CategoryOptionsAsync();
        ViewBag.ImagesText = string.Join("\n", product.Images);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product, string? imagesText)
    {
        if (id != product.Id) return NotFound();

        var existing = await _context.Products.FindAsync(id);
        if (existing == null) return NotFound();

        ModelState.Remove(nameof(Product.Slug));
        ModelState.Remove(nameof(Product.Category));
        ModelState.Remove(nameof(Product.Images));

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await CategoryOptionsAsync();
            ViewBag.ImagesText = imagesText;
            return View(product);
        }

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Sku = product.Sku;
        existing.Brand = product.Brand;
        existing.Stock = product.Stock;
        existing.CategoryId = product.CategoryId;
        existing.IsFeatured = product.IsFeatured;
        existing.Images = ParseImages(imagesText);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> CategoryOptionsAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
    }

    private static List<string> ParseImages(string? imagesText)
    {
        if (string.IsNullOrWhiteSpace(imagesText)) return new List<string>();
        return imagesText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();
    }

}

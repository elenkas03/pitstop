using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PitstopWeb.Data;
using PitstopWeb.Models;
using PitstopWeb.Services;

namespace PitstopWeb.Areas.Admin.Controllers;

public class CategoriesController : AdminControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RedirectToAction(nameof(Index));
        }

        var baseSlug = SlugHelper.Slugify(name);
        var exists = await _context.Categories.AnyAsync(c => c.Slug == baseSlug);
        var slug = exists ? $"{baseSlug}-{Guid.NewGuid().ToString("N")[..4]}" : baseSlug;

        _context.Categories.Add(new Category { Name = name.Trim(), Slug = slug });
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category != null && !category.Products.Any())
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PitstopWeb.Data;
using PitstopWeb.Models.ViewModels;

namespace PitstopWeb.Controllers;

public class ProductsController : Controller
{
    private const int PageSize = 12;
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index([FromQuery] ProductFilterInput filters)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Q))
        {
            var q = filters.Q.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(q) ||
                p.Brand.ToLower().Contains(q) ||
                p.Sku.ToLower().Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(filters.Category))
        {
            query = query.Where(p => p.Category.Slug == filters.Category);
        }

        if (filters.Brand.Any())
        {
            query = query.Where(p => filters.Brand.Contains(p.Brand));
        }

        if (filters.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filters.MinPrice.Value);
        }

        if (filters.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filters.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Make) || !string.IsNullOrWhiteSpace(filters.Model) || filters.Year.HasValue)
        {
            query = query.Where(p => p.Compatibility.Any(c =>
                (string.IsNullOrWhiteSpace(filters.Make) || c.Make == filters.Make) &&
                (string.IsNullOrWhiteSpace(filters.Model) || c.Model.ToLower().Contains(filters.Model.ToLower())) &&
                (!filters.Year.HasValue || (c.YearFrom <= filters.Year.Value && c.YearTo >= filters.Year.Value))));
        }

        var total = await query.CountAsync();
        var page = filters.Page > 0 ? filters.Page : 1;

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var vm = new ProductListViewModel
        {
            Products = products,
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync(),
            AllBrands = await _context.Products.Select(p => p.Brand).Distinct().OrderBy(b => b).ToListAsync(),
            AllMakes = await _context.ProductCompatibilities.Select(c => c.Make).Distinct().OrderBy(m => m).ToListAsync(),
            Filters = filters,
            Total = total,
            TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)),
        };
        vm.Filters.Page = page;

        return View(vm);
    }

    [HttpGet("Products/Details/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Compatibility)
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (product == null) return NotFound();

        return View(product);
    }
}

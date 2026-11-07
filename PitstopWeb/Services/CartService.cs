using System.Text.Json;
using PitstopWeb.Models;

namespace PitstopWeb.Services;

public class CartService
{
    private const string SessionKey = "Cart";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    public List<CartItem> GetItems()
    {
        var json = Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json)) return new List<CartItem>();
        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    private void SaveItems(List<CartItem> items)
    {
        Session.SetString(SessionKey, JsonSerializer.Serialize(items));
    }

    public void AddItem(Product product, int quantity = 1)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.ProductId == product.Id);

        if (existing != null)
        {
            existing.Quantity = Math.Min(existing.Quantity + quantity, existing.Stock);
        }
        else
        {
            items.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Price = product.Price,
                Image = product.Images.FirstOrDefault(),
                Quantity = Math.Min(quantity, product.Stock),
                Stock = product.Stock,
            });
        }

        SaveItems(items);
    }

    public void RemoveItem(int productId)
    {
        var items = GetItems();
        items.RemoveAll(i => i.ProductId == productId);
        SaveItems(items);
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            existing.Quantity = Math.Max(1, Math.Min(quantity, existing.Stock));
            SaveItems(items);
        }
    }

    public void Clear()
    {
        Session.Remove(SessionKey);
    }

    public int TotalItems() => GetItems().Sum(i => i.Quantity);

    public decimal TotalPrice() => GetItems().Sum(i => i.LineTotal);
}

namespace PitstopWeb.Models;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }
    public string ShippingName { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? StripeSessionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}

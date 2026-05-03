namespace guitar_shop.Models;

public class ProfileViewModel
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DeliveryAddress { get; set; }
    public List<CartItem> CartItems { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}

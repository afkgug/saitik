namespace guitar_shop.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; } = false;
    public string? ConfirmationToken { get; set; }
    public string? FullName { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Навигационное свойство
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

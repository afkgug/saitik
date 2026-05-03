namespace guitar_shop.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    
    // Навигационное свойство
    public virtual Order? Order { get; set; }
}

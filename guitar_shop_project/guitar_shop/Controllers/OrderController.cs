using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using guitar_shop.Data;
using guitar_shop.Models;

namespace guitar_shop.Controllers;

public class OrderController : Controller
{
    private readonly AppDbContext _db;
    private const string CartSessionKey = "CartItems";

    public OrderController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Оформление заказа";
        
        // Проверка авторизации
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            // Сохраняем текущую корзину и перенаправляем на страницу требования авторизации
            return RedirectToAction("RequireAuth");
        }

        var json = HttpContext.Session.GetString(CartSessionKey);
        var cart = json != null ? JsonSerializer.Deserialize<List<CartItem>>(json) : new List<CartItem>();
        return View(cart);
    }

    // Страница требования авторизации
    public IActionResult RequireAuth()
    {
        ViewData["Title"] = "Требуется авторизация";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(string name, string email, string address)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("RequireAuth");
        }

        var json = HttpContext.Session.GetString(CartSessionKey);
        var cartItems = json != null ? JsonSerializer.Deserialize<List<CartItem>>(json) : new List<CartItem>();

        if (cartItems.Count == 0)
        {
            ViewBag.Error = "Корзина пуста";
            return View(cartItems);
        }

        // Создаем заказ
        var order = new Order
        {
            UserId = userId.Value,
            TotalAmount = cartItems.Sum(i => i.Price * i.Quantity),
            CustomerName = name,
            CustomerEmail = email,
            DeliveryAddress = address,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Items = cartItems.Select(i => new OrderItem
            {
                ProductName = i.Name,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // Очищаем корзину
        HttpContext.Session.Remove(CartSessionKey);
        ViewData["Success"] = true;
        return View(new List<CartItem>());
    }
}
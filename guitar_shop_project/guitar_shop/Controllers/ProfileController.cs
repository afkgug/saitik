using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using guitar_shop.Data;
using guitar_shop.Models;

namespace guitar_shop.Controllers;

public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private const string CartSessionKey = "CartItems";

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Профиль";

        // Проверка авторизации
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Profile" });
        }

        var user = await _db.Users
            .Include(u => u.Orders)
                .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (user == null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Profile" });
        }

        // Получаем корзину из сессии
        var json = HttpContext.Session.GetString(CartSessionKey);
        var cartItems = json != null 
            ? System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(json) 
            : new List<CartItem>();

        var model = new ProfileViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName ?? user.Username,
            DeliveryAddress = user.DeliveryAddress,
            CartItems = cartItems,
            Orders = user.Orders.OrderByDescending(o => o.CreatedAt).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        ViewData["Title"] = "Редактировать профиль";

        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Profile/Edit" });
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var model = new ProfileViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName ?? user.Username,
            DeliveryAddress = user.DeliveryAddress
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ProfileViewModel model)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue || userId != model.UserId)
        {
            return RedirectToAction("Login", "Auth");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        // Обновляем данные
        user.FullName = model.FullName;
        user.DeliveryAddress = model.DeliveryAddress;
        
        await _db.SaveChangesAsync();

        // Обновляем имя в сессии
        HttpContext.Session.SetString("UserName", user.FullName ?? user.Username);

        ViewBag.Success = "Профиль успешно обновлен";
        
        return RedirectToAction("Index");
    }

    // AJAX: Получение частичного представления корзины
    public IActionResult GetCartPartial()
    {
        var json = HttpContext.Session.GetString("CartItems");
        var cartItems = json != null 
            ? System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(json) 
            : new List<CartItem>();
        
        return PartialView("_CartPartial", cartItems);
    }
}
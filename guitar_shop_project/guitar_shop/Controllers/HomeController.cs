using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using guitar_shop.Models;
using guitar_shop.Services;

namespace guitar_shop.Controllers;

public class HomeController : Controller
{
    private readonly IWebHostEnvironment _env;
    private const string CartSessionKey = "CartItems";

    public HomeController(IWebHostEnvironment env)
    {
        _env = env;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Главная";
        ViewData["CartPreview"] = GetCartItems();
        var guitars = GuitarService.GetAll(_env) ?? new List<Guitar>();
        return View(guitars);
    }

    private List<CartItem> GetCartItems()
    {
        var json = HttpContext.Session.GetString(CartSessionKey);
        return json != null ? JsonSerializer.Deserialize<List<CartItem>>(json) : new List<CartItem>();
    }
}
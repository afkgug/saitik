using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using guitar_shop.Models;
using guitar_shop.Services;

namespace guitar_shop.Controllers;

public class CartController : Controller
{
    private const string CartSessionKey = "CartItems";
    private readonly IWebHostEnvironment _env;

    public CartController(IWebHostEnvironment env) => _env = env;

    public IActionResult Index()
    {
        var cart = GetCartItems();
        ViewData["Title"] = "Корзина";
        return View(cart);
    }

    [HttpPost]
    public IActionResult Add(int id)
    {
        var guitar = GuitarService.GetById(id, _env);
        if (guitar == null) return RedirectToAction(nameof(ShopController.Index), "Shop");

        var cart = GetCartItems();
        var existing = cart.FirstOrDefault(i => i.Id == id);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            cart.Add(new CartItem { Id = id, Name = guitar.Name, Price = guitar.Price, Quantity = 1 });
        }
        
        SaveCartItems(cart);
        return RedirectToAction(nameof(ShopController.Index), "Shop");
    }

    [HttpPost]
    public IActionResult Remove(int id)
    {
        var cart = GetCartItems();
        var item = cart.FirstOrDefault(x => x.Id == id);
        if (item != null) cart.Remove(item);
        SaveCartItems(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Clear()
    {
        HttpContext.Session.Remove(CartSessionKey);
        return RedirectToAction(nameof(Index));
    }

    private List<CartItem> GetCartItems()
    {
        var json = HttpContext.Session.GetString(CartSessionKey);
        return json != null ? JsonSerializer.Deserialize<List<CartItem>>(json) : new List<CartItem>();
    }

    private void SaveCartItems(List<CartItem> items)
    {
        HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(items));
    }
}
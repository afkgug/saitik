using Microsoft.AspNetCore.Mvc;
using guitar_shop.Services;

namespace guitar_shop.Controllers;

public class ShopController : Controller
{
    private readonly IWebHostEnvironment _env;
    public ShopController(IWebHostEnvironment env) => _env = env;

    public IActionResult Index(string search, string category, string color, decimal minPrice = 0, decimal maxPrice = 5000)
    {
        var guitars = GuitarService.GetAll(_env);
        
        if (!string.IsNullOrWhiteSpace(search))
            guitars = guitars.Where(g => g.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (!string.IsNullOrWhiteSpace(category) && category != "Все")
            guitars = guitars.Where(g => g.Category == category).ToList();
            
        if (!string.IsNullOrWhiteSpace(color) && color != "Все")
            guitars = guitars.Where(g => g.Color == color).ToList();

        guitars = guitars.Where(g => g.Price >= minPrice && g.Price <= maxPrice).ToList();

        ViewData["Title"] = "Каталог";
        return View(guitars);
    }
}
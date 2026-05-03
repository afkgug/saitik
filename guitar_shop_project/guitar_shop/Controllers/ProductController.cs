using Microsoft.AspNetCore.Mvc;
using guitar_shop.Services;

namespace guitar_shop.Controllers;

public class ProductController : Controller
{
    private readonly IWebHostEnvironment _env;
    public ProductController(IWebHostEnvironment env) => _env = env;

    public IActionResult Index(int id)
    {
        var guitar = GuitarService.GetById(id, _env);
        if (guitar == null) return NotFound();
        ViewData["Title"] = guitar.Name;
        return View(guitar);
    }
}
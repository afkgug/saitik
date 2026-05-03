using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using guitar_shop.Data;
using guitar_shop.Models;

namespace guitar_shop.Controllers;

public class LoginController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<LoginController> _logger;

    public LoginController(AppDbContext db, ILogger<LoginController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IActionResult Index(string returnUrl = "/")
    {
        ViewData["Title"] = "Войти";
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(string username, string password, string returnUrl = "/")
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            TempData["Email"] = username;
            ViewBag.Error = "Введите email и пароль";
            return View();
        }

        // Ищем пользователя по email
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == username);
        
        if (user == null || !BCrypt.Verify(password, user.PasswordHash))
        {
            TempData["Email"] = username;
            ViewBag.Error = "Неверный email или пароль";
            return View();
        }

        if (!user.IsConfirmed)
        {
            TempData["Email"] = username;
            ViewBag.Error = "Подтвердите ваш email перед входом. Проверьте вашу почту.";
            return View();
        }

        // Устанавливаем сессию
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserName", user.FullName ?? user.Username);

        _logger.LogInformation($"Пользователь {user.Email} вошел в систему");

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
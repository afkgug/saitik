using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using guitar_shop.Data;
using guitar_shop.Models;
using BCrypt.Net;

namespace guitar_shop.Controllers;

public class RegisterController : Controller
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterController> _logger;

    public RegisterController(AppDbContext db, IEmailService emailService, ILogger<RegisterController> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Регистрация";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(string username, string email, string password, string confirmPassword, string? deliveryAddress)
    {
        // Валидация
        if (password.Length < 6)
        {
            TempData["Email"] = email;
            TempData["FullName"] = username;
            TempData["DeliveryAddress"] = deliveryAddress;
            ViewBag.Error = "Пароль должен быть не менее 6 символов";
            return View();
        }

        if (password != confirmPassword)
        {
            TempData["Email"] = email;
            TempData["FullName"] = username;
            TempData["DeliveryAddress"] = deliveryAddress;
            ViewBag.Error = "Пароли не совпадают";
            return View();
        }

        // Проверка на существующий email
        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            TempData["Email"] = email;
            TempData["FullName"] = username;
            TempData["DeliveryAddress"] = deliveryAddress;
            ViewBag.Error = "Пользователь с таким email уже существует";
            return View();
        }

        // Создаем нового пользователя
        var user = new User
        {
            Email = email,
            Username = username,
            FullName = username,
            PasswordHash = BCrypt.HashPassword(password),
            DeliveryAddress = deliveryAddress,
            IsConfirmed = false,
            ConfirmationToken = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Отправляем письмо подтверждения
        await _emailService.SendConfirmationEmailAsync(user.Email, user.FullName ?? user.Username, user.ConfirmationToken);

        ViewBag.Success = "На вашу почту отправлено письмо для подтверждения.";
        return View();
    }
}
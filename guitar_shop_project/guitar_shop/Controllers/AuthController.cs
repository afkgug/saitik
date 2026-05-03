using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using guitar_shop.Data;
using guitar_shop.Models;

namespace guitar_shop.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuthController> _logger;
    private readonly IEmailService _emailService;

    public AuthController(AppDbContext db, ILogger<AuthController> logger, IEmailService emailService)
    {
        _db = db;
        _logger = logger;
        _emailService = emailService;
    }

    // GET: /Auth/Register
    public IActionResult Register()
    {
        ViewData["Title"] = "Регистрация";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Сохраняем данные формы в TempData для восстановления при ошибке
            TempData["Email"] = model.Email;
            TempData["FullName"] = model.FullName;
            TempData["DeliveryAddress"] = model.DeliveryAddress;
            
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                ViewBag.Error = error.ErrorMessage;
                break;
            }
            return View(model);
        }

        // Проверка на существующий email
        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (existingUser != null)
        {
            TempData["Email"] = model.Email;
            TempData["FullName"] = model.FullName;
            TempData["DeliveryAddress"] = model.DeliveryAddress;
            ViewBag.Error = "Пользователь с таким email уже существует";
            return View(model);
        }

        // Создаем нового пользователя
        var user = new User
        {
            Email = model.Email,
            Username = model.FullName,
            FullName = model.FullName,
            PasswordHash = BCrypt.HashPassword(model.Password),
            DeliveryAddress = model.DeliveryAddress,
            IsConfirmed = false,
            ConfirmationToken = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Отправляем письмо подтверждения
        await _emailService.SendConfirmationEmailAsync(user.Email, user.FullName, user.ConfirmationToken);

        ViewBag.Success = "На вашу почту отправлено письмо для подтверждения.";
        return View(new RegisterViewModel());
    }

    // GET: /Auth/Login
    public IActionResult Login(string returnUrl = "/")
    {
        ViewData["Title"] = "Вход";
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = "/")
    {
        if (!ModelState.IsValid)
        {
            TempData["Email"] = model.Email;
            return View(model);
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        
        if (user == null || !BCrypt.Verify(model.Password, user.PasswordHash))
        {
            TempData["Email"] = model.Email;
            ViewBag.Error = "Неверный email или пароль";
            return View(model);
        }

        if (!user.IsConfirmed)
        {
            TempData["Email"] = model.Email;
            ViewBag.Error = "Подтвердите ваш email перед входом";
            return View(model);
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

    // GET: /Auth/Confirm?token=xxx&email=xxx
    public async Task<IActionResult> Confirm(string token, string email)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
        {
            ViewData["Title"] = "Ошибка подтверждения";
            ViewBag.Error = "Некорректная ссылка подтверждения";
            return View();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.ConfirmationToken == token);
        
        if (user == null)
        {
            ViewData["Title"] = "Ошибка подтверждения";
            ViewBag.Error = "Пользователь не найден или токен неверен. Возможно, вы уже подтвердили email.";
            return View();
        }

        if (user.IsConfirmed)
        {
            ViewData["Title"] = "Email уже подтвержден";
            ViewBag.Success = "Ваш email уже был подтвержден ранее.";
            return View();
        }

        // Подтверждаем пользователя
        user.IsConfirmed = true;
        user.ConfirmationToken = null;
        await _db.SaveChangesAsync();

        _logger.LogInformation($"Email {email} успешно подтвержден");

        ViewData["Title"] = "Подтверждение успешно";
        ViewBag.Success = "Ваш email успешно подтвержден! Теперь вы можете войти в систему.";
        return View();
    }

    // GET: /Auth/Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    // AJAX: Проверка email на уникальность
    public async Task<IActionResult> CheckEmail(string email)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        return Json(new { isUnique = !exists });
    }
}

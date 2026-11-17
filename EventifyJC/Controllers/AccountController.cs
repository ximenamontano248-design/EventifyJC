using EventifyJC.Models;
using EventifyJC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventifyJC.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<Usuario> _signInManager;
    private readonly UserManager<Usuario> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public AccountController(
        SignInManager<Usuario> signInManager,
        UserManager<Usuario> userManager,
        ApplicationDbContext context,
        IEmailService emailService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
        _emailService = emailService;
    }

    // =========================
    // LOGIN
    // =========================
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || user.Activo == false)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, isPersistent: true, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        if (user.Rol?.ToLowerInvariant() == "proveedor")
        {
            var tieneNegocio = await _context.Proveedors.AnyAsync(p => p.IdUsuario == user.IdUsuario);
            if (!tieneNegocio)
                return RedirectToAction("Create", "Proveedor");

            return RedirectToAction("Index", "Proveedor");
        }

        if (user.Rol?.ToLowerInvariant() == "organizador")
            return RedirectToAction("Index", "Organizador");

        return RedirectToAction("Index", RutaSegunRol(user.Rol)); 
    }

    // =========================
    // REGISTER
    // =========================
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Rol != "organizador" && model.Rol != "proveedor")
            ModelState.AddModelError(nameof(model.Rol), "Tipo de cuenta inválido.");

        if (!ModelState.IsValid) return View(model);

        var existente = await _userManager.FindByEmailAsync(model.Email);
        if (existente != null)
        {
            ModelState.AddModelError(nameof(model.Email), "Ya existe una cuenta con este correo.");
            return View(model);
        }

        var user = new Usuario
        {
            Nombre = $"{model.Nombre.Trim()} {model.Apellido.Trim()}".Trim(),
            Email = model.Email.Trim(),
            Telefono = model.Telefono.Trim(),
            Rol = model.Rol,
            FechaRegistro = DateTime.Now,
            Activo = true,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: true);

        if (model.Rol == "proveedor")
            return RedirectToAction("Create", "Proveedor");

        return RedirectToAction("Index", "Home");
    }

    // =========================
    // LOGOUT
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // =========================
    // MI PERFIL
    // =========================
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Perfil()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        return View(new PerfilViewModel
        {
            Nombre = user.Nombre,
            Email = user.Email ?? "",
            Telefono = user.Telefono ?? "",
            Rol = user.Rol,
            FechaRegistro = user.FechaRegistro,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Perfil(PerfilViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        user.Nombre = model.Nombre.Trim();
        user.Telefono = model.Telefono?.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        TempData["Success"] = "Perfil actualizado correctamente.";
        return RedirectToAction("Perfil");
    }

    // =========================
    // CAMBIAR CONTRASEÑA
    // =========================
    [HttpGet]
    [Authorize]
    public IActionResult CambiarContrasena()
    {
        return View(new CambiarContrasenaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        var result = await _userManager.ChangePasswordAsync(
            user, model.PasswordActual, model.PasswordNueva);

        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] = "Contraseña cambiada con éxito.";
        return RedirectToAction("Perfil");
    }

    // =========================
    // OLVIDÉ MI CONTRASEÑA
    // =========================
    [HttpGet]
    public IActionResult OlvideContrasena()
    {
        return View(new OlvideContrasenaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OlvideContrasena(OlvideContrasenaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            TempData["Success"] = "Si el correo está registrado, recibirás una contraseña temporal.";
            return RedirectToAction("Login");
        }

        var tempPassword = GenerarPasswordTemporal();

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, tempPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        try
        {
            await _emailService.SendPasswordResetEmailAsync(
                user.Email!,
                user.Nombre,
                tempPassword);

            TempData["Success"] = "¡Te enviamos una contraseña temporal a tu correo! Revisa tu bandeja (y spam).";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty,
                "No se pudo enviar el correo. Intenta más tarde.");
            Console.WriteLine($"ERROR SMTP: {ex.Message}");
            return View(model);
        }
    }

    private static string GenerarPasswordTemporal()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string RutaSegunRol(string rol) => rol?.ToLowerInvariant() switch
    {
        "admin" or "administrador" => "Admin",
        "proveedor" => "Proveedor",
        _ => "Home",
    };
}
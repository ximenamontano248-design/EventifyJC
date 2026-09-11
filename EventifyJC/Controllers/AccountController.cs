using EventifyJC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventifyJC.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<Usuario> _signInManager;
    private readonly UserManager<Usuario> _userManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        SignInManager<Usuario> signInManager,
        UserManager<Usuario> userManager,
        ApplicationDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

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

        // Si es proveedor y aún no ha registrado ningún negocio, lo mandamos a crearlo
        if (user.Rol?.ToLowerInvariant() == "proveedor")
        {
            var tieneNegocio = await _context.Proveedors.AnyAsync(p => p.IdUsuario == user.IdUsuario);
            if (!tieneNegocio)
                return RedirectToAction("Create", "Proveedor");

            return RedirectToAction("Index", "Proveedor");
        }

        return RedirectToAction("Index", RutaSegunRol(user.Rol));
    }

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

        if (!ModelState.IsValid)
            return View(model);

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

        // Si eligió "proveedor", lo llevamos directo a registrar su primer negocio
        if (model.Rol == "proveedor")
            return RedirectToAction("Create", "Proveedor");

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    private static string RutaSegunRol(string rol) => rol?.ToLowerInvariant() switch
    {
        "admin" or "administrador" => "Admin",
        "proveedor" => "Proveedor",
        _ => "Home",
    };

    [Authorize]
    public IActionResult Perfil()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int idUsuario))
        {
            return RedirectToAction("Login", "Account");
        }

        var usuario = _context.Usuarios.Find(idUsuario);
        return View(usuario);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ActualizarPerfil(Usuario model)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int idUsuario))
        {
            return RedirectToAction("Login", "Account");
        }

        var usuario = _context.Usuarios.Find(idUsuario);

        usuario.Nombre = model.Nombre;
        usuario.Telefono = model.Telefono;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Perfil actualizado correctamente";
        return RedirectToAction("Perfil");
    }

    [Authorize]
    public IActionResult CambiarContrasena()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CambiarContrasena(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int idUsuario))
        {
            return RedirectToAction("Login", "Account");
        }

        var usuario = _context.Usuarios.Find(idUsuario);

        // Verificar contraseña actual con el hash
        var passwordHasher = new PasswordHasher<Usuario>();
        var result = passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, model.PasswordActual);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "La contraseña actual es incorrecta.");
            return View(model);
        }

        // Actualizar contraseña
        usuario.PasswordHash = passwordHasher.HashPassword(usuario, model.NuevaPassword);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Contraseña actualizada correctamente";
        return RedirectToAction("Perfil");
    }
}
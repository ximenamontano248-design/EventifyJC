// Controllers/ProveedorController.cs
using EventifyJC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventifyJC.Controllers;

[Authorize]
public class ProveedorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly IWebHostEnvironment _env;

    public ProveedorController(ApplicationDbContext context, UserManager<Usuario> userManager, IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var negocios = await _context.Proveedors
            .Where(p => p.IdUsuario == user.IdUsuario)
            .ToListAsync();

        return View(negocios);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProveedorViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProveedorViewModel model)
    {
        if (!model.AceptaTratamientoDatos)
            ModelState.AddModelError(nameof(model.AceptaTratamientoDatos), "Debes autorizar el tratamiento de datos.");

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        string? logoUrl = null;

        if (model.Logo != null && model.Logo.Length > 0)
        {
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(model.Logo.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.Logo), "Solo se permiten imágenes JPG, PNG o WEBP.");
                return View(model);
            }

            if (model.Logo.Length > 3 * 1024 * 1024) // 3 MB máximo
            {
                ModelState.AddModelError(nameof(model.Logo), "El logo no puede pesar más de 3MB.");
                return View(model);
            }

            var carpetaLogos = Path.Combine(_env.WebRootPath, "uploads", "logos");
            Directory.CreateDirectory(carpetaLogos);

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaFisica = Path.Combine(carpetaLogos, nombreArchivo);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await model.Logo.CopyToAsync(stream);
            }

            logoUrl = $"/uploads/logos/{nombreArchivo}";
        }

        var proveedor = new Proveedor
        {
            IdUsuario = user.IdUsuario,
            NombreComercial = model.NombreComercial.Trim(),
            RazonSocial = model.RazonSocial.Trim(),
            Nit = model.Nit.Trim(),
            Representante = model.Representante.Trim(),
            TelefonoNegocio = model.TelefonoNegocio.Trim(),
            Ciudad = model.Ciudad,
            Direccion = model.Direccion.Trim(),
            NivelCobertura = model.NivelCobertura,
            CiudadesCubiertas = model.CiudadesCubiertas,
            Categorias = model.Categorias,
            Descripcion = model.Descripcion,
            Tarifas = model.Tarifas,
            Capacidad = model.Capacidad,
            AnticipacionRequerida = model.AnticipacionRequerida,
            LogoUrl = logoUrl,
            Galeria = model.Galeria,
            Videos = model.Videos,
            Whatsapp = model.Whatsapp,
            Instagram = model.Instagram,
            Facebook = model.Facebook,
            SitioWeb = model.SitioWeb,
            Verificado = false,
            CalificacionPromedio = 0,
        };

        _context.Proveedors.Add(proveedor);
        await _context.SaveChangesAsync();

        TempData["Success"] = "¡Negocio registrado con éxito!";
        return RedirectToAction("Index");
    }
}
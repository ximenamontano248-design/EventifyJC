// Controllers/ProveedorController.cs
using EventifyJC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventifyJC.Controllers;

[Authorize(Roles = "proveedor")]
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

    // --- Acciones para el perfil del proveedor (HU-05) ---

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        // Obtener el negocio del proveedor INCLUYENDO la categoría de cada servicio
        var negocio = await _context.Proveedors
            .Include(p => p.Servicios)
                .ThenInclude(s => s.IdCategoriaNavigation)  // ✅ ESTO FALTA: carga la categoría
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);

        if (negocio == null)
        {
            return RedirectToAction("Create");
        }

        return View(negocio);
    }

    [HttpGet]
    public IActionResult Create()
    {
        // Cargar las categorías para el formulario de creación de servicios
        ViewBag.Categorias = _context.Categoria.ToList();
        return View(new ProveedorViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProveedorViewModel model)
    {
        if (!model.AceptaTratamientoDatos)
            ModelState.AddModelError(nameof(model.AceptaTratamientoDatos), "Debes autorizar el tratamiento de datos.");

        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = _context.Categoria.ToList();
            return View(model);
        }

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
                ViewBag.Categorias = _context.Categoria.ToList();
                return View(model);
            }

            if (model.Logo.Length > 3 * 1024 * 1024) // 3 MB máximo
            {
                ModelState.AddModelError(nameof(model.Logo), "El logo no puede pesar más de 3MB.");
                ViewBag.Categorias = _context.Categoria.ToList();
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

        TempData["Success"] = "¡Negocio registrado con éxito! Ahora puedes publicar tus servicios.";
        return RedirectToAction("Index");
    }

    // --- Acciones para los servicios del proveedor (HU-06, HU-07) ---

    [HttpGet]
    public IActionResult CrearServicio()
    {
        ViewBag.Categorias = _context.Categoria.ToList();
        return View(new ServicioViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearServicio(ServicioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = _context.Categoria.ToList();
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var proveedor = await _context.Proveedors.FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);
        if (proveedor == null)
        {
            TempData["Error"] = "Primero debes registrar tu negocio.";
            return RedirectToAction("Create");
        }

        // Guardar la imagen del servicio si se subió
        string? imagenUrl = null;
        if (model.Imagen != null && model.Imagen.Length > 0)
        {
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(model.Imagen.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.Imagen), "Solo se permiten imágenes JPG, PNG o WEBP.");
                ViewBag.Categorias = _context.Categoria.ToList();
                return View(model);
            }

            if (model.Imagen.Length > 3 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(model.Imagen), "La imagen no puede pesar más de 3MB.");
                ViewBag.Categorias = _context.Categoria.ToList();
                return View(model);
            }

            var carpetaImagenes = Path.Combine(_env.WebRootPath, "uploads", "servicios");
            Directory.CreateDirectory(carpetaImagenes);

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaFisica = Path.Combine(carpetaImagenes, nombreArchivo);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await model.Imagen.CopyToAsync(stream);
            }

            imagenUrl = $"/uploads/servicios/{nombreArchivo}";
        }

        var servicio = new Servicio
        {
            IdProveedor = proveedor.IdProveedor,
            IdCategoria = model.IdCategoria,
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            PrecioBase = model.PrecioBase,
            Disponibilidad = model.Disponibilidad,
            ImagenUrl = imagenUrl
        };

        _context.Servicios.Add(servicio);
        await _context.SaveChangesAsync();

        TempData["Success"] = "¡Servicio publicado con éxito!";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> EditarServicio(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .Include(s => s.IdCategoriaNavigation)  // ✅ Cargar categoría
            .FirstOrDefaultAsync(s => s.IdServicio == id);

        if (servicio == null || servicio.IdProveedorNavigation.IdUsuario != user.IdUsuario)
        {
            TempData["Error"] = "No tienes permiso para editar este servicio.";
            return RedirectToAction("Index");
        }

        ViewBag.Categorias = _context.Categoria.ToList();
        var model = new ServicioViewModel
        {
            IdServicio = servicio.IdServicio,
            IdCategoria = servicio.IdCategoria,
            Nombre = servicio.Nombre,
            Descripcion = servicio.Descripcion,
            PrecioBase = servicio.PrecioBase,
            Disponibilidad = servicio.Disponibilidad,
            ImagenUrl = servicio.ImagenUrl
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarServicio(ServicioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = _context.Categoria.ToList();
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .FirstOrDefaultAsync(s => s.IdServicio == model.IdServicio);

        if (servicio == null || servicio.IdProveedorNavigation.IdUsuario != user.IdUsuario)
        {
            TempData["Error"] = "No tienes permiso para editar este servicio.";
            return RedirectToAction("Index");
        }

        // Actualizar datos
        servicio.IdCategoria = model.IdCategoria;
        servicio.Nombre = model.Nombre;
        servicio.Descripcion = model.Descripcion;
        servicio.PrecioBase = model.PrecioBase;
        servicio.Disponibilidad = model.Disponibilidad;

        // Actualizar imagen si se subió una nueva
        if (model.Imagen != null && model.Imagen.Length > 0)
        {
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(model.Imagen.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.Imagen), "Solo se permiten imágenes JPG, PNG o WEBP.");
                ViewBag.Categorias = _context.Categoria.ToList();
                return View(model);
            }

            if (model.Imagen.Length > 3 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(model.Imagen), "La imagen no puede pesar más de 3MB.");
                ViewBag.Categorias = _context.Categoria.ToList();
                return View(model);
            }

            var carpetaImagenes = Path.Combine(_env.WebRootPath, "uploads", "servicios");
            Directory.CreateDirectory(carpetaImagenes);

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaFisica = Path.Combine(carpetaImagenes, nombreArchivo);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await model.Imagen.CopyToAsync(stream);
            }

            // Eliminar la imagen anterior si existe
            if (!string.IsNullOrEmpty(servicio.ImagenUrl))
            {
                var rutaAnterior = Path.Combine(_env.WebRootPath, servicio.ImagenUrl.TrimStart('/'));
                if (System.IO.File.Exists(rutaAnterior))
                {
                    System.IO.File.Delete(rutaAnterior);
                }
            }

            servicio.ImagenUrl = $"/uploads/servicios/{nombreArchivo}";
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "¡Servicio actualizado con éxito!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarServicio(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .FirstOrDefaultAsync(s => s.IdServicio == id);

        if (servicio == null || servicio.IdProveedorNavigation.IdUsuario != user.IdUsuario)
        {
            TempData["Error"] = "No tienes permiso para eliminar este servicio.";
            return RedirectToAction("Index");
        }

        // Eliminar la imagen asociada
        if (!string.IsNullOrEmpty(servicio.ImagenUrl))
        {
            var rutaImagen = Path.Combine(_env.WebRootPath, servicio.ImagenUrl.TrimStart('/'));
            if (System.IO.File.Exists(rutaImagen))
            {
                System.IO.File.Delete(rutaImagen);
            }
        }

        _context.Servicios.Remove(servicio);
        await _context.SaveChangesAsync();

        TempData["Success"] = "¡Servicio eliminado con éxito!";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "proveedor")]
    public IActionResult PerfilEmpresa()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int idUsuario))
        {
            return RedirectToAction("Login", "Account");
        }

        var proveedor = _context.Proveedors.FirstOrDefault(p => p.IdUsuario == idUsuario);
        return View(proveedor);
    }

    [Authorize(Roles = "proveedor")]
    [HttpPost]
    public async Task<IActionResult> EditarPerfilEmpresa(Proveedor model)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int idUsuario))
        {
            return RedirectToAction("Login", "Account");
        }

        var proveedor = _context.Proveedors.FirstOrDefault(p => p.IdUsuario == idUsuario);

        proveedor.NombreComercial = model.NombreComercial;
        proveedor.RazonSocial = model.RazonSocial;
        proveedor.Nit = model.Nit;
        proveedor.Representante = model.Representante;
        proveedor.Descripcion = model.Descripcion;
        proveedor.Direccion = model.Direccion;
        proveedor.Ciudad = model.Ciudad;
        proveedor.TelefonoNegocio = model.TelefonoNegocio;
        proveedor.NivelCobertura = model.NivelCobertura;
        proveedor.Categorias = model.Categorias;
        proveedor.Whatsapp = model.Whatsapp;
        proveedor.Instagram = model.Instagram;
        proveedor.Facebook = model.Facebook;
        proveedor.SitioWeb = model.SitioWeb;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Información de la empresa actualizada correctamente";
        return RedirectToAction("PerfilEmpresa");
    }
}
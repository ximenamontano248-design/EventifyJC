using EventifyJC.Models;
using EventifyJC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventifyJC.Controllers;

[Authorize(Roles = "proveedor")]
public class ProveedorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IWebHostEnvironment _env;
    private readonly IBlobStorageService _blobStorage;
    private readonly IEmailService _emailService;

    public ProveedorController(
        ApplicationDbContext context,
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        IWebHostEnvironment env,
        IBlobStorageService blobStorage,
        IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _env = env;
        _blobStorage = blobStorage;
        _emailService = emailService;
    }

    // =========================================================
    // DASHBOARD
    // =========================================================
    public async Task<IActionResult> Index(string? q, string? disponibilidad)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var negocio = await _context.Proveedors
            .Include(p => p.Servicios)
                .ThenInclude(s => s.IdCategoriaNavigation)
            .Include(p => p.Cotizacions)
                .ThenInclude(c => c.IdSolicitudNavigation)
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);

        if (negocio == null) return RedirectToAction("Create");

        var servicios = negocio.Servicios.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            servicios = servicios.Where(s =>
                s.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (s.Descripcion ?? "").Contains(q, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(disponibilidad))
            servicios = servicios.Where(s => s.Disponibilidad == disponibilidad);

        ViewBag.TotalServicios = negocio.Servicios.Count;
        ViewBag.ServiciosDisponibles = negocio.Servicios.Count(s => s.Disponibilidad == "disponible");
        ViewBag.TotalCotizaciones = negocio.Cotizacions.Count;
        ViewBag.CotizacionesPend = negocio.Cotizacions.Count(c => c.Estado == "enviada");
        ViewBag.CotizacionesAcep = negocio.Cotizacions.Count(c => c.Estado == "aceptada");
        ViewBag.Q = q;
        ViewBag.Disponibilidad = disponibilidad;

        negocio.Servicios = servicios.ToList();

        return View(negocio);
    }

    // =========================================================
    // REGISTRAR NEGOCIO
    // =========================================================
    [HttpGet]
    public IActionResult Create()
    {
        var model = new ProveedorViewModel
        {
            CategoriasDisponibles = _context.Categoria.OrderBy(c => c.Nombre).ToList(),
            CiudadesDisponibles = new List<string>
            {
                "Bogotá", "Medellín", "Cali", "Barranquilla", "Cartagena",
                "Bucaramanga", "Pereira", "Manizales", "Santa Marta", "Villavicencio"
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProveedorViewModel model)
    {
        if (!model.AceptaTratamientoDatos)
            ModelState.AddModelError(nameof(model.AceptaTratamientoDatos), "Debes autorizar el tratamiento de datos.");

        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var logoUrl = await GuardarArchivo(model.Logo, "logos");

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
            EstadoVerificacion = "no_verificado"   
        };

        _context.Proveedors.Add(proveedor);
        await _context.SaveChangesAsync();

        TempData["Success"] = "¡Negocio registrado con éxito! Ahora puedes publicar tus servicios.";
        return RedirectToAction("Index");
    }

    // =========================================================
    // EDITAR PERFIL DEL NEGOCIO
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> EditarPerfil()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var negocio = await _context.Proveedors.FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);
        if (negocio == null) return RedirectToAction("Create");

        return View(new ProveedorViewModel
        {
            NombreComercial = negocio.NombreComercial ?? "",
            RazonSocial = negocio.RazonSocial ?? "",
            Nit = negocio.Nit ?? "",
            Representante = negocio.Representante ?? "",
            TelefonoNegocio = negocio.TelefonoNegocio ?? "",
            Ciudad = negocio.Ciudad ?? "",
            Direccion = negocio.Direccion ?? "",
            NivelCobertura = negocio.NivelCobertura ?? "municipal",
            CiudadesCubiertas = negocio.CiudadesCubiertas,
            Categorias = negocio.Categorias ?? "",
            Descripcion = negocio.Descripcion,
            Tarifas = negocio.Tarifas,
            Capacidad = negocio.Capacidad,
            AnticipacionRequerida = negocio.AnticipacionRequerida,
            Whatsapp = negocio.Whatsapp,
            Instagram = negocio.Instagram,
            Facebook = negocio.Facebook,
            SitioWeb = negocio.SitioWeb,
            Galeria = negocio.Galeria,
            Videos = negocio.Videos,
            AceptaTratamientoDatos = true,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPerfil(ProveedorViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var negocio = await _context.Proveedors.FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);
        if (negocio == null) return RedirectToAction("Create");

        negocio.NombreComercial = model.NombreComercial.Trim();
        negocio.RazonSocial = model.RazonSocial.Trim();
        negocio.Nit = model.Nit.Trim();
        negocio.Representante = model.Representante.Trim();
        negocio.TelefonoNegocio = model.TelefonoNegocio.Trim();
        negocio.Ciudad = model.Ciudad;
        negocio.Direccion = model.Direccion.Trim();
        negocio.NivelCobertura = model.NivelCobertura;
        negocio.CiudadesCubiertas = model.CiudadesCubiertas;
        negocio.Categorias = model.Categorias;
        negocio.Descripcion = model.Descripcion;
        negocio.Tarifas = model.Tarifas;
        negocio.Capacidad = model.Capacidad;
        negocio.AnticipacionRequerida = model.AnticipacionRequerida;
        negocio.Whatsapp = model.Whatsapp;
        negocio.Instagram = model.Instagram;
        negocio.Facebook = model.Facebook;
        negocio.SitioWeb = model.SitioWeb;
        negocio.Galeria = model.Galeria;
        negocio.Videos = model.Videos;

        if (model.Logo != null && model.Logo.Length > 0)
        {
            var nuevoLogo = await GuardarArchivo(model.Logo, "logos");
            if (nuevoLogo != null)
            {
                if (!string.IsNullOrEmpty(negocio.LogoUrl))
                    await BorrarArchivo(negocio.LogoUrl);
                negocio.LogoUrl = nuevoLogo;
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Perfil actualizado con éxito.";
        return RedirectToAction("Index");
    }

    // =========================================================
    // CREAR SERVICIO
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> CrearServicio()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var proveedor = await _context.Proveedors
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);

        if (proveedor == null) return RedirectToAction("Create");

        if (proveedor.EstadoVerificacion != "verificado")
        {
            TempData["Error"] = "Debes estar verificado para publicar servicios. Sube tus documentos.";
            return RedirectToAction("SubirDocumentos");
        }

        ViewBag.Categorias = _context.Categoria.ToList();
        return View(new ServicioViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearServicio(ServicioViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var proveedor = await _context.Proveedors
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);

        if (proveedor == null) return RedirectToAction("Create");

        if (proveedor.EstadoVerificacion != "verificado")
        {
            TempData["Error"] = "Debes estar verificado para publicar servicios.";
            return RedirectToAction("SubirDocumentos");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = _context.Categoria.ToList();
            return View(model);
        }

        var imagenUrl = await GuardarArchivo(model.Imagen, "servicios");

        var servicio = new Servicio
        {
            IdProveedor = proveedor.IdProveedor,
            IdCategoria = model.IdCategoria,
            Nombre = model.Nombre.Trim(),
            Descripcion = model.Descripcion,
            PrecioBase = model.PrecioBase,        
            Disponibilidad = model.Disponibilidad ?? "disponible",
            ImagenUrl = imagenUrl,
            Tipo = "servicio",
            CantidadMinima = 50,
            CantidadMaxima = 500,
            CantidadIncremento = 50,
        };

        _context.Servicios.Add(servicio);
        await _context.SaveChangesAsync();

        TempData["Success"] = "¡Servicio creado! Ahora configura la matriz de precios.";
        return RedirectToAction("ConfigurarMatriz", new { id = servicio.IdServicio });
    }

    // =========================================================
    // EDITAR SERVICIO
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> EditarServicio(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .FirstOrDefaultAsync(s => s.IdServicio == id);

        if (servicio == null || servicio.IdProveedorNavigation.IdUsuario != user.IdUsuario)
        {
            TempData["Error"] = "No tienes permiso para editar este servicio.";
            return RedirectToAction("Index");
        }

        ViewBag.Categorias = _context.Categoria.ToList();
        return View(new ServicioViewModel
        {
            IdServicio = servicio.IdServicio,
            IdCategoria = servicio.IdCategoria,
            Nombre = servicio.Nombre,
            Descripcion = servicio.Descripcion,
            PrecioBase = servicio.PrecioBase,
            Disponibilidad = servicio.Disponibilidad,
            ImagenUrl = servicio.ImagenUrl,
        });
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

        servicio.IdCategoria = model.IdCategoria;
        servicio.Nombre = model.Nombre.Trim();
        servicio.Descripcion = model.Descripcion;
        servicio.PrecioBase = model.PrecioBase;
        servicio.Disponibilidad = model.Disponibilidad ?? "disponible";

        if (model.Imagen != null && model.Imagen.Length > 0)
        {
            var nuevaImagen = await GuardarArchivo(model.Imagen, "servicios");
            if (nuevaImagen != null)
            {
                if (!string.IsNullOrEmpty(servicio.ImagenUrl))
                    await BorrarArchivo(servicio.ImagenUrl);
                servicio.ImagenUrl = nuevaImagen;
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Servicio actualizado.";
        return RedirectToAction("Index");
    }

    // =========================================================
    // ELIMINAR SERVICIO
    // =========================================================
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

        if (!string.IsNullOrEmpty(servicio.ImagenUrl))
            await BorrarArchivo(servicio.ImagenUrl);

        _context.Servicios.Remove(servicio);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Servicio eliminado.";
        return RedirectToAction("Index");
    }

    // =========================================================
    // CAMBIAR DISPONIBILIDAD (AJAX)
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarDisponibilidad(int id, string disponibilidad)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .FirstOrDefaultAsync(s => s.IdServicio == id);

        if (servicio == null || servicio.IdProveedorNavigation.IdUsuario != user.IdUsuario)
            return Forbid();

        servicio.Disponibilidad = disponibilidad;
        await _context.SaveChangesAsync();

        return Json(new { ok = true, disponibilidad });
    }

    // =========================================================
    // DUPLICAR SERVICIO
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DuplicarServicio(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var original = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .FirstOrDefaultAsync(s => s.IdServicio == id);

        if (original == null || original.IdProveedorNavigation.IdUsuario != user.IdUsuario)
            return Forbid();

        var copia = new Servicio
        {
            IdProveedor = original.IdProveedor,
            IdCategoria = original.IdCategoria,
            Nombre = original.Nombre + " (copia)",
            Descripcion = original.Descripcion,
            PrecioBase = original.PrecioBase,
            Disponibilidad = original.Disponibilidad,
            ImagenUrl = original.ImagenUrl,
        };

        _context.Servicios.Add(copia);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Servicio duplicado. Edítalo para ajustarlo.";
        return RedirectToAction("Index");
    }

    // =========================================================
    // VISTA PÚBLICA DEL NEGOCIO
    // =========================================================
    [AllowAnonymous]
    public async Task<IActionResult> Ver(int id)
    {
        var proveedor = await _context.Proveedors
            .Include(p => p.Servicios).ThenInclude(s => s.IdCategoriaNavigation)
            .Include(p => p.Servicios).ThenInclude(s => s.Cotizacions)
                .ThenInclude(c => c.Reserva).ThenInclude(r => r!.Calificacions)
                    .ThenInclude(c => c.IdOrganizadorNavigation)
            .Include(p => p.IdUsuarioNavigation)
            .FirstOrDefaultAsync(p => p.IdProveedor == id);

        if (proveedor == null) return NotFound();
        return View(proveedor);
    }

    // =========================================================
    // COTIZACIONES RECIBIDAS
    // =========================================================
    public async Task<IActionResult> Cotizaciones(string? estado, string? q)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var proveedor = await _context.Proveedors
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);

        if (proveedor == null)
        {
            TempData["Error"] = "Primero debes registrar tu negocio.";
            return RedirectToAction("Create");
        }

        var query = _context.Cotizacions
            .Include(c => c.IdSolicitudNavigation)
                .ThenInclude(s => s.IdOrganizadorNavigation)
            .Include(c => c.IdServicioNavigation)
            .Where(c => c.IdProveedor == proveedor.IdProveedor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(c => c.Estado == estado);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c =>
                c.IdSolicitudNavigation.DescripcionEvento!.Contains(q) ||
                c.IdSolicitudNavigation.IdOrganizadorNavigation.Nombre.Contains(q));

        var cotizaciones = await query
            .OrderByDescending(c => c.FechaEnvio)
            .ToListAsync();

        ViewBag.TotalCotizaciones = await _context.Cotizacions.CountAsync(c => c.IdProveedor == proveedor.IdProveedor);
        ViewBag.TotalEnviadas = await _context.Cotizacions.CountAsync(c => c.IdProveedor == proveedor.IdProveedor && c.Estado == "enviada");
        ViewBag.TotalAceptadas = await _context.Cotizacions.CountAsync(c => c.IdProveedor == proveedor.IdProveedor && c.Estado == "aceptada");
        ViewBag.TotalRechazadas = await _context.Cotizacions.CountAsync(c => c.IdProveedor == proveedor.IdProveedor && c.Estado == "rechazada");
        ViewBag.EstadoFiltro = estado;
        ViewBag.Q = q;

        return View(cotizaciones);
    }

    public async Task<IActionResult> DetalleCotizacion(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var cotizacion = await _context.Cotizacions
            .Include(c => c.IdSolicitudNavigation)
                .ThenInclude(s => s.IdOrganizadorNavigation)
            .Include(c => c.IdServicioNavigation)
                .ThenInclude(s => s.IdCategoriaNavigation)
            .Include(c => c.IdProveedorNavigation)
            .Include(c => c.Reserva)
            .FirstOrDefaultAsync(c => c.IdCotizacion == id);

        if (cotizacion == null) return NotFound();

        if (cotizacion.IdProveedorNavigation.IdUsuario != user.IdUsuario)
            return Forbid();

        return View(cotizacion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoCotizacion(int id, string estado)
    {
        var estadosValidos = new[] { "enviada", "aceptada", "rechazada", "expirada" };
        if (!estadosValidos.Contains(estado))
            return BadRequest();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var cotizacion = await _context.Cotizacions
            .Include(c => c.IdProveedorNavigation)
            .FirstOrDefaultAsync(c => c.IdCotizacion == id);

        if (cotizacion == null || cotizacion.IdProveedorNavigation.IdUsuario != user.IdUsuario)
            return Forbid();

        cotizacion.Estado = estado;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Cotización marcada como '{estado}'.";
        return RedirectToAction("Cotizaciones");
    }

    // =========================================================
    // OPORTUNIDADES
    // =========================================================
    public async Task<IActionResult> Oportunidades()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var proveedor = await _context.Proveedors
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);

        if (proveedor == null)
            return RedirectToAction("Create");

        var solicitudes = await _context.SolicitudCotizacions
            .Include(s => s.IdOrganizadorNavigation)
            .Where(s => s.Estado == "pendiente" || s.Estado == "en_proceso")
            .Where(s => !s.Cotizacions.Any(c => c.IdProveedor == proveedor.IdProveedor))
            .OrderByDescending(s => s.FechaSolicitud)
            .Take(50)
            .ToListAsync();

        ViewBag.ProveedorId = proveedor.IdProveedor;
        return View(solicitudes);
    }

  
    [HttpGet]
    public async Task<IActionResult> ResponderSolicitud(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var proveedor = await _context.Proveedors
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);
        if (proveedor == null) return RedirectToAction("Create");

        var solicitud = await _context.SolicitudCotizacions
            .Include(s => s.IdOrganizadorNavigation)
            .Include(s => s.IdServicioNavigation).ThenInclude(sv => sv.IdCategoriaNavigation)
            .FirstOrDefaultAsync(s => s.IdSolicitud == id);

        if (solicitud == null) return NotFound();
        return View(solicitud);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AceptarSolicitud(int idSolicitud)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var proveedor = await _context.Proveedors
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);
        if (proveedor == null) return Forbid();

        var solicitud = await _context.SolicitudCotizacions
            .Include(s => s.IdServicioNavigation)
            .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);
        if (solicitud == null || solicitud.IdServicio == null) return NotFound();

        var yaCotizo = await _context.Cotizacions.AnyAsync(c =>
            c.IdSolicitud == idSolicitud && c.IdProveedor == proveedor.IdProveedor);
        if (yaCotizo)
        {
            TempData["Error"] = "Ya enviaste una cotización para esta solicitud.";
            return RedirectToAction("Oportunidades");
        }

        var cotizacion = new Cotizacion
        {
            IdSolicitud = solicitud.IdSolicitud,
            IdProveedor = proveedor.IdProveedor,
            IdServicio = solicitud.IdServicio.Value,
            Monto = solicitud.PresupuestoEstimado ?? solicitud.IdServicioNavigation.PrecioBase,
            Estado = "enviada",
            FechaEnvio = DateTime.Now,
            Mensaje = "Cotización generada desde Oportunidades."
        };
        _context.Cotizacions.Add(cotizacion);
        solicitud.Estado = "en_proceso";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Cotización enviada al organizador.";
        return RedirectToAction("Cotizaciones");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RechazarSolicitud(int idSolicitud)
    {
        TempData["Success"] = "Solicitud rechazada. No se envió cotización.";
        return RedirectToAction("Oportunidades");
    }

    // =========================================================
    // ELIMINAR / INHABILITAR EMPRESA
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarEmpresa()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var proveedor = await _context.Proveedors
            .Include(p => p.Servicios)
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);

        if (proveedor == null)
        {
            TempData["Error"] = "No tienes una empresa registrada.";
            return RedirectToAction("Index");
        }

        var tieneCotizaciones = await _context.Cotizacions
            .AnyAsync(c => c.IdProveedor == proveedor.IdProveedor);

        if (tieneCotizaciones)
        {
            TempData["Error"] = "No puedes eliminar tu empresa porque tienes cotizaciones o servicios asociados. " +
                                "Contacta al administrador si necesitas eliminarla.";
            return RedirectToAction("Index");
        }

        if (!string.IsNullOrEmpty(proveedor.LogoUrl))
            await BorrarArchivo(proveedor.LogoUrl);

        var servicios = await _context.Servicios
            .Include(s => s.MatrizPrecios)
            .Include(s => s.Disponibilidades)
            .Where(s => s.IdProveedor == proveedor.IdProveedor)
            .ToListAsync();

        foreach (var s in servicios)
        {
            _context.MatrizPrecios.RemoveRange(s.MatrizPrecios);
            _context.Disponibilidads.RemoveRange(s.Disponibilidades);
        }
        _context.Servicios.RemoveRange(servicios);

        _context.DocumentoVerificacions.RemoveRange(
            await _context.DocumentoVerificacions
                .Where(d => d.IdProveedor == proveedor.IdProveedor)
                .ToListAsync());

        _context.Proveedors.Remove(proveedor);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Tu empresa fue eliminada. Puedes volver a registrarla cuando quieras.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InhabilitarEmpresa()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var proveedor = await _context.Proveedors
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);

        if (proveedor == null)
        {
            TempData["Error"] = "No tienes una empresa registrada.";
            return RedirectToAction("Index");
        }

        user.Activo = false;
        await _userManager.UpdateAsync(user);

        var servicios = await _context.Servicios
            .Where(s => s.IdProveedor == proveedor.IdProveedor)
            .ToListAsync();

        foreach (var s in servicios)
            s.Disponibilidad = "no disponible";

        await _context.SaveChangesAsync();
        await _signInManager.SignOutAsync();

        TempData["Success"] = "Tu empresa fue inhabilitada. Tus servicios ya no aparecerán en el marketplace. " +
                              "Contacta al administrador para reactivarla.";
        return RedirectToAction("Index", "Home");
    }

    // =========================================================
    // SUBIR DOCUMENTOS DE VERIFICACIÓN
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> SubirDocumentos()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var proveedor = await _context.Proveedors
            .Include(p => p.DocumentosVerificacion)
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);

        if (proveedor == null) return RedirectToAction("Create");

        return View(proveedor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirDocumento(DocumentoVerificacionViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var proveedor = await _context.Proveedors
            .FirstOrDefaultAsync(p => p.IdUsuario == user.IdUsuario);

        if (proveedor == null) return RedirectToAction("Create");

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Revisa los datos del formulario.";
            return RedirectToAction("SubirDocumentos");
        }

        var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        var ext = Path.GetExtension(model.Archivo.FileName).ToLowerInvariant();
        if (!extensionesPermitidas.Contains(ext))
        {
            TempData["Error"] = "Solo se permiten archivos PDF, JPG o PNG.";
            return RedirectToAction("SubirDocumentos");
        }

        if (model.Archivo.Length > 5 * 1024 * 1024)
        {
            TempData["Error"] = "El archivo no puede pesar más de 5MB.";
            return RedirectToAction("SubirDocumentos");
        }

        string urlBlob;
        try
        {
            using var stream = model.Archivo.OpenReadStream();
            urlBlob = await _blobStorage.SubirArchivoAsync(
                stream,
                model.Archivo.FileName,
                model.Archivo.ContentType,
                "documentos"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR subiendo a Azure: {ex.Message}");
            TempData["Error"] = "No se pudo subir el archivo. Intenta de nuevo.";
            return RedirectToAction("SubirDocumentos");
        }

        _context.DocumentoVerificacions.Add(new DocumentoVerificacion
        {
            IdProveedor = proveedor.IdProveedor,
            Tipo = model.Tipo,
            NombreArchivo = model.Archivo.FileName,
            UrlArchivo = urlBlob,
            FechaSubida = DateTime.Now,
            Estado = "pendiente"
        });

        if (proveedor.EstadoVerificacion == "no_verificado" || proveedor.EstadoVerificacion == "rechazado")
        {
            proveedor.EstadoVerificacion = "en_revision";
            proveedor.FechaSolicitudVerificacion = DateTime.Now;
            proveedor.MotivoRechazo = null;
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Documento subido. El administrador lo revisará pronto.";
        return RedirectToAction("SubirDocumentos");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarDocumento(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doc = await _context.DocumentoVerificacions
            .Include(d => d.IdProveedorNavigation)
            .FirstOrDefaultAsync(d => d.IdDocumento == id);

        if (doc == null || doc.IdProveedorNavigation.IdUsuario != user.IdUsuario)
            return Forbid();

        if (doc.Estado == "aprobado")
        {
            TempData["Error"] = "No puedes eliminar un documento ya aprobado.";
            return RedirectToAction("SubirDocumentos");
        }

        await _blobStorage.EliminarArchivoAsync(doc.UrlArchivo);

        _context.DocumentoVerificacions.Remove(doc);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Documento eliminado.";
        return RedirectToAction("SubirDocumentos");
    }

    // =========================================================
    // CONFIGURAR MATRIZ DE PRECIOS
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> ConfigurarMatriz(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .Include(s => s.MatrizPrecios)
            .FirstOrDefaultAsync(s => s.IdServicio == id);

        if (servicio == null || servicio.IdProveedorNavigation.IdUsuario != user.IdUsuario)
        {
            TempData["Error"] = "No tienes permiso para editar este servicio.";
            return RedirectToAction("Index");
        }

        var model = new MatrizPrecioViewModel
        {
            IdServicio = servicio.IdServicio,
            ServicioNombre = servicio.Nombre,
            Tipo = servicio.Tipo,
            CantidadMinima = servicio.CantidadMinima ?? 50,
            CantidadMaxima = servicio.CantidadMaxima ?? 500,
            CantidadIncremento = servicio.CantidadIncremento ?? 50,
            CapacidadSimultanea = servicio.CapacidadSimultanea,
            BloquesHoras = "4,6,8,24"
        };

        foreach (var m in servicio.MatrizPrecios)
        {
            if (!model.Precios.ContainsKey(m.Cantidad))
                model.Precios[m.Cantidad] = new Dictionary<int, decimal>();
            model.Precios[m.Cantidad][m.Horas] = m.Precio;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarMatriz(
        int IdServicio, string Tipo, int CantidadMinima, int CantidadMaxima,
        int CantidadIncremento, string BloquesHoras, int? CapacidadSimultanea,
        Dictionary<int, Dictionary<int, decimal>> Precios)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .FirstOrDefaultAsync(s => s.IdServicio == IdServicio);

        if (servicio == null || servicio.IdProveedorNavigation.IdUsuario != user.IdUsuario)
            return Forbid();

        if (CantidadMaxima < CantidadMinima)
        {
            TempData["Error"] = "La cantidad máxima debe ser mayor o igual a la mínima.";
            return RedirectToAction("ConfigurarMatriz", new { id = IdServicio });
        }

        servicio.Tipo = Tipo;
        servicio.CantidadMinima = CantidadMinima;
        servicio.CantidadMaxima = CantidadMaxima;
        servicio.CantidadIncremento = CantidadIncremento;
        servicio.CapacidadSimultanea = CapacidadSimultanea;

        var existentes = await _context.MatrizPrecios
            .Where(m => m.IdServicio == IdServicio)
            .ToListAsync();
        _context.MatrizPrecios.RemoveRange(existentes);

        if (Precios != null)
        {
            foreach (var kvp in Precios)
            {
                foreach (var hkv in kvp.Value)
                {
                    if (hkv.Value > 0)
                    {
                        _context.MatrizPrecios.Add(new MatrizPrecio
                        {
                            IdServicio = IdServicio,
                            Cantidad = kvp.Key,
                            Horas = hkv.Key,
                            Precio = hkv.Value
                        });
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Matriz de precios guardada correctamente.";
        return RedirectToAction("Index");
    }

    // =========================================================
    // HELPERS
    // =========================================================
    private async Task<string?> GuardarArchivo(IFormFile? file, string carpeta)
    {
        if (file == null || file.Length == 0) return null;

        var extensiones = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!extensiones.Contains(ext)) return null;
        if (file.Length > 3 * 1024 * 1024) return null;

        try
        {
            using var stream = file.OpenReadStream();
            return await _blobStorage.SubirArchivoAsync(stream, file.FileName, file.ContentType, carpeta);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR subiendo archivo a Azure: {ex.Message}");
            return null;
        }
    }

    private async Task BorrarArchivo(string url)
    {
        if (string.IsNullOrEmpty(url)) return;
        await _blobStorage.EliminarArchivoAsync(url);
    }

    // =========================================================
    // CALENDARIO DE DISPONIBILIDAD
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> Calendario(int id, int? año, int? mes)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .Include(s => s.Disponibilidades)
            .FirstOrDefaultAsync(s => s.IdServicio == id);

        if (servicio == null || servicio.IdProveedorNavigation.IdUsuario != user.IdUsuario)
        {
            TempData["Error"] = "No tienes permiso para editar este servicio.";
            return RedirectToAction("Index");
        }

        var añoActual = año ?? DateTime.Now.Year;
        var mesActual = mes ?? DateTime.Now.Month;

        var fechasOcupadas = servicio.Disponibilidades
            .Where(d => d.Fecha.Year == añoActual && d.Fecha.Month == mesActual && !d.Disponible)
            .Select(d => d.Fecha)
            .ToList();

        var fechasReservadas = await _context.Reservas
            .Include(r => r.IdCotizacionNavigation)
                .ThenInclude(c => c.IdSolicitudNavigation)
            .Where(r => r.IdCotizacionNavigation.IdServicio == id
                     && r.Estado != "cancelada"
                     && r.IdCotizacionNavigation.IdSolicitudNavigation.FechaEvento.Year == añoActual
                     && r.IdCotizacionNavigation.IdSolicitudNavigation.FechaEvento.Month == mesActual)
            .Select(r => r.IdCotizacionNavigation.IdSolicitudNavigation.FechaEvento)
            .ToListAsync();

        var model = new CalendarioDisponibilidadViewModel
        {
            IdServicio = servicio.IdServicio,
            ServicioNombre = servicio.Nombre,
            Año = añoActual,
            Mes = mesActual,
            FechasOcupadas = fechasOcupadas,
            FechasReservadas = fechasReservadas
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarOcupado(int idServicio, string fecha, string? motivo)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .FirstOrDefaultAsync(s => s.IdServicio == idServicio);

        if (servicio == null || servicio.IdProveedorNavigation.IdUsuario != user.IdUsuario)
            return Forbid();

        if (!DateOnly.TryParse(fecha, out var fechaParsed))
        {
            TempData["Error"] = "Fecha inválida.";
            return RedirectToAction("Calendario", new { id = idServicio });
        }

        var existente = await _context.Disponibilidads
            .FirstOrDefaultAsync(d => d.IdServicio == idServicio && d.Fecha == fechaParsed);

        if (existente != null)
        {
            existente.Disponible = false;
            existente.Motivo = motivo;
        }
        else
        {
            _context.Disponibilidads.Add(new Disponibilidad
            {
                IdServicio = idServicio,
                Fecha = fechaParsed,
                Disponible = false,
                Motivo = motivo
            });
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Fecha {fechaParsed:dd/MM/yyyy} marcada como no disponible.";
        return RedirectToAction("Calendario", new { id = idServicio, año = fechaParsed.Year, mes = fechaParsed.Month });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarDisponible(int idServicio, string fecha)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .FirstOrDefaultAsync(s => s.IdServicio == idServicio);

        if (servicio == null || servicio.IdProveedorNavigation.IdUsuario != user.IdUsuario)
            return Forbid();

        if (!DateOnly.TryParse(fecha, out var fechaParsed))
        {
            TempData["Error"] = "Fecha inválida.";
            return RedirectToAction("Calendario", new { id = idServicio });
        }

        var existente = await _context.Disponibilidads
            .FirstOrDefaultAsync(d => d.IdServicio == idServicio && d.Fecha == fechaParsed);

        if (existente != null)
        {
            _context.Disponibilidads.Remove(existente);
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = $"Fecha {fechaParsed:dd/MM/yyyy} marcada como disponible.";
        return RedirectToAction("Calendario", new { id = idServicio, año = fechaParsed.Year, mes = fechaParsed.Month });
    }
}
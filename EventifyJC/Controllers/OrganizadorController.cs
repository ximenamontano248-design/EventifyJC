using EventifyJC.Models;
using EventifyJC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventifyJC.Controllers;

[Authorize(Roles = "organizador")]
public class OrganizadorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly IEmailService _emailService;

    public OrganizadorController(
        ApplicationDbContext context,
        UserManager<Usuario> userManager,
        IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
    }

    // =========================================================
    // DASHBOARD
    // =========================================================
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var solicitudes = await _context.SolicitudCotizacions
            .Where(s => s.IdOrganizador == user.IdUsuario)
            .ToListAsync();

        var solicitudIds = solicitudes.Select(s => s.IdSolicitud).ToList();

        var cotizaciones = await _context.Cotizacions
            .Where(c => solicitudIds.Contains(c.IdSolicitud))
            .ToListAsync();

        var cotizacionIds = cotizaciones.Select(c => c.IdCotizacion).ToList();

        var reservas = await _context.Reservas
            .Where(r => cotizacionIds.Contains(r.IdCotizacion))
            .ToListAsync();

        ViewBag.TotalSolicitudes = solicitudes.Count;
        ViewBag.SolicitudesAbiertas = solicitudes.Count(s => s.Estado == "pendiente" || s.Estado == "en_proceso");
        ViewBag.TotalCotizaciones = cotizaciones.Count;
        ViewBag.CotizacionesNuevas = cotizaciones.Count(c => c.Estado == "enviada");
        ViewBag.TotalReservas = reservas.Count;
        ViewBag.TotalGastado = reservas.Sum(r => r.MontoFinal);

        var ultimasSolicitudes = await _context.SolicitudCotizacions
            .Where(s => s.IdOrganizador == user.IdUsuario)
            .OrderByDescending(s => s.FechaSolicitud)
            .Take(5)
            .ToListAsync();

        var ultimasCotizaciones = await _context.Cotizacions
            .Include(c => c.IdProveedorNavigation)
            .Include(c => c.IdServicioNavigation)
            .Where(c => solicitudIds.Contains(c.IdSolicitud))
            .OrderByDescending(c => c.FechaEnvio)
            .Take(5)
            .ToListAsync();

        ViewBag.UltimasSolicitudes = ultimasSolicitudes;
        ViewBag.UltimasCotizaciones = ultimasCotizaciones;

        return View();
    }

    // =========================================================
    // MIS SOLICITUDES
    // =========================================================
    public async Task<IActionResult> MisSolicitudes(string? estado)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var query = _context.SolicitudCotizacions
            .Include(s => s.Cotizacions)
            .Where(s => s.IdOrganizador == user.IdUsuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(s => s.Estado == estado);

        var solicitudes = await query
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync();

        ViewBag.EstadoFiltro = estado;
        ViewBag.Total = await query.CountAsync();

        return View(solicitudes);
    }

    // =========================================================
    // DETALLE DE UNA SOLICITUD
    // =========================================================
    public async Task<IActionResult> DetalleSolicitud(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var solicitud = await _context.SolicitudCotizacions
            .Include(s => s.IdOrganizadorNavigation)
            .Include(s => s.Cotizacions).ThenInclude(c => c.IdProveedorNavigation)
            .Include(s => s.Cotizacions).ThenInclude(c => c.IdServicioNavigation).ThenInclude(sv => sv.IdCategoriaNavigation)
            .Include(s => s.Cotizacions).ThenInclude(c => c.Reserva)
            .FirstOrDefaultAsync(s => s.IdSolicitud == id);

        if (solicitud == null) return NotFound();
        if (solicitud.IdOrganizador != user.IdUsuario) return Forbid();

        return View(solicitud);
    }

    // =========================================================
    // MIS COTIZACIONES
    // =========================================================
    public async Task<IActionResult> MisCotizaciones(string? estado)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var solicitudIds = await _context.SolicitudCotizacions
            .Where(s => s.IdOrganizador == user.IdUsuario)
            .Select(s => s.IdSolicitud)
            .ToListAsync();

        var query = _context.Cotizacions
            .Include(c => c.IdProveedorNavigation)
            .Include(c => c.IdServicioNavigation).ThenInclude(s => s.IdCategoriaNavigation)
            .Include(c => c.IdSolicitudNavigation)
            .Include(c => c.Reserva)
            .Where(c => solicitudIds.Contains(c.IdSolicitud))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(c => c.Estado == estado);

        var cotizaciones = await query
            .OrderByDescending(c => c.FechaEnvio)
            .ToListAsync();

        ViewBag.EstadoFiltro = estado;
        return View(cotizaciones);
    }

    // =========================================================
    // MIS RESERVAS
    // =========================================================
    public async Task<IActionResult> MisReservas()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var solicitudIds = await _context.SolicitudCotizacions
            .Where(s => s.IdOrganizador == user.IdUsuario)
            .Select(s => s.IdSolicitud)
            .ToListAsync();

        var reservas = await _context.Reservas
            .Include(r => r.IdCotizacionNavigation).ThenInclude(c => c.IdProveedorNavigation)
            .Include(r => r.IdCotizacionNavigation).ThenInclude(c => c.IdServicioNavigation)
            .Include(r => r.IdCotizacionNavigation).ThenInclude(c => c.IdSolicitudNavigation)
            .Include(r => r.Pagos)
            .Include(r => r.Calificacions)
            .Where(r => solicitudIds.Contains(r.IdCotizacionNavigation.IdSolicitud))
            .OrderByDescending(r => r.FechaConfirmacion)
            .ToListAsync();

        return View(reservas);
    }

    // =========================================================
    // CREAR SOLICITUD DE COTIZACIÓN
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> CrearSolicitud(int? servicioId)
    {
        var model = new SolicitudCotizacionViewModel
        {
            FechaEvento = DateOnly.FromDateTime(DateTime.Now.AddDays(30))
        };

        if (servicioId.HasValue)
        {
            var servicio = await _context.Servicios
                .Include(s => s.IdProveedorNavigation)
                .FirstOrDefaultAsync(s => s.IdServicio == servicioId.Value);

            if (servicio != null)
            {
                model.ServicioId = servicio.IdServicio;
                model.ProveedorNombre = servicio.IdProveedorNavigation?.NombreComercial;
                model.ServicioNombre = servicio.Nombre;
            }
        }

        ViewBag.Servicios = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .OrderBy(s => s.Nombre)
            .Take(100)
            .ToListAsync();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearSolicitud(SolicitudCotizacionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Servicios = await _context.Servicios
                .Include(s => s.IdProveedorNavigation)
                .OrderBy(s => s.Nombre)
                .Take(100)
                .ToListAsync();
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var solicitud = new SolicitudCotizacion
        {
            IdOrganizador = user.IdUsuario,
            FechaSolicitud = DateTime.Now,
            FechaEvento = model.FechaEvento,
            DescripcionEvento = model.DescripcionEvento,
            PresupuestoEstimado = model.PresupuestoEstimado,
            Estado = "pendiente",
            IdServicio = model.ServicioId         
        };

        _context.SolicitudCotizacions.Add(solicitud);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Solicitud creada. Los proveedores podrán verla y enviarte cotizaciones.";
        return RedirectToAction("DetalleSolicitud", new { id = solicitud.IdSolicitud });
    }

    // =========================================================
    // COTIZAR UN SERVICIO ESPECÍFICO (POST)
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CotizarServicio(
        CotizarServicioViewModel model,
        int? CantidadSeleccionada,
        int? HorasSeleccionadas)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation).ThenInclude(p => p!.IdUsuarioNavigation)
            .Include(s => s.IdCategoriaNavigation)
            .Include(s => s.MatrizPrecios)
            .Include(s => s.Disponibilidades)
            .FirstOrDefaultAsync(s => s.IdServicio == model.ServicioId);

        if (servicio == null)
        {
            ModelState.AddModelError(string.Empty, "El servicio ya no está disponible.");
            return View(model);
        }

        var fechaOcupada = servicio.Disponibilidades
            .Any(d => !d.Disponible && d.Fecha == model.FechaEvento);

        if (fechaOcupada)
        {
            ModelState.AddModelError(nameof(model.FechaEvento),
                "El proveedor no tiene disponible esa fecha. Por favor elige otra.");
            return View(model);
        }

        if (model.FechaEvento < DateOnly.FromDateTime(DateTime.Now))
        {
            ModelState.AddModelError(nameof(model.FechaEvento),
                "La fecha del evento no puede ser en el pasado.");
            return View(model);
        }

        decimal presupuestoCalculado = servicio.PrecioBase;

        if (CantidadSeleccionada.HasValue && HorasSeleccionadas.HasValue)
        {
            var itemMatriz = servicio.MatrizPrecios
                .FirstOrDefault(m => m.Cantidad == CantidadSeleccionada.Value
                                  && m.Horas == HorasSeleccionadas.Value);

            if (itemMatriz != null && itemMatriz.Precio > 0)
            {
                presupuestoCalculado = itemMatriz.Precio;
            }
        }

        var descripcionCompleta = model.DescripcionEvento;

        if (CantidadSeleccionada.HasValue || HorasSeleccionadas.HasValue)
        {
            descripcionCompleta += "\n\n";
            if (CantidadSeleccionada.HasValue)
                descripcionCompleta += $"| Cantidad: {CantidadSeleccionada} unidades/personas";
            if (HorasSeleccionadas.HasValue)
                descripcionCompleta += $" | Horas: {HorasSeleccionadas} horas";
        }

        var solicitud = new SolicitudCotizacion
        {
            IdOrganizador = user.IdUsuario,
            FechaSolicitud = DateTime.Now,
            FechaEvento = model.FechaEvento,
            DescripcionEvento = descripcionCompleta,
            PresupuestoEstimado = presupuestoCalculado,
            Estado = "pendiente",
            IdServicio = model.ServicioId
        };

        _context.SolicitudCotizacions.Add(solicitud);
        await _context.SaveChangesAsync();

        try
        {
            var proveedorUsuario = servicio.IdProveedorNavigation?.IdUsuarioNavigation;

            if (proveedorUsuario != null && !string.IsNullOrEmpty(proveedorUsuario.Email))
            {
                await _emailService.SendNuevaCotizacionEmailAsync(
                    toEmail: proveedorUsuario.Email,
                    toName: proveedorUsuario.Nombre,
                    organizadorNombre: user.Nombre,
                    organizadorEmail: user.Email ?? "",
                    organizadorTelefono: user.Telefono ?? "",
                    servicioNombre: servicio.Nombre,
                    categoriaNombre: servicio.IdCategoriaNavigation?.Nombre ?? "",
                    fechaEvento: model.FechaEvento,
                    descripcionEvento: model.DescripcionEvento,
                    presupuestoEstimado: presupuestoCalculado,
                    precioBase: servicio.PrecioBase
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR enviando correo al proveedor: {ex.Message}");
        }

        TempData["Success"] = "¡Solicitud enviada! El proveedor podrá aceptarla o rechazarla.";
        return RedirectToAction("DetalleSolicitud", new { id = solicitud.IdSolicitud });
    }

    // =========================================================
    // ACEPTAR / RECHAZAR COTIZACIÓN
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AceptarCotizacion(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var cotizacion = await _context.Cotizacions
            .Include(c => c.IdSolicitudNavigation)
            .FirstOrDefaultAsync(c => c.IdCotizacion == id);

        if (cotizacion == null) return NotFound();
        if (cotizacion.IdSolicitudNavigation.IdOrganizador != user.IdUsuario) return Forbid();

        cotizacion.Estado = "aceptada";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Cotización aceptada. Ahora puedes confirmar la reserva.";
        return RedirectToAction("DetalleSolicitud", new { id = cotizacion.IdSolicitud });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RechazarCotizacion(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var cotizacion = await _context.Cotizacions
            .Include(c => c.IdSolicitudNavigation)
            .FirstOrDefaultAsync(c => c.IdCotizacion == id);

        if (cotizacion == null) return NotFound();
        if (cotizacion.IdSolicitudNavigation.IdOrganizador != user.IdUsuario) return Forbid();

        cotizacion.Estado = "rechazada";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Cotización rechazada.";
        return RedirectToAction("DetalleSolicitud", new { id = cotizacion.IdSolicitud });
    }

    // =========================================================
    // CONFIRMAR RESERVA
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarReserva(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var cotizacion = await _context.Cotizacions
            .Include(c => c.IdSolicitudNavigation)
            .Include(c => c.Reserva)
            .FirstOrDefaultAsync(c => c.IdCotizacion == id);

        if (cotizacion == null) return NotFound();
        if (cotizacion.IdSolicitudNavigation.IdOrganizador != user.IdUsuario) return Forbid();

        if (cotizacion.Estado != "aceptada")
        {
            TempData["Error"] = "Solo puedes confirmar cotizaciones aceptadas.";
            return RedirectToAction("DetalleSolicitud", new { id = cotizacion.IdSolicitud });
        }

        if (cotizacion.Reserva != null)
        {
            TempData["Error"] = "Esta cotización ya tiene una reserva asociada.";
            return RedirectToAction("MisReservas");
        }

        var reserva = new Reserva
        {
            IdCotizacion = cotizacion.IdCotizacion,
            Estado = "confirmada",
            FechaConfirmacion = DateTime.Now,
            MontoFinal = cotizacion.Monto,
            ComisionEventify = Math.Round(cotizacion.Monto * 0.10m, 2)
        };

        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Reserva confirmada. Puedes simular el pago desde 'Mis Reservas'.";
        return RedirectToAction("MisReservas");
    }

    // =========================================================
    // SIMULAR PAGO
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SimularPago(int idReserva, string metodo)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var reserva = await _context.Reservas
            .Include(r => r.IdCotizacionNavigation).ThenInclude(c => c.IdSolicitudNavigation)
            .FirstOrDefaultAsync(r => r.IdReserva == idReserva);

        if (reserva == null) return NotFound();
        if (reserva.IdCotizacionNavigation.IdSolicitudNavigation.IdOrganizador != user.IdUsuario) return Forbid();

        var pago = new Pago
        {
            IdReserva = reserva.IdReserva,
            Monto = reserva.MontoFinal,
            Metodo = string.IsNullOrWhiteSpace(metodo) ? "tarjeta_credito" : metodo,
            Estado = "completado",
            FechaPago = DateTime.Now,
            IdTransaccionExterna = "TXN-" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper()
        };

        _context.Pagos.Add(pago);
        reserva.Estado = "finalizada";
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Pago simulado con éxito. Transacción: {pago.IdTransaccionExterna}";
        return RedirectToAction("MisReservas");
    }

    // =========================================================
    // CALIFICAR SERVICIO (GET)
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> Calificar(int idReserva)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var reserva = await _context.Reservas
            .Include(r => r.IdCotizacionNavigation).ThenInclude(c => c.IdSolicitudNavigation)
            .Include(r => r.IdCotizacionNavigation).ThenInclude(c => c.IdServicioNavigation).ThenInclude(s => s.IdProveedorNavigation)
            .Include(r => r.Calificacions)
            .FirstOrDefaultAsync(r => r.IdReserva == idReserva);

        if (reserva == null) return NotFound();

        if (reserva.IdCotizacionNavigation.IdSolicitudNavigation.IdOrganizador != user.IdUsuario)
            return Forbid();

        if (reserva.Estado != "finalizada")
        {
            TempData["Error"] = "Solo puedes calificar eventos finalizados y pagados.";
            return RedirectToAction("MisReservas");
        }

        var fechaEvento = reserva.IdCotizacionNavigation.IdSolicitudNavigation.FechaEvento;
        if (fechaEvento > DateOnly.FromDateTime(DateTime.Now))
        {
            TempData["Error"] = "Aún no puedes calificar. El evento no ha ocurrido.";
            return RedirectToAction("MisReservas");
        }

        var calificacionExistente = reserva.Calificacions?.FirstOrDefault();

        var model = new CalificacionViewModel
        {
            IdReserva = idReserva,
            ServicioNombre = reserva.IdCotizacionNavigation.IdServicioNavigation?.Nombre,
            ProveedorNombre = reserva.IdCotizacionNavigation.IdServicioNavigation?.IdProveedorNavigation?.NombreComercial,
            FechaEvento = fechaEvento,
            Puntuacion = calificacionExistente?.Puntuacion ?? 0,
            Comentario = calificacionExistente?.Comentario
        };

        ViewBag.EsEdicion = calificacionExistente != null;

        return View(model);
    }

    // =========================================================
    // CALIFICAR SERVICIO (POST)
    // =========================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calificar(CalificacionViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var reserva = await _context.Reservas
            .Include(r => r.IdCotizacionNavigation).ThenInclude(c => c.IdSolicitudNavigation)
            .Include(r => r.IdCotizacionNavigation).ThenInclude(c => c.IdServicioNavigation)
            .Include(r => r.Calificacions)
            .FirstOrDefaultAsync(r => r.IdReserva == model.IdReserva);

        if (reserva == null) return NotFound();

        if (reserva.IdCotizacionNavigation.IdSolicitudNavigation.IdOrganizador != user.IdUsuario)
            return Forbid();

        if (reserva.Estado != "finalizada")
        {
            TempData["Error"] = "Solo puedes calificar eventos finalizados.";
            return RedirectToAction("MisReservas");
        }

        var fechaEvento = reserva.IdCotizacionNavigation.IdSolicitudNavigation.FechaEvento;
        if (fechaEvento > DateOnly.FromDateTime(DateTime.Now))
        {
            TempData["Error"] = "El evento aún no ha ocurrido.";
            return RedirectToAction("MisReservas");
        }

        if (!ModelState.IsValid)
        {
            model.ServicioNombre = reserva.IdCotizacionNavigation.IdServicioNavigation?.Nombre;
            model.ProveedorNombre = reserva.IdCotizacionNavigation.IdServicioNavigation?.IdProveedorNavigation?.NombreComercial;
            model.FechaEvento = fechaEvento;
            ViewBag.EsEdicion = reserva.Calificacions?.Any() == true;
            return View(model);
        }

        var calificacionExistente = reserva.Calificacions?.FirstOrDefault();

        if (calificacionExistente != null)
        {
            calificacionExistente.Puntuacion = model.Puntuacion;
            calificacionExistente.Comentario = model.Comentario;
            calificacionExistente.Fecha = DateTime.Now;
            TempData["Success"] = "¡Calificación actualizada!";
        }
        else
        {
            _context.Calificacions.Add(new Calificacion
            {
                IdReserva = model.IdReserva,
                IdOrganizador = user.IdUsuario,
                Puntuacion = model.Puntuacion,
                Comentario = model.Comentario,
                Fecha = DateTime.Now
            });
            TempData["Success"] = "¡Gracias por tu calificación!";
        }

        await _context.SaveChangesAsync();

        var idProveedor = await _context.Cotizacions
            .Where(c => c.IdCotizacion == reserva.IdCotizacion)
            .Select(c => c.IdProveedor)
            .FirstOrDefaultAsync();

        var promedio = await _context.Calificacions
            .Where(c => c.IdReservaNavigation.IdCotizacionNavigation.IdProveedor == idProveedor)
            .AverageAsync(c => (decimal?)c.Puntuacion) ?? 0;

        var proveedor = await _context.Proveedors.FindAsync(idProveedor);
        if (proveedor != null)
        {
            proveedor.CalificacionPromedio = Math.Round(promedio, 2);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("MisReservas");
    }

    // =========================================================
    // COTIZAR UN SERVICIO ESPECÍFICO (GET)
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> CotizarServicio(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var servicio = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .Include(s => s.IdCategoriaNavigation)
            .Include(s => s.MatrizPrecios)
            .Include(s => s.Disponibilidades)
            .FirstOrDefaultAsync(s => s.IdServicio == id);

        if (servicio == null) return NotFound();

        var model = new CotizarServicioViewModel
        {
            ServicioId = servicio.IdServicio,
            ServicioNombre = servicio.Nombre,
            ServicioDescripcion = servicio.Descripcion,
            ServicioImagen = servicio.ImagenUrl,
            CategoriaNombre = servicio.IdCategoriaNavigation?.Nombre ?? "",
            PrecioBase = servicio.PrecioBase,
            ProveedorId = servicio.IdProveedor,
            ProveedorNombre = servicio.IdProveedorNavigation?.NombreComercial ?? "",
            ProveedorCiudad = servicio.IdProveedorNavigation?.Ciudad,
            ProveedorVerificado = servicio.IdProveedorNavigation?.Verificado ?? false,
            ProveedorCalificacion = servicio.IdProveedorNavigation?.CalificacionPromedio ?? 0,
            FechaEvento = DateOnly.FromDateTime(DateTime.Now.AddDays(30))
        };

        if (servicio.MatrizPrecios != null && servicio.MatrizPrecios.Any())
        {
            ViewBag.TieneMatriz = true;
            ViewBag.CantidadesMatriz = servicio.MatrizPrecios
                .Select(m => m.Cantidad).Distinct().OrderBy(x => x).ToList();
            ViewBag.HorasMatriz = servicio.MatrizPrecios
                .Select(m => m.Horas).Distinct().OrderBy(x => x).ToList();
            ViewBag.MatrizJson = System.Text.Json.JsonSerializer.Serialize(
                servicio.MatrizPrecios.Select(m => new
                {
                    cantidad = m.Cantidad,
                    horas = m.Horas,
                    precio = m.Precio
                }));
        }
        else
        {
            ViewBag.TieneMatriz = false;
        }

        var fechasOcupadas = servicio.Disponibilidades
            .Where(d => !d.Disponible && d.Fecha >= DateOnly.FromDateTime(DateTime.Now))
            .Select(d => d.Fecha.ToString("yyyy-MM-dd"))
            .ToList();

        ViewBag.FechasOcupadas = fechasOcupadas;

        return View(model);
    }
}
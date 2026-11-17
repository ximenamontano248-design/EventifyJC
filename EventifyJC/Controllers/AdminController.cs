using EventifyJC.Models;
using EventifyJC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventifyJC.Controllers;

[Authorize(Roles = "admin,administrador")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly IEmailService _emailService;

    public AdminController(
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
        ViewBag.TotalUsuarios = await _context.Usuarios.CountAsync();
        ViewBag.TotalProveedores = await _context.Proveedors.CountAsync();
        ViewBag.ProveedoresVerif = await _context.Proveedors.CountAsync(p => p.Verificado == true);
        ViewBag.TotalServicios = await _context.Servicios.CountAsync();
        ViewBag.TotalCategorias = await _context.Categoria.CountAsync();
        ViewBag.TotalSolicitudes = await _context.SolicitudCotizacions.CountAsync();
        ViewBag.TotalCotizaciones = await _context.Cotizacions.CountAsync();
        ViewBag.TotalReservas = await _context.Reservas.CountAsync();

        ViewBag.ProveedoresNoVerif = await _context.Proveedors.CountAsync(p => p.EstadoVerificacion == "no_verificado");
        ViewBag.ProveedoresEnRevision = await _context.Proveedors.CountAsync(p => p.EstadoVerificacion == "en_revision");
        ViewBag.ProveedoresVerificados = await _context.Proveedors.CountAsync(p => p.EstadoVerificacion == "verificado");
        ViewBag.ProveedoresRechazados = await _context.Proveedors.CountAsync(p => p.EstadoVerificacion == "rechazado");
        ViewBag.DocumentosPendientes = await _context.DocumentoVerificacions.CountAsync(d => d.Estado == "pendiente");

        ViewBag.TopProveedores = await _context.Proveedors
            .Include(p => p.IdUsuarioNavigation)
            .Where(p => p.Verificado == true)
            .OrderByDescending(p => p.CalificacionPromedio)
            .Take(5)
            .ToListAsync();

        ViewBag.UltimosUsuarios = await _context.Usuarios
            .OrderByDescending(u => u.FechaRegistro)
            .Take(5)
            .ToListAsync();

        return View();
    }

    // =========================================================
    // USUARIOS
    // =========================================================
    public async Task<IActionResult> Usuarios(string? rol, string? q, bool? activo)
    {
        var query = _context.Usuarios.AsQueryable();

        if (!string.IsNullOrWhiteSpace(rol))
            query = query.Where(u => u.Rol == rol);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => u.Nombre.Contains(q) || u.Email.Contains(q));

        if (activo.HasValue)
            query = query.Where(u => u.Activo == activo.Value);

        ViewBag.Rol = rol;
        ViewBag.Q = q;
        ViewBag.Activo = activo;
        ViewBag.Total = await query.CountAsync();

        return View(await query.OrderByDescending(u => u.FechaRegistro).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarActivo(int id)
    {
        var u = await _context.Usuarios.FindAsync(id);
        if (u == null) return NotFound();

        u.Activo = !(u.Activo ?? true);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Usuario {(u.Activo == true ? "activado" : "desactivado")}.";
        return RedirectToAction("Usuarios");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(int id, string rol)
    {
        var rolesValidos = new[] { "organizador", "proveedor", "admin" };
        if (!rolesValidos.Contains(rol)) return BadRequest();

        var u = await _context.Usuarios.FindAsync(id);
        if (u == null) return NotFound();

        u.Rol = rol;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Rol actualizado.";
        return RedirectToAction("Usuarios");
    }

    // =========================================================
    // PROVEEDORES
    // =========================================================
    public async Task<IActionResult> Proveedores(string? q, bool? verificado, string? ciudad)
    {
        var query = _context.Proveedors
            .Include(p => p.IdUsuarioNavigation)
            .Include(p => p.Servicios)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p =>
                p.NombreComercial!.Contains(q) ||
                p.RazonSocial!.Contains(q) ||
                p.Nit!.Contains(q));

        if (verificado.HasValue)
            query = query.Where(p => p.Verificado == verificado.Value);

        if (!string.IsNullOrWhiteSpace(ciudad))
            query = query.Where(p => p.Ciudad == ciudad);

        ViewBag.Q = q;
        ViewBag.Verificado = verificado;
        ViewBag.Ciudad = ciudad;
        ViewBag.Ciudades = await _context.Proveedors
            .Where(p => p.Ciudad != null)
            .Select(p => p.Ciudad!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return View(await query.OrderBy(p => p.NombreComercial).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerificarProveedor(int id)
    {
        var p = await _context.Proveedors.FindAsync(id);
        if (p == null) return NotFound();

        p.Verificado = !(p.Verificado ?? false);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Proveedor {(p.Verificado == true ? "verificado" : "desverificado")}.";
        return RedirectToAction("Proveedores");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarProveedor(int id)
    {
        var p = await _context.Proveedors
            .Include(x => x.Servicios)
                .ThenInclude(s => s.MatrizPrecios)
            .Include(x => x.Servicios)
                .ThenInclude(s => s.Disponibilidades)
            .Include(x => x.DocumentosVerificacion)
            .FirstOrDefaultAsync(x => x.IdProveedor == id);

        if (p == null) return NotFound();

        foreach (var s in p.Servicios)
        {
            _context.MatrizPrecios.RemoveRange(s.MatrizPrecios);
            _context.Disponibilidads.RemoveRange(s.Disponibilidades);
        }
        _context.Servicios.RemoveRange(p.Servicios);
        _context.DocumentoVerificacions.RemoveRange(p.DocumentosVerificacion);

        _context.Proveedors.Remove(p);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Proveedor eliminado.";
        return RedirectToAction("Proveedores");
    }

    // =========================================================
    // SERVICIOS
    // =========================================================
    public async Task<IActionResult> Servicios(string? q, int? categoriaId, string? disponibilidad)
    {
        var query = _context.Servicios
            .Include(s => s.IdCategoriaNavigation)
            .Include(s => s.IdProveedorNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(s => s.Nombre.Contains(q));

        if (categoriaId.HasValue)
            query = query.Where(s => s.IdCategoria == categoriaId.Value);

        if (!string.IsNullOrWhiteSpace(disponibilidad))
            query = query.Where(s => s.Disponibilidad == disponibilidad);

        ViewBag.Categorias = await _context.Categoria.OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Q = q;
        ViewBag.CategoriaId = categoriaId;
        ViewBag.Disponibilidad = disponibilidad;

        return View(await query.OrderBy(s => s.Nombre).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarServicio(int id)
    {
        var s = await _context.Servicios.FindAsync(id);
        if (s == null) return NotFound();

        _context.Servicios.Remove(s);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Servicio eliminado.";
        return RedirectToAction("Servicios");
    }

    // =========================================================
    // CATEGORÍAS
    // =========================================================
    public async Task<IActionResult> Categorias()
        => View(await _context.Categoria.Include(c => c.Servicios).OrderBy(c => c.Nombre).ToListAsync());

    [HttpGet] public IActionResult CrearCategoria() => View(new Categorium());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoria(Categorium model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _context.Categoria.AnyAsync(c => c.Nombre.ToLower() == model.Nombre.ToLower()))
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe una categoría con ese nombre.");
            return View(model);
        }

        _context.Categoria.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Categoría creada.";
        return RedirectToAction("Categorias");
    }

    [HttpGet]
    public async Task<IActionResult> EditarCategoria(int id)
    {
        var cat = await _context.Categoria.FindAsync(id);
        return cat == null ? NotFound() : View(cat);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCategoria(Categorium model)
    {
        if (!ModelState.IsValid) return View(model);

        var cat = await _context.Categoria.FindAsync(model.IdCategoria);
        if (cat == null) return NotFound();

        if (await _context.Categoria.AnyAsync(c => c.Nombre.ToLower() == model.Nombre.ToLower() && c.IdCategoria != model.IdCategoria))
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe una categoría con ese nombre.");
            return View(model);
        }

        cat.Nombre = model.Nombre;
        cat.Descripcion = model.Descripcion;
        cat.Link = model.Link;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Categoría actualizada.";
        return RedirectToAction("Categorias");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCategoria(int id)
    {
        var cat = await _context.Categoria.Include(c => c.Servicios).FirstOrDefaultAsync(c => c.IdCategoria == id);
        if (cat == null) return NotFound();

        if (cat.Servicios.Any())
        {
            TempData["Error"] = "No se puede eliminar: tiene servicios asociados.";
            return RedirectToAction("Categorias");
        }

        _context.Categoria.Remove(cat);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Categoría eliminada.";
        return RedirectToAction("Categorias");
    }

    // =========================================================
    // SOLICITUDES SIN RESPUESTA
    // =========================================================
    public async Task<IActionResult> SolicitudesSinRespuesta()
    {
        var solicitudes = await _context.SolicitudCotizacions
            .Include(s => s.IdOrganizadorNavigation)
            .Where(s => s.Estado == "pendiente" && !s.Cotizacions.Any())
            .OrderBy(s => s.FechaSolicitud)
            .ToListAsync();

        return View(solicitudes);
    }

    // =========================================================
    // AUDITORÍA
    // =========================================================
    public async Task<IActionResult> Auditoria(string? tabla)
    {
        var query = _context.Auditoria
            .Include(a => a.UsuarioAccionNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tabla))
            query = query.Where(a => a.Tabla == tabla);

        ViewBag.Tabla = tabla;
        ViewBag.Tablas = await _context.Auditoria
            .Where(a => a.Tabla != null)
            .Select(a => a.Tabla!)
            .Distinct()
            .ToListAsync();

        return View(await query.OrderByDescending(a => a.FechaHora).Take(500).ToListAsync());
    }

    // =========================================================
    // VERIFICACIÓN DE PROVEEDORES
    // =========================================================
    public async Task<IActionResult> RevisarDocumentos(string? estado)
    {
        var query = _context.Proveedors
            .Include(p => p.IdUsuarioNavigation)
            .Include(p => p.DocumentosVerificacion)
            .Where(p => p.DocumentosVerificacion.Any());

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.EstadoVerificacion == estado);

        var proveedores = await query
            .OrderBy(p => p.EstadoVerificacion)
            .ThenBy(p => p.FechaSolicitudVerificacion)
            .ToListAsync();

        ViewBag.EstadoFiltro = estado;
        ViewBag.TotalNoVerificado = await _context.Proveedors.CountAsync(p => p.EstadoVerificacion == "no_verificado");
        ViewBag.TotalEnRevision = await _context.Proveedors.CountAsync(p => p.EstadoVerificacion == "en_revision");
        ViewBag.TotalVerificado = await _context.Proveedors.CountAsync(p => p.EstadoVerificacion == "verificado");
        ViewBag.TotalRechazado = await _context.Proveedors.CountAsync(p => p.EstadoVerificacion == "rechazado");

        return View(proveedores);
    }

    public async Task<IActionResult> DetalleVerificacion(int id)
    {
        var proveedor = await _context.Proveedors
            .Include(p => p.IdUsuarioNavigation)
            .Include(p => p.DocumentosVerificacion)
                .ThenInclude(d => d.RevisadoPorNavigation)
            .FirstOrDefaultAsync(p => p.IdProveedor == id);

        if (proveedor == null) return NotFound();

        return View(proveedor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevisarDocumento(RevisionDocumentoViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var doc = await _context.DocumentoVerificacions
            .Include(d => d.IdProveedorNavigation)
            .FirstOrDefaultAsync(d => d.IdDocumento == model.IdDocumento);

        if (doc == null) return NotFound();

        doc.Estado = model.Estado;
        doc.Observacion = model.Observacion;
        doc.RevisadoPor = user.IdUsuario;
        doc.FechaRevision = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Documento marcado como '{model.Estado}'.";
        return RedirectToAction("DetalleVerificacion", new { id = doc.IdProveedor });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AprobarProveedor(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var proveedor = await _context.Proveedors
            .Include(p => p.IdUsuarioNavigation)
            .FirstOrDefaultAsync(p => p.IdProveedor == id);

        if (proveedor == null) return NotFound();

        proveedor.EstadoVerificacion = "verificado";
        proveedor.Verificado = true;
        proveedor.FechaVerificacion = DateTime.Now;
        proveedor.VerificadoPor = user.IdUsuario;
        proveedor.MotivoRechazo = null;

        await _context.SaveChangesAsync();

        try
        {
            if (proveedor.IdUsuarioNavigation != null && !string.IsNullOrEmpty(proveedor.IdUsuarioNavigation.Email))
            {
                await _emailService.SendEmailAsync(
                    proveedor.IdUsuarioNavigation.Email,
                    proveedor.IdUsuarioNavigation.Nombre,
                    "¡Tu empresa ha sido verificada!",
                    $@"
                    <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; padding: 32px; background: #ffffff; border-radius: 16px;'>
                        <h2 style='color: #1b2340;'>¡Felicidades {proveedor.NombreComercial}!</h2>
                        <p style='color: #6b7080;'>Tu empresa ha sido <strong>verificada exitosamente</strong> por Eventify JC.</p>
                        <p style='color: #6b7080;'>Ya puedes publicar tus servicios y recibir cotizaciones.</p>
                        <p style='text-align: center; margin: 24px 0;'>
                            <a href='https://eventifyjc-a5fxehe7g9babpba.westus3-01.azurewebsites.net/Proveedor'
                               style='background: #e8823c; color: #fff; padding: 12px 24px; border-radius: 10px; text-decoration: none; font-weight: bold;'>
                                Ir a mi negocio
                            </a>
                        </p>
                    </div>");
            }
        }
        catch { }

        TempData["Success"] = "Proveedor verificado correctamente.";
        return RedirectToAction("RevisarDocumentos");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RechazarProveedor(RechazoProveedorViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var proveedor = await _context.Proveedors
            .Include(p => p.IdUsuarioNavigation)
            .FirstOrDefaultAsync(p => p.IdProveedor == model.IdProveedor);

        if (proveedor == null) return NotFound();

        proveedor.EstadoVerificacion = "rechazado";
        proveedor.Verificado = false;
        proveedor.MotivoRechazo = model.Motivo;
        proveedor.FechaVerificacion = DateTime.Now;
        proveedor.VerificadoPor = user.IdUsuario;

        await _context.SaveChangesAsync();

        try
        {
            if (proveedor.IdUsuarioNavigation != null && !string.IsNullOrEmpty(proveedor.IdUsuarioNavigation.Email))
            {
                await _emailService.SendEmailAsync(
                    proveedor.IdUsuarioNavigation.Email,
                    proveedor.IdUsuarioNavigation.Nombre,
                    "Tu verificación fue rechazada",
                    $@"
                    <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; padding: 32px; background: #ffffff; border-radius: 16px;'>
                        <h2 style='color: #1b2340;'>Hola {proveedor.NombreComercial},</h2>
                        <p style='color: #6b7080;'>Tu solicitud de verificación ha sido <strong style='color: #dc2626;'>rechazada</strong>.</p>
                        <div style='background: #fef2f2; border-left: 4px solid #dc2626; padding: 16px; border-radius: 8px; margin: 24px 0;'>
                            <strong>Motivo:</strong> {model.Motivo}
                        </div>
                        <p style='color: #6b7080;'>Puedes corregir la información y volver a subir tus documentos.</p>
                        <p style='text-align: center; margin: 24px 0;'>
                            <a href='https://eventifyjc-a5fxehe7g9babpba.westus3-01.azurewebsites.net/Proveedor/SubirDocumentos'
                               style='background: #e8823c; color: #fff; padding: 12px 24px; border-radius: 10px; text-decoration: none; font-weight: bold;'>
                                Volver a subir documentos
                            </a>
                        </p>
                    </div>");
            }
        }
        catch { }

        TempData["Success"] = "Proveedor rechazado. Se notificó por correo.";
        return RedirectToAction("RevisarDocumentos");
    }
}
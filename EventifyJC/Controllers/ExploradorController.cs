using EventifyJC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventifyJC.Controllers;

public class ExploradorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public ExploradorController(ApplicationDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // =========================================================
    // LISTA DE SERVICIOS CON FILTROS AVANZADOS
    // =========================================================
    public async Task<IActionResult> Index(
        string? q, int? categoriaId, string? ciudad,
        decimal? precioMin, decimal? precioMax,
        bool? verificado, string? orden, int pagina = 1)
    {
        var query = _context.Servicios
            .Include(s => s.IdCategoriaNavigation)
            .Include(s => s.IdProveedorNavigation)
                .ThenInclude(p => p!.IdUsuarioNavigation)
            .AsQueryable();

        // =========================================================
        // FILTROS QUE SÍ PUEDE HACER SQL
        // (el query nunca se materializa antes de tiempo)
        // =========================================================
        if (categoriaId.HasValue)
            query = query.Where(s => s.IdCategoria == categoriaId.Value);

        if (precioMin.HasValue)
            query = query.Where(s => s.PrecioBase >= precioMin.Value);

        if (precioMax.HasValue)
            query = query.Where(s => s.PrecioBase <= precioMax.Value);

        if (verificado.HasValue && verificado.Value)
            query = query.Where(s => s.IdProveedorNavigation!.Verificado == true);

        // Ordenamiento
        query = orden switch
        {
            "precio_asc" => query.OrderBy(s => s.PrecioBase),
            "precio_desc" => query.OrderByDescending(s => s.PrecioBase),
            "rating" => query.OrderByDescending(s => s.IdProveedorNavigation!.CalificacionPromedio),
            "nombre" => query.OrderBy(s => s.Nombre),
            _ => query.OrderByDescending(s => s.IdServicio),
        };

        // =========================================================
        // UNA SOLA CONSULTA A LA BASE DE DATOS
        // =========================================================
        var todos = await query.ToListAsync();

        // =========================================================
        // FILTROS SIN TILDES (en memoria, después de traer los datos)
        // List.Where conserva el orden aplicado en SQL
        // =========================================================
        if (!string.IsNullOrWhiteSpace(ciudad))
        {
            var ciudadNormalizada = QuitarTildes(ciudad);
            todos = todos
                .Where(s => QuitarTildes(s.IdProveedorNavigation?.Ciudad ?? "") == ciudadNormalizada)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var qNormalizado = QuitarTildes(q);
            todos = todos.Where(s =>
                QuitarTildes(s.Nombre ?? "").Contains(qNormalizado) ||
                QuitarTildes(s.Descripcion ?? "").Contains(qNormalizado) ||
                QuitarTildes(s.IdProveedorNavigation?.NombreComercial ?? "").Contains(qNormalizado)
            ).ToList();
        }

        var total = todos.Count;

        // =========================================================
        // MAPEAR A DTO
        // =========================================================
        var servicios = todos.Select(s => new ServiceDto
        {
            Id = s.IdServicio,
            Nombre = s.Nombre,
            Descripcion = s.Descripcion,
            PrecioBase = s.PrecioBase,
            ImagenUrl = s.ImagenUrl,
            CategoriaNombre = s.IdCategoriaNavigation?.Nombre ?? "",
            ProveedorId = s.IdProveedor,
            ProveedorNombre = s.IdProveedorNavigation?.NombreComercial ?? "",
            Ciudad = s.IdProveedorNavigation?.Ciudad,
            Verificado = s.IdProveedorNavigation?.Verificado ?? false,
            CalificacionPromedio = s.IdProveedorNavigation?.CalificacionPromedio ?? 0,
        }).ToList();

        // =========================================================
        // CARGAR DATOS DE FILTROS
        // =========================================================
        ViewBag.Categorias = await _context.Categoria.OrderBy(c => c.Nombre).ToListAsync();

        var ciudadesRaw = await _context.Proveedors
            .Where(p => p.Ciudad != null && p.Ciudad != "")
            .Select(p => p.Ciudad!)
            .Distinct()
            .ToListAsync();

        ViewBag.Ciudades = ciudadesRaw
            .GroupBy(c => QuitarTildes(c))
            .Select(g => g.OrderByDescending(c => c == c.ToUpper() || c.Contains('á') || c.Contains('é') || c.Contains('í') || c.Contains('ó') || c.Contains('ú')) // Preferir la versión con tilde
                           .ThenByDescending(c => c[0] == char.ToUpper(c[0]))
                           .First())
            .OrderBy(c => c)
            .ToList();

        ViewBag.Q = q;
        ViewBag.CategoriaId = categoriaId;
        ViewBag.Ciudad = ciudad;
        ViewBag.PrecioMin = precioMin;
        ViewBag.PrecioMax = precioMax;
        ViewBag.Verificado = verificado;
        ViewBag.Orden = orden;
        ViewBag.PaginaActual = 1;
        ViewBag.TotalPaginas = 1;
        ViewBag.TotalResultados = total;
        ViewBag.EstaLogueado = User.Identity?.IsAuthenticated == true;

        return View(servicios);
    }

    // =========================================================
    // DETALLE DE UN SERVICIO (público, con CTA según login)
    // =========================================================
    public async Task<IActionResult> Detalle(int id)
    {
        var servicio = await _context.Servicios
            .Include(s => s.IdCategoriaNavigation)
            .Include(s => s.IdProveedorNavigation)
                .ThenInclude(p => p!.IdUsuarioNavigation)
            .Include(s => s.IdProveedorNavigation)
                .ThenInclude(p => p!.Servicios)
            .FirstOrDefaultAsync(s => s.IdServicio == id);

        if (servicio == null) return NotFound();

        // Servicios relacionados (misma categoría, distinto id)
        ViewBag.Relacionados = await _context.Servicios
            .Include(s => s.IdProveedorNavigation)
            .Include(s => s.IdCategoriaNavigation)
            .Where(s => s.IdCategoria == servicio.IdCategoria && s.IdServicio != id)
            .Take(4)
            .Select(s => new ServiceDto
            {
                Id = s.IdServicio,
                Nombre = s.Nombre,
                Descripcion = s.Descripcion,
                PrecioBase = s.PrecioBase,
                ImagenUrl = s.ImagenUrl,
                CategoriaNombre = s.IdCategoriaNavigation != null ? s.IdCategoriaNavigation.Nombre : "",
                ProveedorId = s.IdProveedor,
                ProveedorNombre = s.IdProveedorNavigation != null ? (s.IdProveedorNavigation.NombreComercial ?? "") : "",
                Ciudad = s.IdProveedorNavigation != null ? s.IdProveedorNavigation.Ciudad : null,
                Verificado = s.IdProveedorNavigation != null ? (s.IdProveedorNavigation.Verificado ?? false) : false,
                CalificacionPromedio = s.IdProveedorNavigation != null ? (s.IdProveedorNavigation.CalificacionPromedio ?? 0) : 0,
            })
            .ToListAsync();

        ViewBag.EstaLogueado = User.Identity?.IsAuthenticated == true;
        ViewBag.EsOrganizador = User.IsInRole("organizador");

        return View(servicio);
    }

    // =========================================================
    // HELPER: Quitar tildes y normalizar texto
    // =========================================================
    private static string QuitarTildes(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        var normalized = texto.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant();
    }
}
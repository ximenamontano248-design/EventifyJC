using EventifyJC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventifyJC.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categorias = await _context.Categoria
            .Select(c => new CategoryDto
            {
                IdCategoria = c.IdCategoria,
                Slug = c.Nombre.ToLower().Replace(" ", "-"),
                Name = c.Nombre,
                Count = c.Servicios.Count(),
            })
            .ToListAsync();

        var proveedores = await _context.Proveedors
            .Include(p => p.IdUsuarioNavigation)
            .OrderByDescending(p => p.CalificacionPromedio)
            .Take(4)
            .Select(p => new ProviderDto
            {
                Id = p.IdProveedor,
                Name = p.IdUsuarioNavigation.Nombre,
                Category = p.Servicios.Select(s => s.IdCategoriaNavigation.Nombre).FirstOrDefault() ?? "General",
                City = p.Ciudad,
                Rating = (double)(p.CalificacionPromedio ?? 0),
                Reviews = 0, // TODO: calcular vía Calificacion -> Reserva -> Cotizacion -> Servicio -> Proveedor si quieres el número real
                Price = "Consultar",
                Verified = p.Verificado ?? false,
            })
            .ToListAsync();

        var vm = new HomeViewModel
        {
            Categories = categorias,
            Providers = proveedores,
        };

        return View(vm);
    }

    public async Task<IActionResult> Marketplace()
    {
        var q = Request.Query["q"].ToString();
        var categoryFilter = Request.Query["category"].ToString();
        var cityFilter = Request.Query["city"].ToString();
        var verifiedOnly = Request.Query["verified"].ToString() == "true";

        var query = _context.Servicios
            .Include(s => s.IdCategoriaNavigation)
            .Include(s => s.IdProveedorNavigation)
                .ThenInclude(p => p!.IdUsuarioNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(s =>
                s.Nombre.Contains(q) ||
                (s.IdProveedorNavigation != null && s.IdProveedorNavigation.IdUsuarioNavigation.Nombre.Contains(q)));
        }

        if (int.TryParse(categoryFilter, out var catId))
        {
            query = query.Where(s => s.IdCategoria == catId);
        }

        if (!string.IsNullOrWhiteSpace(cityFilter))
        {
            query = query.Where(s => s.IdProveedorNavigation != null && s.IdProveedorNavigation.Ciudad == cityFilter);
        }

        if (verifiedOnly)
        {
            query = query.Where(s => s.IdProveedorNavigation != null && s.IdProveedorNavigation.Verificado == true);
        }

        var servicios = await query
            .Select(s => new ServiceDto
            {
                Id = s.IdServicio,
                Nombre = s.Nombre,
                Descripcion = s.Descripcion,
                PrecioBase = s.PrecioBase,
                ImagenUrl = s.ImagenUrl,
                CategoriaNombre = s.IdCategoriaNavigation.Nombre,
                ProveedorId = s.IdProveedor,
                ProveedorNombre = s.IdProveedorNavigation != null ? s.IdProveedorNavigation.IdUsuarioNavigation.Nombre : "",
                Ciudad = s.IdProveedorNavigation != null ? s.IdProveedorNavigation.Ciudad : null,
                Verificado = s.IdProveedorNavigation != null && (s.IdProveedorNavigation.Verificado ?? false),
                CalificacionPromedio = s.IdProveedorNavigation != null ? (s.IdProveedorNavigation.CalificacionPromedio ?? 0) : 0,
            })
            .ToListAsync();

        var categorias = await _context.Categoria.OrderBy(c => c.Nombre).ToListAsync();

        var cities = await _context.Proveedors
            .Where(p => p.Ciudad != null)
            .Select(p => p.Ciudad!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var vm = new MarketplaceViewModel
        {
            Servicios = servicios,
            Categorias = categorias,
            Cities = cities,
        };

        return View(vm);
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
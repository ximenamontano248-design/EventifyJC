using EventifyJC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventifyJC.Controllers;

[Authorize(Roles = "admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // === VISTA PRINCIPAL DEL PANEL ===
    public async Task<IActionResult> Index()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        var proveedores = await _context.Proveedors.ToListAsync();
        var categorias = await _context.Categoria.ToListAsync();

        ViewBag.Usuarios = usuarios;
        ViewBag.Proveedores = proveedores;
        ViewBag.Categorias = categorias;

        return View();
    }

    // === HU-04: ACTIVAR / DESACTIVAR USUARIO ===
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarEstadoUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            // CORREGIDO: Manejar bool? (nullable)
            usuario.Activo = !(usuario.Activo ?? false);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Usuario {(usuario.Activo == true ? "activado" : "desactivado")} correctamente.";
        }
        return RedirectToAction("Index");
    }

    // === HU-09: VERIFICAR PROVEEDOR ===
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerificarProveedor(int id)
    {
        var proveedor = await _context.Proveedors.FindAsync(id);
        if (proveedor != null)
        {
            proveedor.Verificado = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Proveedor '{proveedor.NombreComercial}' verificado correctamente.";
        }
        return RedirectToAction("Index");
    }

    // === HU-08: CREAR CATEGORÍA ===
    [HttpGet]
    public IActionResult CrearCategoria()
    {
        return View(new Categorium());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoria(Categorium model)
    {
        if (string.IsNullOrWhiteSpace(model.Nombre))
        {
            ModelState.AddModelError(nameof(model.Nombre), "El nombre es obligatorio.");
            return View(model);
        }

        var existe = await _context.Categoria.AnyAsync(c => c.Nombre == model.Nombre);
        if (existe)
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe una categoría con ese nombre.");
            return View(model);
        }

        _context.Categoria.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Categoría creada correctamente.";
        return RedirectToAction("Index");
    }

    // === HU-08: EDITAR CATEGORÍA ===
    [HttpGet]
    public async Task<IActionResult> EditarCategoria(int id)
    {
        var categoria = await _context.Categoria.FindAsync(id);
        if (categoria == null) return NotFound();
        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCategoria(Categorium model)
    {
        if (string.IsNullOrWhiteSpace(model.Nombre))
        {
            ModelState.AddModelError(nameof(model.Nombre), "El nombre es obligatorio.");
            return View(model);
        }

        var categoria = await _context.Categoria.FindAsync(model.IdCategoria);
        if (categoria == null) return NotFound();

        categoria.Nombre = model.Nombre;
        categoria.Descripcion = model.Descripcion;
        categoria.Link = model.Link;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Categoría actualizada correctamente.";
        return RedirectToAction("Index");
    }

    // === HU-08: ELIMINAR CATEGORÍA ===
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCategoria(int id)
    {
        var categoria = await _context.Categoria.FindAsync(id);
        if (categoria != null)
        {
            _context.Categoria.Remove(categoria);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Categoría eliminada correctamente.";
        }
        return RedirectToAction("Index");
    }
}
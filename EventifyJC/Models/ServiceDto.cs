namespace EventifyJC.Models;

public class ServiceDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioBase { get; set; }
    public string? ImagenUrl { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public int? ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public string? Ciudad { get; set; }
    public bool Verificado { get; set; }
    public decimal CalificacionPromedio { get; set; }
}

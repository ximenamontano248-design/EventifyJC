using System.ComponentModel.DataAnnotations;

namespace EventifyJC.Models;

public class CotizarServicioViewModel
{
    public int ServicioId { get; set; }
    public string ServicioNombre { get; set; } = string.Empty;
    public string? ServicioDescripcion { get; set; }
    public string? ServicioImagen { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }

    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public string? ProveedorCiudad { get; set; }
    public bool ProveedorVerificado { get; set; }
    public decimal ProveedorCalificacion { get; set; }

    [Required(ErrorMessage = "La fecha del evento es obligatoria.")]
    [Display(Name = "Fecha del evento")]
    [DataType(DataType.Date)]
    public DateOnly FechaEvento { get; set; }

    [Required(ErrorMessage = "Cuéntanos sobre tu evento.")]
    [Display(Name = "Detalles del evento")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Mínimo 10 caracteres.")]
    public string DescripcionEvento { get; set; } = string.Empty;

    [Display(Name = "Presupuesto estimado (COP)")]
    [Range(0, double.MaxValue, ErrorMessage = "Debe ser un número positivo.")]
    public decimal? PresupuestoEstimado { get; set; }
}
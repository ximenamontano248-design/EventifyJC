using System.ComponentModel.DataAnnotations;

namespace EventifyJC.Models;

public class SolicitudCotizacionViewModel
{
    [Required(ErrorMessage = "La fecha del evento es obligatoria.")]
    [Display(Name = "Fecha del evento")]
    [DataType(DataType.Date)]
    public DateOnly FechaEvento { get; set; }

    [Required(ErrorMessage = "Describe tu evento.")]
    [Display(Name = "Descripción del evento")]
    [StringLength(1000)]
    public string DescripcionEvento { get; set; } = string.Empty;

    [Display(Name = "Presupuesto estimado (COP)")]
    [Range(0, double.MaxValue)]
    public decimal? PresupuestoEstimado { get; set; }

    // Opcional: si viene desde un servicio específico
    public int? ServicioId { get; set; }
    public string? ServicioNombre { get; set; }
    public string? ProveedorNombre { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace EventifyJC.Models;

public class CotizacionViewModel
{
    public int IdSolicitud { get; set; }
    public string? NombreOrganizador { get; set; }
    public DateOnly FechaEvento { get; set; }
    public string? DescripcionEvento { get; set; }
    public decimal PresupuestoEstimado { get; set; }

    [Required(ErrorMessage = "Selecciona un servicio.")]
    public int IdServicio { get; set; }

    [Required(ErrorMessage = "Ingresa el monto de tu cotización.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
    public decimal Monto { get; set; }

    [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string? Mensaje { get; set; }
}
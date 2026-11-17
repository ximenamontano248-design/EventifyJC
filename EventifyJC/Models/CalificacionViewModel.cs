using System.ComponentModel.DataAnnotations;

namespace EventifyJC.Models;

public class CalificacionViewModel
{
    public int IdReserva { get; set; }

    public string? ServicioNombre { get; set; }
    public string? ProveedorNombre { get; set; }
    public DateOnly FechaEvento { get; set; }

    [Required(ErrorMessage = "Selecciona una puntuación.")]
    [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5.")]
    [Display(Name = "Puntuación")]
    public int Puntuacion { get; set; }

    [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    [Display(Name = "Comentario")]
    public string? Comentario { get; set; }
}
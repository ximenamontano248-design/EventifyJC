using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EventifyJC.Models;

public class ServicioViewModel
{
    public int? IdServicio { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [Display(Name = "Categoría")]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El nombre del servicio es obligatorio")]
    [Display(Name = "Nombre del servicio")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El precio base es obligatorio")]
    [Display(Name = "Precio base (COP)")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser un número positivo")]
    public decimal PrecioBase { get; set; }

    [Display(Name = "Disponibilidad")]
    public string? Disponibilidad { get; set; } = "disponible";

    [Display(Name = "Imagen del servicio")]
    public IFormFile? Imagen { get; set; }

    public string? ImagenUrl { get; set; }
}

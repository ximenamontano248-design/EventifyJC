using System.ComponentModel.DataAnnotations;

namespace EventifyJC.Models;

public class MatrizPrecioViewModel
{
    public int IdServicio { get; set; }
    public string ServicioNombre { get; set; } = "";

    [Required]
    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = "servicio";

    [Required(ErrorMessage = "Indica la cantidad mínima.")]
    [Range(1, int.MaxValue)]
    [Display(Name = "Cantidad mínima")]
    public int CantidadMinima { get; set; } = 50;

    [Required(ErrorMessage = "Indica la cantidad máxima.")]
    [Range(1, int.MaxValue)]
    [Display(Name = "Cantidad máxima")]
    public int CantidadMaxima { get; set; } = 500;

    [Required(ErrorMessage = "Indica el incremento entre bloques.")]
    [Range(1, int.MaxValue)]
    [Display(Name = "Incremento")]
    public int CantidadIncremento { get; set; } = 50;

    [Display(Name = "Bloques de horas separadas por coma")]
    public string BloquesHoras { get; set; } = "4,6,8,24";

    [Display(Name = "Capacidad simultánea")]
    public int? CapacidadSimultanea { get; set; }

    public Dictionary<int, Dictionary<int, decimal>> Precios { get; set; } = new();
}
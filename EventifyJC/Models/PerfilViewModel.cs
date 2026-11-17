using System.ComponentModel.DataAnnotations;

namespace EventifyJC.Models;

public class PerfilViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Teléfono")]
    [Phone(ErrorMessage = "Teléfono inválido.")]
    public string? Telefono { get; set; }

    public string Rol { get; set; } = string.Empty;
    public DateTime? FechaRegistro { get; set; }
}
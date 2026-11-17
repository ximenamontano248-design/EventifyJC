using System.ComponentModel.DataAnnotations;

namespace EventifyJC.Models;

public class OlvideContrasenaViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;
}
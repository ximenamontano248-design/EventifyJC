using System.ComponentModel.DataAnnotations;

namespace EventifyJC.Models;

public class CambiarContrasenaViewModel
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string PasswordNueva { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu nueva contraseña.")]
    [DataType(DataType.Password)]
    [Compare("PasswordNueva", ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar nueva contraseña")]
    public string PasswordConfirm { get; set; } = string.Empty;
}
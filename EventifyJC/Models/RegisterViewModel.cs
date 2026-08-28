// Models/RegisterViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace EventifyJC.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Selecciona un tipo de cuenta")]
    [Display(Name = "Tipo de cuenta")]
    public string Rol { get; set; } = "organizador"; // "organizador" | "proveedor"

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [Display(Name = "Nombres")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [Display(Name = "Apellidos")]
    public string Apellido { get; set; } = null!;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Confirma tu contraseña")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar contraseña")]
    public string PasswordConfirm { get; set; } = null!;

    [Required(ErrorMessage = "El celular es obligatorio")]
    [Display(Name = "Celular")]
    public string Telefono { get; set; } = null!;
}
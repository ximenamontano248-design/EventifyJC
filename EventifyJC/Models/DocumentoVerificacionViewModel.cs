using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EventifyJC.Models;

public class DocumentoVerificacionViewModel
{
    [Required(ErrorMessage = "Selecciona el tipo de documento.")]
    [Display(Name = "Tipo de documento")]
    public string Tipo { get; set; } = "rut";

    [Required(ErrorMessage = "Debes subir un archivo.")]
    [Display(Name = "Archivo (PDF, JPG o PNG)")]
    public IFormFile Archivo { get; set; } = null!;
}

public class RevisionDocumentoViewModel
{
    public int IdDocumento { get; set; }

    [Required]
    public string Estado { get; set; } = "aprobado";

    [StringLength(500)]
    [Display(Name = "Observación")]
    public string? Observacion { get; set; }
}

public class RechazoProveedorViewModel
{
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "Debes indicar el motivo del rechazo.")]
    [StringLength(500)]
    [Display(Name = "Motivo del rechazo")]
    public string Motivo { get; set; } = null!;
}
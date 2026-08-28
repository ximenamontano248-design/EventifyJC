// Models/ProveedorViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EventifyJC.Models;

public class ProveedorViewModel
{
    // Datos del negocio
    [Required(ErrorMessage = "El nombre comercial es obligatorio")]
    [Display(Name = "Nombre comercial")]
    public string NombreComercial { get; set; } = null!;

    [Required(ErrorMessage = "La razón social es obligatoria")]
    [Display(Name = "Razón social")]
    public string RazonSocial { get; set; } = null!;

    [Required(ErrorMessage = "El NIT es obligatorio")]
    [Display(Name = "NIT")]
    public string Nit { get; set; } = null!;

    [Required(ErrorMessage = "El nombre del representante es obligatorio")]
    [Display(Name = "Nombre del representante")]
    public string Representante { get; set; } = null!;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Display(Name = "Teléfono")]
    public string TelefonoNegocio { get; set; } = null!;

    // Ubicación y cobertura
    [Required(ErrorMessage = "La ciudad principal es obligatoria")]
    [Display(Name = "Ciudad principal")]
    public string Ciudad { get; set; } = null!;

    [Required(ErrorMessage = "La dirección es obligatoria")]
    [Display(Name = "Dirección")]
    public string Direccion { get; set; } = null!;

    [Display(Name = "Nivel de cobertura")]
    public string NivelCobertura { get; set; } = "municipal";

    [Display(Name = "Ciudades cubiertas")]
    public string? CiudadesCubiertas { get; set; }

    // Servicios
    [Required(ErrorMessage = "Selecciona al menos una categoría")]
    [Display(Name = "Categorías")]
    public string Categorias { get; set; } = null!; // CSV, construido por JS desde los botones

    [Display(Name = "Descripción detallada")]
    public string? Descripcion { get; set; }

    [Display(Name = "Tarifas aproximadas")]
    public string? Tarifas { get; set; }

    [Display(Name = "Capacidad de atención")]
    public string? Capacidad { get; set; }

    [Display(Name = "Anticipación requerida")]
    public string? AnticipacionRequerida { get; set; }

    // Portafolio
    [Display(Name = "Logo")]
    public IFormFile? Logo { get; set; }

    [Display(Name = "Fotografías (URLs separadas por coma)")]
    public string? Galeria { get; set; }

    [Display(Name = "Videos (URLs separadas por coma)")]
    public string? Videos { get; set; }

    // Redes
    [Display(Name = "WhatsApp")]
    public string? Whatsapp { get; set; }

    [Display(Name = "Instagram")]
    public string? Instagram { get; set; }

    [Display(Name = "Facebook")]
    public string? Facebook { get; set; }

    [Display(Name = "Sitio web")]
    public string? SitioWeb { get; set; }

    [Display(Name = "Autorización de datos")]
    public bool AceptaTratamientoDatos { get; set; }
}
// Models/Proveedor.cs
using System;
using System.Collections.Generic;

namespace EventifyJC.Models;

public partial class Proveedor
{
    public int IdProveedor { get; set; }
    public int IdUsuario { get; set; }
    public string? NombreComercial { get; set; }
    public string? RazonSocial { get; set; }
    public string? Nit { get; set; }
    public string? Representante { get; set; }
    public string? TelefonoNegocio { get; set; }
    public string? Descripcion { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? NivelCobertura { get; set; }
    public string? CiudadesCubiertas { get; set; }
    public string? Categorias { get; set; }
    public string? Tarifas { get; set; }
    public string? Capacidad { get; set; }
    public string? AnticipacionRequerida { get; set; }
    public string? LogoUrl { get; set; }
    public string? Galeria { get; set; }
    public string? Videos { get; set; }
    public string? Whatsapp { get; set; }
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public string? SitioWeb { get; set; }
    public decimal? Lat { get; set; }
    public decimal? Lng { get; set; }
    public bool? Verificado { get; set; }
    public decimal? CalificacionPromedio { get; set; }

    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
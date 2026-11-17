using System;
using System.Collections.Generic;

namespace EventifyJC.Models;

public partial class Servicio
{
    public int IdServicio { get; set; }

    public int IdProveedor { get; set; }

    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal PrecioBase { get; set; }

    public string? Disponibilidad { get; set; }

    public string? ImagenUrl { get; set; }

    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();

    public virtual Categorium IdCategoriaNavigation { get; set; } = null!;

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
    // =========================================================
    // Tipo y cantidades (Producto vs Servicio)
    // =========================================================
    public string Tipo { get; set; } = "servicio";
    public int? CantidadMinima { get; set; }
    public int? CantidadMaxima { get; set; }
    public int? CantidadIncremento { get; set; }
    public int? CapacidadSimultanea { get; set; }

    public virtual ICollection<MatrizPrecio> MatrizPrecios { get; set; } = new List<MatrizPrecio>();
    public virtual ICollection<Disponibilidad> Disponibilidades { get; set; } = new List<Disponibilidad>();
}

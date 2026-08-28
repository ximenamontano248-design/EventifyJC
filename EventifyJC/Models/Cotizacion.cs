using System;
using System.Collections.Generic;

namespace EventifyJC.Models;

public partial class Cotizacion
{
    public int IdCotizacion { get; set; }

    public int IdSolicitud { get; set; }

    public int IdProveedor { get; set; }

    public int IdServicio { get; set; }

    public decimal Monto { get; set; }

    public string? Mensaje { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public string? Estado { get; set; }

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    public virtual Servicio IdServicioNavigation { get; set; } = null!;

    public virtual SolicitudCotizacion IdSolicitudNavigation { get; set; } = null!;

    public virtual Reserva? Reserva { get; set; }
}

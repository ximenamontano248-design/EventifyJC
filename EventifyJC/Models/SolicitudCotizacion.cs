using System;
using System.Collections.Generic;

namespace EventifyJC.Models;

public partial class SolicitudCotizacion
{
    public int IdSolicitud { get; set; }

    public int IdOrganizador { get; set; }

    public DateTime? FechaSolicitud { get; set; }

    public DateOnly FechaEvento { get; set; }

    public string? DescripcionEvento { get; set; }

    public decimal? PresupuestoEstimado { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();

    public virtual Usuario IdOrganizadorNavigation { get; set; } = null!;
}

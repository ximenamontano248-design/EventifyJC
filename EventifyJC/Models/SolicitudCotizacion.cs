using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventifyJC.Models;

public partial class SolicitudCotizacion
{
    [Key]
    [Column("id_solicitud")]
    public int IdSolicitud { get; set; }

    [Column("id_organizador")]
    public int IdOrganizador { get; set; }

    [Column("id_servicio")]
    public int? IdServicio { get; set; }

    [Column("fecha_solicitud")]
    public DateTime? FechaSolicitud { get; set; }

    [Column("fecha_evento")]
    public DateOnly FechaEvento { get; set; }

    [Column("descripcion_evento")]
    public string? DescripcionEvento { get; set; }

    [Column("presupuesto_estimado")]
    public decimal? PresupuestoEstimado { get; set; }

    [Column("estado")]
    public string? Estado { get; set; }

    // Relaciones
    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();

    [ForeignKey("IdOrganizador")]
    public virtual Usuario IdOrganizadorNavigation { get; set; } = null!;

    [ForeignKey("IdServicio")]
    public virtual Servicio? IdServicioNavigation { get; set; }
}
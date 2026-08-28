using System;
using System.Collections.Generic;

namespace EventifyJC.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public int IdCotizacion { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaConfirmacion { get; set; }

    public decimal MontoFinal { get; set; }

    public decimal ComisionEventify { get; set; }

    public virtual ICollection<Calificacion> Calificacions { get; set; } = new List<Calificacion>();

    public virtual Cotizacion IdCotizacionNavigation { get; set; } = null!;

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}

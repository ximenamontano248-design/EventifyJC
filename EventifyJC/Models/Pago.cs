using System;
using System.Collections.Generic;

namespace EventifyJC.Models;

public partial class Pago
{
    public int IdPago { get; set; }

    public int IdReserva { get; set; }

    public decimal Monto { get; set; }

    public string Metodo { get; set; } = null!;

    public string? Estado { get; set; }

    public DateTime? FechaPago { get; set; }

    public string? IdTransaccionExterna { get; set; }

    public virtual Reserva IdReservaNavigation { get; set; } = null!;
}

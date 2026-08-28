using System;
using System.Collections.Generic;

namespace EventifyJC.Models;

public partial class Calificacion
{
    public int IdCalificacion { get; set; }

    public int IdReserva { get; set; }

    public int IdOrganizador { get; set; }

    public int Puntuacion { get; set; }

    public string? Comentario { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Usuario IdOrganizadorNavigation { get; set; } = null!;

    public virtual Reserva IdReservaNavigation { get; set; } = null!;
}

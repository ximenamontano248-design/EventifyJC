using System;
using System.Collections.Generic;

namespace EventifyJC.Models;

public partial class Auditorium
{
    public int IdAuditoria { get; set; }

    public string? Tabla { get; set; }

    public string? Operacion { get; set; }

    public int? IdRegistro { get; set; }

    public int? UsuarioAccion { get; set; }

    public DateTime? FechaHora { get; set; }

    public string? DatosAnteriores { get; set; }

    public string? DatosNuevos { get; set; }

    public virtual Usuario? UsuarioAccionNavigation { get; set; }
}

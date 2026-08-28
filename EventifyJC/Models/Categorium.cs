using System;
using System.Collections.Generic;

namespace EventifyJC.Models;

public partial class Categorium
{
    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Link { get; set; }

    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
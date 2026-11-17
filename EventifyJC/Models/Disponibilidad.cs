namespace EventifyJC.Models;

public partial class Disponibilidad
{
    public int IdDisponibilidad { get; set; }
    public int IdServicio { get; set; }
    public DateOnly Fecha { get; set; }
    public bool Disponible { get; set; } = true;
    public string? Motivo { get; set; }

    public virtual Servicio IdServicioNavigation { get; set; } = null!;
}
namespace EventifyJC.Models;

public class CalendarioDisponibilidadViewModel
{
    public int IdServicio { get; set; }
    public string ServicioNombre { get; set; } = "";
    public int Año { get; set; }
    public int Mes { get; set; }
    public List<DateOnly> FechasOcupadas { get; set; } = new();
    public List<DateOnly> FechasReservadas { get; set; } = new();
    public string Motivo { get; set; } = "";
}
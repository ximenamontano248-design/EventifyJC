namespace EventifyJC.Models;

public partial class MatrizPrecio
{
    public int IdMatriz { get; set; }
    public int IdServicio { get; set; }
    public int Cantidad { get; set; }
    public int Horas { get; set; }
    public decimal Precio { get; set; }

    public virtual Servicio IdServicioNavigation { get; set; } = null!;
}
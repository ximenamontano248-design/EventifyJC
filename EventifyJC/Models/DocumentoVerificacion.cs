namespace EventifyJC.Models;

public partial class DocumentoVerificacion
{
    public int IdDocumento { get; set; }
    public int IdProveedor { get; set; }
    public string Tipo { get; set; } = null!;
    public string NombreArchivo { get; set; } = null!;
    public string UrlArchivo { get; set; } = null!;
    public DateTime FechaSubida { get; set; }
    public string Estado { get; set; } = "pendiente";
    public string? Observacion { get; set; }
    public int? RevisadoPor { get; set; }
    public DateTime? FechaRevision { get; set; }

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
    public virtual Usuario? RevisadoPorNavigation { get; set; }
}
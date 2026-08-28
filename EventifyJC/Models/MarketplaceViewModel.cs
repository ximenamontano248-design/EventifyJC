namespace EventifyJC.Models;

public class MarketplaceViewModel
{
    public List<ServiceDto> Servicios { get; set; } = new();
    public List<Categorium> Categorias { get; set; } = new();
    public List<string> Cities { get; set; } = new();
}

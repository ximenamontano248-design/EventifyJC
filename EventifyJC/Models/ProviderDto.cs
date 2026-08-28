namespace EventifyJC.Models;

public class ProviderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? City { get; set; }
    public double Rating { get; set; }
    public int Reviews { get; set; }
    public string Price { get; set; } = "Consultar";
    public string Image { get; set; } = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?auto=format&fit=crop&w=600&q=70";
    public bool Verified { get; set; }
}

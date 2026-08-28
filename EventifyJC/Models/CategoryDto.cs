namespace EventifyJC.Models;

public class CategoryDto
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Link { get; set; }

    // Usadas por HomeController.Index() y Views/Home/Index.cshtml
    public string Slug { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Count { get; set; }
    public string? ColorClass { get; set; }
    public string? IconClass { get; set; }
}
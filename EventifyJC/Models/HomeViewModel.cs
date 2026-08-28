namespace EventifyJC.Models;

public class HomeViewModel
{
    public List<CategoryDto> Categories { get; set; } = new();
    public List<ProviderDto> Providers { get; set; } = new();
}

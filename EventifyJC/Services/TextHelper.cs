using System.Globalization;
using System.Text;

namespace EventifyJC.Helpers;

public static class TextHelper
{
    public static string QuitarTildes(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        var normalized = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
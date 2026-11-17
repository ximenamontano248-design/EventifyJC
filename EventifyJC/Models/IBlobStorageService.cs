namespace EventifyJC.Services;

public interface IBlobStorageService
{
    /// <summary>
    /// Sube un archivo al contenedor indicado y devuelve la URL pública.
    /// </summary>
    Task<string> SubirArchivoAsync(Stream contenido, string nombreArchivo, string contentType, string carpeta);

    /// <summary>
    /// Elimina un archivo del contenedor indicado dado su URL pública.
    /// </summary>
    Task EliminarArchivoAsync(string urlArchivo);
}
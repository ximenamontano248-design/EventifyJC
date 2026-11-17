using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace EventifyJC.Services;

public class AzureStorageSettings
{
    public string ConnectionString { get; set; } = "";
    public string ContainerDocumentos { get; set; } = "documentos";
    public string ContainerLogos { get; set; } = "logos";
    public string ContainerServicios { get; set; } = "servicios";
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageSettings _settings;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        IOptions<AzureStorageSettings> settings,
        ILogger<BlobStorageService> logger)
    {
        _settings = settings.Value;
        _blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
        _logger = logger;
    }

    public async Task<string> SubirArchivoAsync(Stream contenido, string nombreArchivo, string contentType, string carpeta)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(carpeta);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Nombre único para evitar colisiones
            var ext = Path.GetExtension(nombreArchivo);
            var nombreUnico = $"{Guid.NewGuid()}{ext}";
            var blobClient = containerClient.GetBlobClient(nombreUnico);

            await blobClient.UploadAsync(contenido, new BlobHttpHeaders
            {
                ContentType = contentType
            });

            _logger.LogInformation("Archivo subido: {Url}", blobClient.Uri);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subiendo archivo a Azure Blob");
            throw;
        }
    }

    public async Task EliminarArchivoAsync(string urlArchivo)
    {
        try
        {
            if (string.IsNullOrEmpty(urlArchivo)) return;

            var uri = new Uri(urlArchivo);
            var nombreBlob = Path.GetFileName(uri.LocalPath);
            var segmentos = uri.Segments;
            var carpeta = segmentos.Length > 1 ? segmentos[1].TrimEnd('/') : "";

            var containerClient = _blobServiceClient.GetBlobContainerClient(carpeta);
            var blobClient = containerClient.GetBlobClient(nombreBlob);

            await blobClient.DeleteIfExistsAsync();
            _logger.LogInformation("Archivo eliminado: {Url}", urlArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error eliminando archivo: {Url}", urlArchivo);
        }
    }
}
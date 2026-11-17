namespace EventifyJC.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);
    Task SendPasswordResetEmailAsync(string toEmail, string toName, string temporaryPassword);

    Task SendNuevaCotizacionEmailAsync(
        string toEmail,
        string toName,
        string organizadorNombre,
        string organizadorEmail,
        string organizadorTelefono,
        string servicioNombre,
        string categoriaNombre,
        DateOnly fechaEvento,
        string descripcionEvento,
        decimal? presupuestoEstimado,
        decimal precioBase);
}
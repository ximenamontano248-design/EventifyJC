using EventifyJC.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EventifyJC.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 15000;

            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect);

            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Correo enviado a {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo a {Email}", toEmail);
            throw;
        }
    }

    // =========================================================
    // RECUPERACIÓN DE CONTRASEÑA
    // =========================================================
    public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string temporaryPassword)
    {
        var subject = "Eventify JC - Tu contraseña temporal";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; background-color: #f8fafc; padding: 20px; margin:0;'>
    <div style='max-width: 500px; margin: 0 auto; background: #ffffff; border-radius: 16px; padding: 32px; box-shadow: 0 1px 3px rgba(0,0,0,0.1);'>
        <div style='text-align: center; margin-bottom: 24px;'>
            <div style='display: inline-block; background: #1b2340; color: #e8823c; padding: 12px 16px; border-radius: 12px; font-size: 20px;'>&#10022;</div>
            <h1 style='color: #1b2340; font-size: 24px; margin: 16px 0 8px;'>Eventify JC</h1>
        </div>

        <h2 style='color: #1b2340; font-size: 20px; margin-bottom: 16px;'>Recuperación de contraseña</h2>

        <p style='color: #6b7080; font-size: 14px; line-height: 1.6;'>Hola <strong>{toName}</strong>,</p>

        <p style='color: #6b7080; font-size: 14px; line-height: 1.6;'>
            Hemos recibido una solicitud para restablecer tu contraseña. Tu contraseña temporal es:
        </p>

        <div style='background: #fff7ed; border: 2px dashed #e8823c; border-radius: 12px; padding: 20px; text-align: center; margin: 24px 0;'>
            <span style='font-family: monospace; font-size: 24px; font-weight: bold; color: #1b2340; letter-spacing: 2px;'>
                {temporaryPassword}
            </span>
        </div>

        <p style='color: #6b7080; font-size: 14px; line-height: 1.6;'>
            Por seguridad, te recomendamos <strong>cambiar esta contraseña</strong> una vez inicies sesión.
        </p>

        <div style='text-align: center; margin: 32px 0;'>
            <a href='https://eventifyjc-a5fxehe7g9babpba.westus3-01.azurewebsites.net/Account/Login'
               style='background: #e8823c; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 12px; font-weight: bold; display: inline-block;'>
                Iniciar sesión
            </a>
        </div>

        <p style='color: #9ca3af; font-size: 12px; line-height: 1.6; text-align: center; border-top: 1px solid #e6e5e0; padding-top: 16px; margin-top: 24px;'>
            Si no solicitaste este cambio, ignora este correo.<br>
            &copy; 2026 Eventify JC &middot; Proyecto académico
        </p>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, toName, subject, htmlBody);
    }

    public async Task SendNuevaCotizacionEmailAsync(
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
        decimal precioBase)
    {
        var subject = $"Nueva solicitud de cotización - {servicioNombre}";

        var presupuestoTexto = presupuestoEstimado.HasValue
            ? $"$ {presupuestoEstimado.Value.ToString("N0")} COP"
            : "No especificado";

        var telefonoTexto = string.IsNullOrWhiteSpace(organizadorTelefono)
            ? "No especificado"
            : organizadorTelefono;

        var fechaTexto = fechaEvento.ToString("dddd, dd 'de' MMMM 'de' yyyy",
            new System.Globalization.CultureInfo("es-CO"));

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; background-color: #f8fafc; padding: 20px; margin: 0;'>
    <div style='max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 16px; padding: 32px; box-shadow: 0 1px 3px rgba(0,0,0,0.1);'>

        <!-- Header -->
        <div style='text-align: center; margin-bottom: 24px;'>
            <div style='display: inline-block; background: #1b2340; color: #e8823c; padding: 12px 16px; border-radius: 12px; font-size: 20px;'>&#10022;</div>
            <h1 style='color: #1b2340; font-size: 24px; margin: 16px 0 8px;'>Eventify JC</h1>
            <p style='color: #6b7080; font-size: 14px; margin: 0;'>Plataforma de intermediación de servicios para eventos</p>
        </div>

        <!-- Saludo -->
        <h2 style='color: #1b2340; font-size: 20px; margin-bottom: 16px;'>¡Hola {toName}!</h2>

        <p style='color: #6b7080; font-size: 15px; line-height: 1.6;'>
            Has recibido una <strong>nueva solicitud de cotización</strong> a través de Eventify JC.
            Revisa los detalles y responde lo antes posible.
        </p>

        <!-- Info del servicio -->
        <div style='background: #fff7ed; border-left: 4px solid #e8823c; border-radius: 8px; padding: 16px; margin: 24px 0;'>
            <p style='margin: 0; font-size: 12px; color: #6b7080; text-transform: uppercase; letter-spacing: 1px;'>Servicio solicitado</p>
            <p style='margin: 4px 0 0; font-size: 18px; font-weight: bold; color: #1b2340;'>{servicioNombre}</p>
            <p style='margin: 4px 0 0; font-size: 13px; color: #6b7080;'>Categoría: {categoriaNombre}</p>
            <p style='margin: 8px 0 0; font-size: 14px; color: #e8823c; font-weight: bold;'>
                Precio base: $ {precioBase.ToString("N0")} COP
            </p>
        </div>

        <!-- Info del organizador -->
        <h3 style='color: #1b2340; font-size: 16px; margin-top: 24px; margin-bottom: 12px; border-bottom: 1px solid #e6e5e0; padding-bottom: 8px;'>
            Datos del organizador
        </h3>
        <table style='width: 100%; font-size: 14px; color: #1b2340; border-collapse: collapse;'>
            <tr>
                <td style='padding: 6px 0; color: #6b7080; width: 130px;'>Nombre:</td>
                <td style='padding: 6px 0; font-weight: 600;'>{organizadorNombre}</td>
            </tr>
            <tr>
                <td style='padding: 6px 0; color: #6b7080;'>Correo:</td>
                <td style='padding: 6px 0;'>
                    <a href='mailto:{organizadorEmail}' style='color: #e8823c; text-decoration: none;'>{organizadorEmail}</a>
                </td>
            </tr>
            <tr>
                <td style='padding: 6px 0; color: #6b7080;'>Teléfono:</td>
                <td style='padding: 6px 0;'>{telefonoTexto}</td>
            </tr>
        </table>

        <!-- Info del evento -->
        <h3 style='color: #1b2340; font-size: 16px; margin-top: 24px; margin-bottom: 12px; border-bottom: 1px solid #e6e5e0; padding-bottom: 8px;'>
            Detalles del evento
        </h3>
        <table style='width: 100%; font-size: 14px; color: #1b2340; border-collapse: collapse;'>
            <tr>
                <td style='padding: 6px 0; color: #6b7080; width: 130px;'>Fecha:</td>
                <td style='padding: 6px 0; font-weight: 600;'>{fechaTexto}</td>
            </tr>
            <tr>
                <td style='padding: 6px 0; color: #6b7080;'>Presupuesto:</td>
                <td style='padding: 6px 0; font-weight: 600; color: #e8823c;'>{presupuestoTexto}</td>
            </tr>
        </table>

        <!-- Descripción -->
        <div style='margin-top: 16px;'>
            <p style='font-size: 13px; color: #6b7080; margin-bottom: 6px;'>Descripción del evento:</p>
            <div style='background: #f8fafc; border-radius: 8px; padding: 14px; font-size: 14px; color: #1b2340; line-height: 1.6;'>
                {descripcionEvento}
            </div>
        </div>

        <!-- CTA -->
        <div style='text-align: center; margin: 32px 0 16px;'>
            <a href='https://eventifyjc-a5fxehe7g9babpba.westus3-01.azurewebsites.net/Proveedor/Cotizaciones'
               style='background: #e8823c; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 12px; font-weight: bold; display: inline-block;'>
                Ver cotizaciones en Eventify
            </a>
        </div>

        <!-- Footer -->
        <p style='color: #9ca3af; font-size: 12px; line-height: 1.6; text-align: center; border-top: 1px solid #e6e5e0; padding-top: 16px; margin-top: 24px;'>
            Recibiste este correo porque tienes un servicio publicado en Eventify JC.<br>
            &copy; 2026 Eventify JC &middot; Proyecto académico
        </p>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, toName, subject, htmlBody);
    }
}
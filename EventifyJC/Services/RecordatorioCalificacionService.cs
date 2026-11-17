using EventifyJC.Models;
using Microsoft.EntityFrameworkCore;

namespace EventifyJC.Services;

public class RecordatorioCalificacionService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<RecordatorioCalificacionService> _logger;

    public RecordatorioCalificacionService(
        IServiceProvider services,
        ILogger<RecordatorioCalificacionService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var ayer = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

                var reservas = await context.Reservas
                    .Include(r => r.IdCotizacionNavigation)
                        .ThenInclude(c => c.IdSolicitudNavigation)
                            .ThenInclude(s => s.IdOrganizadorNavigation)
                    .Include(r => r.IdCotizacionNavigation)
                        .ThenInclude(c => c.IdServicioNavigation)
                    .Include(r => r.Calificacions)
                    .Where(r => r.Estado == "finalizada"
                             && r.IdCotizacionNavigation.IdSolicitudNavigation.FechaEvento == ayer
                             && !r.Calificacions.Any())
                    .ToListAsync(stoppingToken);

                _logger.LogInformation("Recordatorio: {Count} reservas para notificar", reservas.Count);

                foreach (var r in reservas)
                {
                    var organizador = r.IdCotizacionNavigation?.IdSolicitudNavigation?.IdOrganizadorNavigation;
                    var servicio = r.IdCotizacionNavigation?.IdServicioNavigation?.Nombre ?? "el servicio";

                    if (organizador == null || string.IsNullOrEmpty(organizador.Email))
                        continue;

                    try
                    {
                        var html = $@"
                        <!DOCTYPE html>
                        <html>
                        <head><meta charset='utf-8'></head>
                        <body style='font-family: Arial, sans-serif; background: #f8fafc; padding: 20px; margin: 0;'>
                            <div style='max-width: 500px; margin: 0 auto; background: #ffffff; border-radius: 16px; padding: 32px; box-shadow: 0 1px 3px rgba(0,0,0,0.1);'>
                                <div style='text-align: center; margin-bottom: 24px;'>
                                    <div style='display: inline-block; background: #1b2340; color: #e8823c; padding: 12px 16px; border-radius: 12px; font-size: 20px;'>&#10022;</div>
                                    <h1 style='color: #1b2340; font-size: 24px; margin: 16px 0 8px;'>Eventify JC</h1>
                                </div>

                                <h2 style='color: #1b2340; font-size: 20px; margin-bottom: 16px;'>
                                    ¿Cómo te fue en tu evento?
                                </h2>

                                <p style='color: #6b7080; font-size: 14px; line-height: 1.6;'>
                                    Hola <strong>{organizador.Nombre}</strong>,
                                </p>

                                <p style='color: #6b7080; font-size: 14px; line-height: 1.6;'>
                                    Esperamos que hayas disfrutado el servicio de <strong>{servicio}</strong>.
                                    Tu opinión es muy valiosa para ayudar a otros organizadores.
                                </p>

                                <div style='background: #fff7ed; border-left: 4px solid #e8823c; border-radius: 8px; padding: 16px; margin: 24px 0; text-align: center;'>
                                    <p style='margin: 0; font-size: 14px; color: #1b2340;'>
                                        <strong>¿Nos regalas 1 minuto?</strong>
                                    </p>
                                    <p style='margin: 8px 0 0; font-size: 13px; color: #6b7080;'>
                                        Califica el servicio y comparte tu experiencia.
                                    </p>
                                </div>

                                <div style='text-align: center; margin: 32px 0;'>
                                    <a href='https://eventifyjc-a5fxehe7g9babpba.westus3-01.azurewebsites.net/Organizador/MisReservas'
                                       style='background: #e8823c; color: #ffffff; text-decoration: none; padding: 14px 32px; border-radius: 12px; font-weight: bold; display: inline-block;'>
                                        Calificar ahora
                                    </a>
                                </div>

                                <p style='color: #9ca3af; font-size: 12px; line-height: 1.6; text-align: center; border-top: 1px solid #e6e5e0; padding-top: 16px; margin-top: 24px;'>
                                    Si ya calificaste el servicio, ignora este correo.<br>
                                    &copy; 2026 Eventify JC
                                </p>
                            </div>
                        </body>
                        </html>";

                        await email.SendEmailAsync(
                            organizador.Email,
                            organizador.Nombre,
                            $"¿Cómo te fue en tu evento con {servicio}?",
                            html);

                        _logger.LogInformation("Recordatorio enviado a {Email}", organizador.Email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error enviando recordatorio a {Email}", organizador.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en RecordatorioCalificacionService");
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}
using System.Globalization;
using EventifyJC.Identity;
using EventifyJC.Models;
using EventifyJC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Cultura español Colombia
// ============================================================
var cultureInfo = new CultureInfo("es-CO");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// ============================================================
// Cadena de conexión
// Se sube el Connect Timeout a 60 s para tolerar bases de datos
// que tardan en "despertar" (Azure SQL Serverless con auto-pause).
// ============================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    var csBuilder = new SqlConnectionStringBuilder(connectionString);
    if (csBuilder.ConnectTimeout < 60)
        csBuilder.ConnectTimeout = 60;
    connectionString = csBuilder.ConnectionString;
}

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        // Reintentos automáticos ante fallos transitorios de conexión
        sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);

        sql.CommandTimeout(60);
    }));

// ============================================================
// Identity
// ============================================================
builder.Services
    .AddIdentityCore<Usuario>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddUserStore<UsuarioStore>()
    .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// ============================================================
// Email
// ============================================================
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// ============================================================
// Azure Blob Storage
// ============================================================
builder.Services.Configure<AzureStorageSettings>(builder.Configuration.GetSection("AzureStorage"));
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// ============================================================
// Hosted Service: recordatorio de calificación
// ============================================================
builder.Services.AddHostedService<RecordatorioCalificacionService>();

var app = builder.Build();

// ============================================================
// Middleware de cultura
// ============================================================
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-CO"),
    SupportedCultures = new[] { cultureInfo },
    SupportedUICultures = new[] { cultureInfo }
});

// ============================================================
// LOG TEMPORAL (seguro): solo muestra servidor y base de datos,
// nunca usuario ni contraseña. Quitar cuando termines de depurar.
// ============================================================
if (string.IsNullOrWhiteSpace(connectionString))
{
    app.Logger.LogWarning("CONEXION SQL EN USO: NULL o vacía");
}
else
{
    var csInfo = new SqlConnectionStringBuilder(connectionString);
    app.Logger.LogInformation(
        "CONEXION SQL EN USO: Servidor={Servidor} Base={Base} ConnectTimeout={Timeout}",
        csInfo.DataSource, csInfo.InitialCatalog, csInfo.ConnectTimeout);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "marketplace",
    pattern: "marketplace",
    defaults: new { controller = "Explorador", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
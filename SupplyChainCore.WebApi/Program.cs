using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SupplyChainCore.Application.Interfaces;
using SupplyChainCore.Application.Services;
using SupplyChainCore.Infrastructure.Persistence;
using SupplyChainCore.Infrastructure.Persistence.Repositories;
using SupplyChainCore.Infrastructure.Security;
using SupplyChainCore.WebApi.OpenApi;
// 🔑 Nuevas directivas para reconocer el motor analítico de KPIs
using SupplyChainCore.Application.Analytics;
using SupplyChainCore.Infrastructure.Analytics;

var builder = WebApplication.CreateBuilder(args);

// 🔑 1. DEFINIR LA POLÍTICA DE CORS (Permitir que React se conecte)
// Los orígenes salen de configuración para no tener que recompilar al desplegar.
string[] origenesPermitidos = builder.Configuration
    .GetSection("Cors:OrigenesPermitidos")
    .Get<string[]>() ?? ["http://localhost:5173", "http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(origenesPermitidos)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Inyección de Dependencias - Base de Datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔐 2. AUTENTICACIÓN JWT
// La clave se lee de configuración: en local desde appsettings.Development.json o
// user-secrets, en producción desde la variable de entorno Jwt__Key.
var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SeccionConfig)
    .Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    throw new InvalidOperationException(
        "Falta la clave de firma JWT. Configure 'Jwt:Key' (o la variable de entorno Jwt__Key) antes de iniciar la API.");
}

builder.Services.AddSingleton(jwtOptions);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            // Por defecto .NET tolera 5 minutos de desfase de reloj: un token
            // expirado seguiría siendo aceptado durante ese margen.
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Repositorios y Servicios Transaccionales (Módulo Ledger Logístico)
builder.Services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
builder.Services.AddScoped<IMovimientoService, MovimientoService>();

// 🔐 Servicios de identidad
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 📊 Registro de Inyección de Dependencias para el Dashboard Analítico
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    // Declara el esquema Bearer en el documento OpenAPI para que quien lo lea
    // sepa que los endpoints van autenticados y con qué formato de cabecera.
    options.AddDocumentTransformer<SeguridadBearerTransformer>();
});

var app = builder.Build();

// Aplica las migraciones pendientes al arrancar. Se activa con Database:AutoMigrate
// y existe para el contenedor, donde no hay una consola donde correr `dotnet ef`.
// Queda desactivado por defecto: en un entorno serio las migraciones son un paso
// deliberado del despliegue, no un efecto secundario de encender la aplicación.
if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// El documento OpenAPI y su interfaz se publican en todos los entornos, no solo
// en desarrollo: son la documentación pública de la API, no una ayuda de
// depuración. Ambos endpoints son anónimos a propósito — describen la forma de
// la API, no exponen datos — mientras que cada operación sigue exigiendo token.
app.MapOpenApi();

// Servida en la raíz, la interfaz de Swagger responde a "/" con un 301 hacia
// "index.html" — un destino relativo. Los navegadores lo resuelven sin problema,
// pero varios rastreadores lo rechazan y reportan la URL como inalcanzable.
// Reescribimos la ruta internamente para devolver 200 directo, sin redirección.
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Request.Path = "/index.html";
    }

    await next();
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "SupplyChainCore API v1");
    options.DocumentTitle = "SupplyChainCore API";
    // Servida en la raíz: quien abra la URL del despliegue aterriza en la
    // documentación en vez de recibir un 404.
    options.RoutePrefix = string.Empty;

    // La página de Swagger es un esqueleto que se rellena por JavaScript: sin
    // estas etiquetas, un rastreador social solo ve un documento vacío y se
    // niega a generar la vista previa del enlace. Se inyectan en el <head> para
    // que compartir esta URL muestre una tarjeta con título y descripción.
    options.HeadContent = """
        <meta name="description" content="API REST de inventarios en .NET 10: ledger inmutable, Clean Architecture, JWT con autorizacion por roles y 57 pruebas unitarias. Documentacion publica: pruebala desde el navegador.">
        <meta property="og:type" content="website">
        <meta property="og:site_name" content="Jose Luis Monteza">
        <meta property="og:title" content="SupplyChainCore API - inventarios en .NET 10 sobre Azure">
        <meta property="og:description" content="Ledger inmutable, Clean Architecture y JWT + RBAC. Entra con la cuenta de solo lectura (operador@supplychain.com / Operador123!) e intenta escribir: responde 403.">
        <meta property="og:url" content="https://supplychaincore-api-lnxj7c.azurewebsites.net">
        <meta property="og:image" content="https://avatars.githubusercontent.com/u/272381527?v=4">
        <meta name="twitter:card" content="summary">
        """;
});

app.UseHttpsRedirection();

// 🔑 3. ACTIVAR CORS EN EL PIPELINE (Debe ir estrictamente antes de Authorization)
app.UseCors("AllowReactApp");

// El orden importa: primero se resuelve QUIÉN es el llamante (Authentication),
// solo después se decide si PUEDE hacer la operación (Authorization).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Expuesto para que el proyecto de tests pueda referenciar el host si se añaden
// pruebas de integración con WebApplicationFactory.
public partial class Program { }

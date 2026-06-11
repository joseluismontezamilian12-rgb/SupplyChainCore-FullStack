using Microsoft.EntityFrameworkCore;
using SupplyChainCore.Application.Interfaces;
using SupplyChainCore.Application.Services;
using SupplyChainCore.Infrastructure.Persistence;
using SupplyChainCore.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 🔑 1. DEFINIR LA POLÍTICA DE CORS (Permitir que React se conecte)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Puertos estándar de Vite y React
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Inyección de Dependencias
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
builder.Services.AddScoped<IMovimientoService, MovimientoService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 🔑 2. ACTIVAR CORS EN EL PIPELINE (Debe ir estrictamente antes de Authorization)
app.UseCors("AllowReactApp");

app.UseAuthorization();
app.MapControllers();

app.Run();
using System; // 🔑 CONEXIÓN CLAVE: Necesario para que compile DateTime y DateTimeKind
using Microsoft.EntityFrameworkCore;
using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Mapeo de nuestras Entidades a Tablas
    public DbSet<Rol> Roles { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Producto> Productos { get; set; } = null!;
    public DbSet<Almacen> Almacenes { get; set; } = null!;
    public DbSet<MovimientoInventario> MovimientosInventario { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración para el precio
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(p => p.PrecioUnitario)
                  .HasColumnType("decimal(18,2)");
        });

        // Configuración de las relaciones jerárquicas del Ledger
        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasOne(m => m.Producto)
                  .WithMany(p => p.Movimientos)
                  .HasForeignKey(m => m.ProductoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Almacen)
                  .WithMany(a => a.Movimientos)
                  .HasForeignKey(m => m.AlmacenId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Usuario)
                  .WithMany()
                  .HasForeignKey(m => m.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 🔑 DATA SEEDING: Poblamos la base de datos con los registros ID = 1 necesarios
        modelBuilder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = "Admin", Descripcion = "Administrador del Sistema" }
        );

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { Id = 1, NombreCompleto = "Jose Luis Monteza", Email = "jose@supplychain.com", PasswordHash = "hashed_password", RolId = 1, FechaCreacion = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Tecnología", Activo = true }
        );

        modelBuilder.Entity<Almacen>().HasData(
            new Almacen { Id = 1, Nombre = "Almacén Central Lima", Ubicacion = "Sede Principal" }
        );

        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, CodigoSku = "LAP-LOQ-01", Nombre = "Lenovo LOQ Laptop", Descripcion = "Core i7 - 16GB RAM", PrecioUnitario = 3500.00m, StockMinimo = 5, CategoriaId = 1 }
        );
    }
}
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

        // El email es la credencial de login: debe ser único a nivel de base de
        // datos, no solo por convención de la capa de aplicación.
        modelBuilder.Entity<Usuario>()
                    .HasIndex(u => u.Email)
                    .IsUnique();

        // 🔑 DATA SEEDING: Poblamos la base de datos con los registros ID = 1 necesarios
        modelBuilder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = RolesDelSistema.Admin, Descripcion = "Administrador del Sistema: puede registrar movimientos en el ledger" },
            new Rol { Id = 2, Nombre = RolesDelSistema.Operador, Descripcion = "Operador: acceso de solo lectura a stock, historial y KPIs" }
        );

        // Hashes PBKDF2-HMAC-SHA256 (100.000 iteraciones, sal por usuario) generados
        // con el mismo formato que produce Pbkdf2PasswordHasher. Son credenciales de
        // demostración para levantar el proyecto en local — en un despliegue real
        // estas cuentas se crean fuera de la migración.
        // jose@supplychain.com     → Admin123!
        // operador@supplychain.com → Operador123!
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                NombreCompleto = "Jose Luis Monteza",
                Email = "jose@supplychain.com",
                PasswordHash = "100000.P3ocnlstSKbA4fS3jSNZrg==.tFxScx1pDVYzd6/qJok/Te3DERUxXxr5c4EGiHOby0U=",
                RolId = 1,
                FechaCreacion = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc)
            },
            new Usuario
            {
                Id = 2,
                NombreCompleto = "Operador de Almacén",
                Email = "operador@supplychain.com",
                PasswordHash = "100000.obLD1OX2BxgpOktcbX6PkA==.GL3wW8VDG1K7qiEzAaCmF0xTdJ60eJshIbcUEvygKG0=",
                RolId = 2,
                FechaCreacion = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc)
            }
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
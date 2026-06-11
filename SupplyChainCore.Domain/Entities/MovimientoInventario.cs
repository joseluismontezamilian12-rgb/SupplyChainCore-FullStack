namespace SupplyChainCore.Domain.Entities;

public class MovimientoInventario
{
    public long Id { get; set; } // 'long' en C# mapea perfectamente a 'BIGINT' en SQL Server
    public int ProductoId { get; set; }
    public int AlmacenId { get; set; }
    public int UsuarioId { get; set; }
    public int Cantidad { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty; // 'INGRESO', 'SALIDA' o 'MERMA'
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaTransaccion { get; set; }

    // Propiedades de navegación orientadas a objetos
    public Producto Producto { get; set; } = null!;
    public Almacen Almacen { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
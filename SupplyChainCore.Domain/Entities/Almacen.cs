namespace SupplyChainCore.Domain.Entities;

public class Almacen
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }

    // Propiedad de navegación (Un almacén tiene muchos movimientos históricos)
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
}
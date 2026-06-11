namespace SupplyChainCore.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
    public string CodigoSku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int StockMinimo { get; set; } = 5;
    public int CategoriaId { get; set; }

    // Propiedades de navegación relacionales
    public Categoria Categoria { get; set; } = null!;
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
}
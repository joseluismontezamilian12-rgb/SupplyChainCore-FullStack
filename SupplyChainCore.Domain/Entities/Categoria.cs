namespace SupplyChainCore.Domain.Entities;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    // Propiedad de navegación (Una categoría tiene muchos productos)
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
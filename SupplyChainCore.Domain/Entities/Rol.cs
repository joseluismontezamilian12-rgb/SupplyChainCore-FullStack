namespace SupplyChainCore.Domain.Entities;

public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    // Propiedad de navegación (Un Rol tiene muchos Usuarios)
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
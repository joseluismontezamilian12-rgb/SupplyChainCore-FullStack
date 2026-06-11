namespace SupplyChainCore.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RolId { get; set; }
    public DateTime FechaCreacion { get; set; }

    // Propiedad de navegación (Un usuario pertenece a un Rol)
    public Rol Rol { get; set; } = null!;
}
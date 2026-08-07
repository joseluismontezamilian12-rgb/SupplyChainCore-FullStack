namespace SupplyChainCore.Application.DTOs;

/// <summary>
/// Resultado de un login exitoso. No expone la entidad Usuario para que el
/// PasswordHash no pueda salir por la API ni por accidente.
/// </summary>
public record AuthResultado(
    string Token,
    DateTime ExpiraUtc,
    string NombreCompleto,
    string Email,
    string Rol
);

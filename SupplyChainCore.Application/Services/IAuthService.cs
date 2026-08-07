using SupplyChainCore.Application.DTOs;

namespace SupplyChainCore.Application.Services;

public interface IAuthService
{
    /// <summary>
    /// Valida credenciales y emite un token. Devuelve null si el email no existe
    /// o la contraseña no coincide — deliberadamente el mismo resultado en ambos
    /// casos, para no revelar qué emails están registrados.
    /// </summary>
    Task<AuthResultado?> LoginAsync(string email, string password);
}

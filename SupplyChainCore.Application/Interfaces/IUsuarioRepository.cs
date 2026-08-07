using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.Application.Interfaces;

public interface IUsuarioRepository
{
    /// <summary>
    /// Recupera un usuario por su email incluyendo su Rol.
    /// Devuelve null si no existe: la decisión de cómo responder ante un email
    /// desconocido pertenece a la capa de aplicación, no al repositorio.
    /// </summary>
    Task<Usuario?> ObtenerPorEmailAsync(string email);
}

using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.Application.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Emite un JWT firmado con los claims de identidad y rol del usuario.
    /// Devuelve también el instante exacto de expiración para que el cliente
    /// no tenga que decodificar el token solo para saber cuándo renovar.
    /// </summary>
    (string Token, DateTime ExpiraUtc) GenerarToken(Usuario usuario);
}

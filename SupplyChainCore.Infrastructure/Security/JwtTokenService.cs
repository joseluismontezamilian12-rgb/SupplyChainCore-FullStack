using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SupplyChainCore.Application.Interfaces;
using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.Infrastructure.Security;

/// <summary>
/// Emite JWT firmados con HMAC-SHA256. El rol viaja como ClaimTypes.Role, que es
/// exactamente el claim que lee [Authorize(Roles = "...")] — así la autorización
/// del endpoint se resuelve contra el token, sin volver a consultar la base.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _opciones;

    public JwtTokenService(JwtOptions opciones)
    {
        if (string.IsNullOrWhiteSpace(opciones.Key))
        {
            throw new InvalidOperationException(
                "La clave de firma JWT no está configurada (sección 'Jwt:Key').");
        }

        // HS256 exige una clave de al menos 256 bits; una clave corta produce un
        // token que parece válido pero es trivial de forzar, así que se corta aquí.
        if (Encoding.UTF8.GetByteCount(opciones.Key) < 32)
        {
            throw new InvalidOperationException(
                "La clave de firma JWT debe tener al menos 32 bytes para HMAC-SHA256.");
        }

        _opciones = opciones;
    }

    public (string Token, DateTime ExpiraUtc) GenerarToken(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var expiraUtc = DateTime.UtcNow.AddMinutes(_opciones.ExpiraEnMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Role, usuario.Rol?.Nombre ?? string.Empty)
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraUtc,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraUtc);
    }
}

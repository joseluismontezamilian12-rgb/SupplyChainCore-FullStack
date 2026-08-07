using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SupplyChainCore.Domain.Entities;
using SupplyChainCore.Infrastructure.Security;

namespace SupplyChainCore.UnitTests;

public class JwtTokenServiceTests
{
    private const string ClaveValida = "clave-de-pruebas-suficientemente-larga-para-hmac-sha256";

    private static JwtOptions OpcionesValidas(int expiraEnMinutos = 60) => new()
    {
        Key = ClaveValida,
        Issuer = "SupplyChainCore.WebApi",
        Audience = "SupplyChainCore.Client",
        ExpiraEnMinutos = expiraEnMinutos
    };

    private static Usuario UsuarioDePrueba(string rol = RolesDelSistema.Admin) => new()
    {
        Id = 42,
        NombreCompleto = "Jose Luis Monteza",
        Email = "jose@supplychain.com",
        RolId = 1,
        Rol = new Rol { Id = 1, Nombre = rol }
    };

    [Theory]
    [InlineData(RolesDelSistema.Admin)]
    [InlineData(RolesDelSistema.Operador)]
    public void GenerarToken_DeberiaIncluirElRolComoClaimDeRol(string rol)
    {
        // [Authorize(Roles = "...")] lee exactamente ClaimTypes.Role. Si el rol
        // viajara con otro nombre, la autorización dejaría pasar a cualquiera.
        var servicio = new JwtTokenService(OpcionesValidas());

        var (token, _) = servicio.GenerarToken(UsuarioDePrueba(rol));

        var leido = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(rol, leido.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerarToken_DeberiaIncluirElIdDelUsuarioComoNameIdentifier()
    {
        // El controlador de movimientos toma la autoría de este claim: si falta,
        // no se puede atribuir quién movió el inventario.
        var servicio = new JwtTokenService(OpcionesValidas());

        var (token, _) = servicio.GenerarToken(UsuarioDePrueba());

        var leido = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("42", leido.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }

    [Fact]
    public void GenerarToken_DeberiaProducirUnTokenQueValidaContraLaMismaClave()
    {
        var opciones = OpcionesValidas();
        var servicio = new JwtTokenService(opciones);

        var (token, _) = servicio.GenerarToken(UsuarioDePrueba());

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = opciones.Issuer,
            ValidAudience = opciones.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opciones.Key)),
            ClockSkew = TimeSpan.Zero
        }, out _);

        Assert.True(principal.Identity?.IsAuthenticated);
        Assert.True(principal.IsInRole(RolesDelSistema.Admin));
    }

    [Fact]
    public void GenerarToken_DeberiaProducirUnTokenQueFallaContraOtraClave()
    {
        // La prueba de que la firma sirve de algo: un token emitido con otra clave
        // debe ser rechazado, no simplemente "parecer" distinto.
        var servicio = new JwtTokenService(OpcionesValidas());
        var (token, _) = servicio.GenerarToken(UsuarioDePrueba());

        var claveAtacante = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("una-clave-completamente-distinta-de-32-bytes+"));

        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = claveAtacante
            }, out _));
    }

    [Fact]
    public void GenerarToken_DeberiaExpirarSegunLasOpcionesConfiguradas()
    {
        var servicio = new JwtTokenService(OpcionesValidas(expiraEnMinutos: 30));

        var (_, expiraUtc) = servicio.GenerarToken(UsuarioDePrueba());

        Assert.InRange(expiraUtc,
            DateTime.UtcNow.AddMinutes(29),
            DateTime.UtcNow.AddMinutes(31));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_DeberiaFallarSiNoHayClaveConfigurada(string claveVacia)
    {
        var opciones = OpcionesValidas();
        opciones.Key = claveVacia;

        var ex = Assert.Throws<InvalidOperationException>(() => new JwtTokenService(opciones));
        Assert.Contains("no está configurada", ex.Message);
    }

    [Fact]
    public void Constructor_DeberiaRechazarUnaClaveDemasiadoCortaParaHmacSha256()
    {
        // Arrancar con una clave débil es peor que no arrancar: el token parece
        // legítimo pero la firma es forzable.
        var opciones = OpcionesValidas();
        opciones.Key = "clave-corta";

        var ex = Assert.Throws<InvalidOperationException>(() => new JwtTokenService(opciones));
        Assert.Contains("32 bytes", ex.Message);
    }

    [Fact]
    public void GenerarToken_DeberiaFallarSiElUsuarioEsNulo()
    {
        var servicio = new JwtTokenService(OpcionesValidas());

        Assert.Throws<ArgumentNullException>(() => servicio.GenerarToken(null!));
    }
}

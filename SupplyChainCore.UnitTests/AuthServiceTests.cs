using Moq;
using SupplyChainCore.Application.Interfaces;
using SupplyChainCore.Application.Services;
using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly AuthService _servicio;

    private static readonly Usuario UsuarioAdmin = new()
    {
        Id = 1,
        NombreCompleto = "Jose Luis Monteza",
        Email = "jose@supplychain.com",
        PasswordHash = "100000.sal.hash",
        RolId = 1,
        Rol = new Rol { Id = 1, Nombre = RolesDelSistema.Admin }
    };

    public AuthServiceTests()
    {
        _servicio = new AuthService(
            _usuarioRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);
    }

    private void ConUsuarioRegistrado(Usuario usuario) =>
        _usuarioRepositoryMock
            .Setup(r => r.ObtenerPorEmailAsync(usuario.Email))
            .ReturnsAsync(usuario);

    private void ConContraseñaValida(bool esValida) =>
        _passwordHasherMock
            .Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(esValida);

    private void ConTokenEmitido(string token, DateTime expiraUtc) =>
        _tokenServiceMock
            .Setup(t => t.GenerarToken(It.IsAny<Usuario>()))
            .Returns((token, expiraUtc));

    [Fact]
    public async Task Login_DeberiaDevolverTokenYDatosDelUsuario_CuandoLasCredencialesSonValidas()
    {
        var expira = DateTime.UtcNow.AddHours(1);
        ConUsuarioRegistrado(UsuarioAdmin);
        ConContraseñaValida(true);
        ConTokenEmitido("token-firmado", expira);

        var resultado = await _servicio.LoginAsync("jose@supplychain.com", "Admin123!");

        Assert.NotNull(resultado);
        Assert.Equal("token-firmado", resultado!.Token);
        Assert.Equal(expira, resultado.ExpiraUtc);
        Assert.Equal("Jose Luis Monteza", resultado.NombreCompleto);
        Assert.Equal("jose@supplychain.com", resultado.Email);
        Assert.Equal(RolesDelSistema.Admin, resultado.Rol);
    }

    [Fact]
    public async Task Login_DeberiaDevolverNull_CuandoElEmailNoExiste()
    {
        _usuarioRepositoryMock
            .Setup(r => r.ObtenerPorEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);

        var resultado = await _servicio.LoginAsync("desconocido@supplychain.com", "loQueSea123!");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Login_DeberiaVerificarUnHashSeñuelo_CuandoElEmailNoExiste()
    {
        // Mitigación de enumeración de usuarios: si solo se hiciera el trabajo
        // criptográfico cuando el email existe, medir el tiempo de respuesta
        // revelaría qué cuentas están registradas.
        _usuarioRepositoryMock
            .Setup(r => r.ObtenerPorEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);

        await _servicio.LoginAsync("desconocido@supplychain.com", "loQueSea123!");

        _passwordHasherMock.Verify(
            h => h.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_DeberiaDevolverNull_CuandoLaContraseñaEsIncorrecta()
    {
        ConUsuarioRegistrado(UsuarioAdmin);
        ConContraseñaValida(false);

        var resultado = await _servicio.LoginAsync("jose@supplychain.com", "claveEquivocada");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Login_NoDeberiaEmitirToken_CuandoLaContraseñaEsIncorrecta()
    {
        ConUsuarioRegistrado(UsuarioAdmin);
        ConContraseñaValida(false);

        await _servicio.LoginAsync("jose@supplychain.com", "claveEquivocada");

        _tokenServiceMock.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Theory]
    [InlineData("", "Admin123!")]
    [InlineData("   ", "Admin123!")]
    [InlineData("jose@supplychain.com", "")]
    [InlineData("jose@supplychain.com", "   ")]
    [InlineData(null, null)]
    public async Task Login_DeberiaRechazarCredencialesVacias_SinConsultarLaBase(string? email, string? password)
    {
        var resultado = await _servicio.LoginAsync(email!, password!);

        Assert.Null(resultado);
        _usuarioRepositoryMock.Verify(
            r => r.ObtenerPorEmailAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_DeberiaIgnorarEspaciosAlrededorDelEmail()
    {
        ConUsuarioRegistrado(UsuarioAdmin);
        ConContraseñaValida(true);
        ConTokenEmitido("token-firmado", DateTime.UtcNow.AddHours(1));

        var resultado = await _servicio.LoginAsync("  jose@supplychain.com  ", "Admin123!");

        Assert.NotNull(resultado);
        _usuarioRepositoryMock.Verify(
            r => r.ObtenerPorEmailAsync("jose@supplychain.com"),
            Times.Once);
    }

    [Fact]
    public async Task Login_DeberiaVerificarLaContraseñaContraElHashAlmacenadoDelUsuario()
    {
        ConUsuarioRegistrado(UsuarioAdmin);
        ConContraseñaValida(true);
        ConTokenEmitido("token-firmado", DateTime.UtcNow.AddHours(1));

        await _servicio.LoginAsync("jose@supplychain.com", "Admin123!");

        _passwordHasherMock.Verify(
            h => h.Verify("Admin123!", UsuarioAdmin.PasswordHash),
            Times.Once);
    }

    [Fact]
    public async Task Login_DeberiaDevolverRolVacio_CuandoElUsuarioNoTieneRolCargado()
    {
        // Defensa ante un Include olvidado en el repositorio: preferimos un rol
        // vacío (que no autoriza nada) antes que una NullReferenceException.
        var sinRol = new Usuario
        {
            Id = 3,
            NombreCompleto = "Usuario Sin Rol",
            Email = "sinrol@supplychain.com",
            PasswordHash = "100000.sal.hash",
            Rol = null!
        };

        ConUsuarioRegistrado(sinRol);
        ConContraseñaValida(true);
        ConTokenEmitido("token-firmado", DateTime.UtcNow.AddHours(1));

        var resultado = await _servicio.LoginAsync(sinRol.Email, "Clave123!");

        Assert.NotNull(resultado);
        Assert.Equal(string.Empty, resultado!.Rol);
    }
}

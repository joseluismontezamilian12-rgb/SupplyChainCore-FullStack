using SupplyChainCore.Application.DTOs;
using SupplyChainCore.Application.Interfaces;

namespace SupplyChainCore.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    // Hash descartable con el formato real del hasher. Se verifica contra él
    // cuando el email no existe, para que un login fallido cueste lo mismo
    // exista o no el usuario y el tiempo de respuesta no delate cuentas válidas.
    private const string HashSeñuelo =
        "100000.YWJjZGVmZ2hpamtsbW5vcHFyc3R1dnd4eXo=.ZGVjb3ktaGFzaC1uZXZlci1tYXRjaGVzLWFueXRoaW5nLXJlYWw=";

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResultado?> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var usuario = await _usuarioRepository.ObtenerPorEmailAsync(email.Trim());

        if (usuario is null)
        {
            // Se gasta el mismo trabajo criptográfico que en un login real y
            // se devuelve null, sin distinguir "no existe" de "clave incorrecta".
            _passwordHasher.Verify(password, HashSeñuelo);
            return null;
        }

        if (!_passwordHasher.Verify(password, usuario.PasswordHash))
        {
            return null;
        }

        var (token, expiraUtc) = _tokenService.GenerarToken(usuario);

        return new AuthResultado(
            token,
            expiraUtc,
            usuario.NombreCompleto,
            usuario.Email,
            usuario.Rol?.Nombre ?? string.Empty
        );
    }
}

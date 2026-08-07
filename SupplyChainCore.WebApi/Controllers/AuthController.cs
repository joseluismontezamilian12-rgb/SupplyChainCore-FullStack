using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChainCore.Application.Services;

namespace SupplyChainCore.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST: api/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var resultado = await _authService.LoginAsync(request.Email, request.Password);

        if (resultado is null)
        {
            // Mismo mensaje para email inexistente y contraseña incorrecta:
            // distinguirlos permitiría enumerar qué cuentas existen.
            return Unauthorized(new { error = "Credenciales inválidas." });
        }

        return Ok(resultado);
    }

    // GET: api/auth/me — devuelve la identidad que el token afirma, útil para
    // que el frontend restaure la sesión sin decodificar el JWT por su cuenta.
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            nombre = User.Identity?.Name,
            email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value,
            rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        });
    }
}

public record LoginRequest(string Email, string Password);

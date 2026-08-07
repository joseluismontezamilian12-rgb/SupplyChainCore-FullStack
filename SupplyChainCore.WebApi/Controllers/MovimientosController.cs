using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChainCore.Application.Services;
using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.WebApi.Controllers;

// Todo el controlador exige un token válido. Las lecturas quedan abiertas a
// cualquier usuario autenticado; la escritura se restringe más abajo.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MovimientosController : ControllerBase
{
    private readonly IMovimientoService _movimientoService;

    // Inyección del servicio de aplicación mediante el constructor
    public MovimientosController(IMovimientoService movimientoService)
    {
        _movimientoService = movimientoService;
    }

    // POST: api/movimientos — única operación que altera el ledger, reservada a Admin.
    [Authorize(Roles = RolesDelSistema.Admin)]
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarMovimientoRequest request)
    {
        // La autoría del movimiento se toma del token, nunca del cuerpo de la
        // petición: si el cliente pudiera elegir el UsuarioId, cualquiera podría
        // firmar un movimiento a nombre de otro y el ledger dejaría de ser una
        // bitácora confiable de quién hizo qué.
        var usuarioIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(usuarioIdClaim, out int usuarioId))
        {
            return Unauthorized(new { error = "El token no contiene un identificador de usuario válido." });
        }

        try
        {
            await _movimientoService.RegistrarTransaccionAsync(
                request.ProductoId,
                request.AlmacenId,
                usuarioId,
                request.Cantidad,
                request.TipoMovimiento,
                request.Motivo
            );

            return Ok(new { mensaje = "Movimiento registrado con éxito en el Ledger logístico." });
        }
        catch (ArgumentException ex)
        {
            // Captura errores de tipos de movimientos inválidos (Regla de negocio)
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Captura bloqueos de stock insuficiente (Regla de negocio)
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // Protección ante fallos catastróficos inesperados
            return StatusCode(500, new { error = "Ocurrió un error interno en el servidor.", detalle = ex.Message });
        }
    }

    // GET: api/movimientos/stock/{productoId}/{almacenId}
    [HttpGet("stock/{productoId:int}/{almacenId:int}")]
    public async Task<IActionResult> ObtenerStock(int productoId, int almacenId)
    {
        var stock = await _movimientoService.ObtenerStockDisponibleAsync(productoId, almacenId);
        return Ok(new { productoId, almacenId, stockDisponible = stock });
    }

    // GET: api/movimientos/historial/{productoId}/{almacenId}
    [HttpGet("historial/{productoId:int}/{almacenId:int}")]
    public async Task<IActionResult> ObtenerHistorial(int productoId, int almacenId)
    {
        var historial = await _movimientoService.ObtenerHistorialProductoAsync(productoId, almacenId);
        return Ok(historial);
    }
}

// 🔑 DTO (Data Transfer Object) moderno usando C# Records para recibir los datos de React.
// No lleva UsuarioId a propósito: la identidad la aporta el token, no el cliente.
public record RegistrarMovimientoRequest(
    int ProductoId,
    int AlmacenId,
    int Cantidad,
    string TipoMovimiento,
    string Motivo
);
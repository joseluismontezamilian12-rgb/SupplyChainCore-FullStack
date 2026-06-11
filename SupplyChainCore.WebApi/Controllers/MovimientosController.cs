using Microsoft.AspNetCore.Mvc;
using SupplyChainCore.Application.Services;

namespace SupplyChainCore.WebApi.Controllers;

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

    // POST: api/movimientos
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarMovimientoRequest request)
    {
        try
        {
            await _movimientoService.RegistrarTransaccionAsync(
                request.ProductoId,
                request.AlmacenId,
                request.UsuarioId,
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

// 🔑 DTO (Data Transfer Object) moderno usando C# Records para recibir los datos de React
public record RegistrarMovimientoRequest(
    int ProductoId,
    int AlmacenId,
    int UsuarioId,
    int Cantidad,
    string TipoMovimiento,
    string Motivo
);
using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.Application.Services;

public interface IMovimientoService
{
    Task RegistrarTransaccionAsync(int productoId, int almacenId, int usuarioId, int cantidad, string tipoMovimiento, string motivo);
    Task<int> ObtenerStockDisponibleAsync(int productoId, int almacenId);
    Task<IEnumerable<MovimientoInventario>> ObtenerHistorialProductoAsync(int productoId, int almacenId);
}
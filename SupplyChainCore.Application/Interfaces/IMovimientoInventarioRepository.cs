using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.Application.Interfaces;

public interface IMovimientoInventarioRepository
{
    Task RegistrarMovimientoAsync(MovimientoInventario movimiento);
    Task<int> ObtenerStockActualAsync(int productoId, int almacenId);
    Task<IEnumerable<MovimientoInventario>> ObtenerHistorialAsync(int productoId, int almacenId);
}
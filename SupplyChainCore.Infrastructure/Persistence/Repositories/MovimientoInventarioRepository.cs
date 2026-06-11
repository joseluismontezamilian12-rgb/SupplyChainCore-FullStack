using Microsoft.EntityFrameworkCore;
using SupplyChainCore.Application.Interfaces;
using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.Infrastructure.Persistence.Repositories;

public class MovimientoInventarioRepository : IMovimientoInventarioRepository
{
    private readonly ApplicationDbContext _context;

    public MovimientoInventarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarMovimientoAsync(MovimientoInventario movimiento)
    {
        await _context.MovimientosInventario.AddAsync(movimiento);
        await _context.SaveChangesAsync();
    }

    public async Task<int> ObtenerStockActualAsync(int productoId, int almacenId)
    {
        // 🔑 Lógica del Ledger: El stock real se calcula sumando ingresos y restando salidas/mermas
        var movimientos = await _context.MovimientosInventario
            .Where(m => m.ProductoId == productoId && m.AlmacenId == almacenId)
            .ToListAsync();

        int ingresos = movimientos.Where(m => m.TipoMovimiento == "INGRESO").Sum(m => m.Cantidad);
        int salidas = movimientos.Where(m => m.TipoMovimiento == "SALIDA").Sum(m => m.Cantidad);
        int mermas = movimientos.Where(m => m.TipoMovimiento == "MERMA").Sum(m => m.Cantidad);

        return ingresos - salidas - mermas;
    }

    public async Task<IEnumerable<MovimientoInventario>> ObtenerHistorialAsync(int productoId, int almacenId)
    {
        return await _context.MovimientosInventario
            .Where(m => m.ProductoId == productoId && m.AlmacenId == almacenId)
            .OrderByDescending(m => m.FechaTransaccion)
            .ToListAsync();
    }
}
using SupplyChainCore.Application.Interfaces;
using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.Application.Services;

public class MovimientoService : IMovimientoService
{
    private readonly IMovimientoInventarioRepository _repository;

    // Inyectamos el repositorio mediante el constructor
    public MovimientoService(IMovimientoInventarioRepository repository)
    {
        _repository = repository;
    }

    public async Task RegistrarTransaccionAsync(int productoId, int almacenId, int usuarioId, int cantidad, string tipoMovimiento, string motivo)
    {
        // 1. Forzar a mayúsculas para evitar discrepancias de texto
        string tipo = tipoMovimiento.ToUpper();

        // 2. Regla de Negocio: Validar que el tipo de movimiento sea legítimo
        if (tipo != "INGRESO" && tipo != "SALIDA" && tipo != "MERMA")
        {
            throw new ArgumentException("El tipo de movimiento debe ser INGRESO, SALIDA o MERMA.");
        }

        // 3. Regla de Negocio: Validar stock antes de restar inventario
        if (tipo == "SALIDA" || tipo == "MERMA")
        {
            int stockActual = await _repository.ObtenerStockActualAsync(productoId, almacenId);

            if (stockActual < cantidad)
            {
                throw new InvalidOperationException($"Operación rechazada: Stock insuficiente. Stock disponible: {stockActual}, Cantidad solicitada: {cantidad}.");
            }
        }

        // 4. Si pasa todas las reglas, mapeamos al objeto del Dominio
        var nuevoMovimiento = new MovimientoInventario
        {
            ProductoId = productoId,
            AlmacenId = almacenId,
            UsuarioId = usuarioId,
            Cantidad = cantidad,
            TipoMovimiento = tipo,
            Motivo = motivo,
            FechaTransaccion = DateTime.UtcNow
        };

        // 5. Persistir en la base de datos a través del repositorio
        await _repository.RegistrarMovimientoAsync(nuevoMovimiento);
    }

    public async Task<int> ObtenerStockDisponibleAsync(int productoId, int almacenId)
    {
        return await _repository.ObtenerStockActualAsync(productoId, almacenId);
    }

    public async Task<IEnumerable<MovimientoInventario>> ObtenerHistorialProductoAsync(int productoId, int almacenId)
    {
        return await _repository.ObtenerHistorialAsync(productoId, almacenId);
    }
}
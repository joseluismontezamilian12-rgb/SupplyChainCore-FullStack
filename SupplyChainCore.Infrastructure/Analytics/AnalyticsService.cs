using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyChainCore.Application.Analytics;
using SupplyChainCore.Application.Analytics.DTOs;
using SupplyChainCore.Infrastructure.Persistence;

namespace SupplyChainCore.Infrastructure.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public AnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    // 📊 KPI 1, 2 y 3: Resumen macro financiero y operativo
    public async Task<KpiSummaryDto> GetKpiSummaryAsync()
    {
        // 1. Calcular Valor Total del Inventario cruzando existencias netas por precio
        var transacciones = await _context.MovimientosInventario
            .Include(m => m.Producto)
            .ToListAsync();

        var valorTotal = transacciones
            .GroupBy(m => new { m.ProductoId, m.Producto.PrecioUnitario })
            .Select(g => {
                int stockNeto = g.Sum(m => m.TipoMovimiento == "INGRESO" ? m.Cantidad : -m.Cantidad);
                return stockNeto * g.Key.PrecioUnitario;
            })
            .Sum();

        // 2. Total movimientos del mes en curso
        int mesActual = DateTime.UtcNow.Month;
        int anioActual = DateTime.UtcNow.Year;

        // 🔑 CORREGIDO: Cambiado a FechaTransaccion
        int totalMovimientosMes = transacciones
            .Count(m => m.FechaTransaccion.Month == mesActual && m.FechaTransaccion.Year == anioActual);

        // 3. Tasa de Merma Global (Unidades en MERMA / Unidades Totales Operadas)
        int totalUnidadesMover = transacciones.Sum(m => m.Cantidad);
        int totalMermas = transacciones.Where(m => m.TipoMovimiento == "MERMA").Sum(m => m.Cantidad);
        decimal tasaMermaGlobal = totalUnidadesMover > 0
            ? ((decimal)totalMermas / totalUnidadesMover) * 100
            : 0;

        return new KpiSummaryDto(valorTotal, totalMovimientosMes, Math.Round(tasaMermaGlobal, 2));
    }

    // 🍩 Gráfico de Dona: Stock consolidado distribuido por Almacén
    public async Task<IEnumerable<OcupacionAlmacenDto>> GetOcupacionAlmacenesAsync()
    {
        var movimientos = await _context.MovimientosInventario
            .Include(m => m.Almacen)
            .ToListAsync();

        return movimientos
            .GroupBy(m => new { m.AlmacenId, AlmacenNombre = m.Almacen.Nombre })
            .Select(g => new OcupacionAlmacenDto(
                g.Key.AlmacenId,
                g.Key.AlmacenNombre,
                g.Sum(m => m.TipoMovimiento == "INGRESO" ? m.Cantidad : -m.Cantidad)
            ))
            .Where(x => x.TotalUnidadesStock >= 0) // Evitar inconsistencias visuales
            .ToList();
    }

    // 📈 Gráfico de Líneas: Comportamiento de Ingresos vs Salidas por Mes
    public async Task<IEnumerable<TendenciaMensualDto>> GetTendenciaMensualAsync()
    {
        var movimientos = await _context.MovimientosInventario.ToListAsync();

        // Agrupamos por mes cronológico y calculamos flujos netos
        // 🔑 CORREGIDO: Cambiado a FechaTransaccion
        var datosAgrupados = movimientos
            .GroupBy(m => m.FechaTransaccion.Month)
            .OrderBy(g => g.Key)
            .Select(g => new TendenciaMensualDto(
                MesNombre: CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key).ToUpper(),
                TotalIngresos: g.Where(m => m.TipoMovimiento == "INGRESO").Sum(m => m.Cantidad),
                TotalSalidas: g.Where(m => m.TipoMovimiento == "SALIDA" || m.TipoMovimiento == "MERMA").Sum(m => m.Cantidad)
            ))
            .ToList();

        return datosAgrupados;
    }
}
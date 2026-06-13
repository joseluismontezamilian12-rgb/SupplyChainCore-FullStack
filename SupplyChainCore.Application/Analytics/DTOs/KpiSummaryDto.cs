namespace SupplyChainCore.Application.Analytics.DTOs;

public record KpiSummaryDto(
    decimal ValorTotalInventario,  // Dinero total inmovilizado en soles
    int TotalMovimientosMes,       // Volumen de transacciones actuales
    decimal TasaMermaGlobal        // Porcentaje de pérdida institucional
);
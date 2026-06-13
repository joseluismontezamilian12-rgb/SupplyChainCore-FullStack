namespace SupplyChainCore.Application.Analytics.DTOs;

public record TendenciaMensualDto(
    string MesNombre,      // Ej: "Enero", "Febrero"
    int TotalIngresos,     // Suma de cantidades tipo INGRESO
    int TotalSalidas       // Suma de cantidades tipo SALIDA o MERMA
);
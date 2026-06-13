namespace SupplyChainCore.Application.Analytics.DTOs;

public record OcupacionAlmacenDto(
    int AlmacenId,
    string AlmacenNombre,
    int TotalUnidadesStock
);
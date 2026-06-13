using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyChainCore.Application.Analytics.DTOs;

namespace SupplyChainCore.Application.Analytics;

public interface IAnalyticsService
{
    Task<KpiSummaryDto> GetKpiSummaryAsync();
    Task<IEnumerable<OcupacionAlmacenDto>> GetOcupacionAlmacenesAsync();
    Task<IEnumerable<TendenciaMensualDto>> GetTendenciaMensualAsync();
}
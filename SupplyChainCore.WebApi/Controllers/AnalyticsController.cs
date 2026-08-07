using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChainCore.Application.Analytics;

namespace SupplyChainCore.WebApi.Controllers;

// Los KPIs agregan datos de negocio (valorización de inventario, rotación):
// se exigen credenciales, aunque cualquier rol autenticado puede consultarlos.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    // 🎯 URL: GET api/analytics/kpis
    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        var result = await _analyticsService.GetKpiSummaryAsync();
        return Ok(result);
    }

    // 🎯 URL: GET api/analytics/ocupacion
    [HttpGet("ocupacion")]
    public async Task<IActionResult> GetOcupacion()
    {
        var result = await _analyticsService.GetOcupacionAlmacenesAsync();
        return Ok(result);
    }

    // 🎯 URL: GET api/analytics/tendencia
    [HttpGet("tendencia")]
    public async Task<IActionResult> GetTendencia()
    {
        var result = await _analyticsService.GetTendenciaMensualAsync();
        return Ok(result);
    }
}
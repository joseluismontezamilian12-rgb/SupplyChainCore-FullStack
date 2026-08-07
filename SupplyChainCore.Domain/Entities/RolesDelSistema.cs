namespace SupplyChainCore.Domain.Entities;

/// <summary>
/// Nombres canónicos de los roles del sistema. Se centralizan aquí para que el
/// seeding, los claims del token y los atributos [Authorize] no dependan de
/// strings sueltos escritos a mano en cada capa.
/// </summary>
public static class RolesDelSistema
{
    /// <summary>Puede registrar movimientos que alteran el ledger de inventario.</summary>
    public const string Admin = "Admin";

    /// <summary>Solo lectura: consulta stock, historial y KPIs.</summary>
    public const string Operador = "Operador";
}

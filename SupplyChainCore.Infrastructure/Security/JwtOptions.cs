namespace SupplyChainCore.Infrastructure.Security;

/// <summary>
/// Configuración de firma y validación del JWT. Se enlaza desde la sección "Jwt"
/// de la configuración; en producción la clave debe venir de una variable de
/// entorno o un secret store, nunca de appsettings.json versionado.
/// </summary>
public class JwtOptions
{
    public const string SeccionConfig = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiraEnMinutos { get; set; } = 60;
}

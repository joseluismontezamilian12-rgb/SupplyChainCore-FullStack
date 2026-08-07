using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SupplyChainCore.WebApi.OpenApi;

/// <summary>
/// Añade el esquema de seguridad Bearer al documento OpenAPI generado.
/// Sin esto el documento describe los endpoints pero no dice que hay que
/// enviar un token, y cualquier cliente generado a partir de él falla con 401.
/// </summary>
public class SeguridadBearerTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Token JWT obtenido en POST /api/auth/login. Formato: Bearer {token}"
        };

        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            }
        ];

        return Task.CompletedTask;
    }
}

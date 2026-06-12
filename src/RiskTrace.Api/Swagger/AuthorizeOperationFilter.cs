using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;          // ✅ thêm namespace này
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace RiskTrace.Api.Swagger;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo
            .GetCustomAttributes<AuthorizeAttribute>(true).Any()
            || (context.MethodInfo.DeclaringType?
                .GetCustomAttributes<AuthorizeAttribute>(true).Any() ?? false);

        var hasAllowAnonymous = context.MethodInfo
            .GetCustomAttributes<AllowAnonymousAttribute>(true).Any();

        if (!hasAuthorize || hasAllowAnonymous)
            return;

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            }
        };
    }
}
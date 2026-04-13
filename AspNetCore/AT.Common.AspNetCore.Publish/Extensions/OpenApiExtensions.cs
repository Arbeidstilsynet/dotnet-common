using Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;

using System.Text.Json.Serialization;
using Microsoft.OpenApi;

/// <summary>
/// Extension methods for configuring OpenAPI specifications.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Configure OpenAPI specification with common settings, such as title, version, description and enum handling.
    /// </summary>
    /// <param name="openApiOptions"></param>
    /// <param name="appName"></param>
    /// <returns></returns>
    public static Microsoft.AspNetCore.OpenApi.OpenApiOptions ConfigureBasicOpenApiSpec(
        this Microsoft.AspNetCore.OpenApi.OpenApiOptions openApiOptions,
        string appName
    )
    {
        return openApiOptions
            .AddDocumentTransformer(
                (document, context, cancellationToken) =>
                {
                    document.Info = new OpenApiInfo
                    {
                        Title = $"{appName} API",
                        Version = "v1",
                        Description = $"Common entrypoints to interact with {appName}.",
                    };

                    return Task.CompletedTask;
                }
            )
            .AddSchemaTransformer(
                (schema, context, ct) =>
                {
                    schema.EnumsAsStringUnions();

                    return Task.CompletedTask;
                }
            );
    }

    /// <summary>
    /// Configure OpenAPI specification with security schemes for OAuth2 and Bearer authentication, based on the provided <see cref="AuthConfiguration"/>.
    /// </summary>
    /// <param name="openApiOptions"></param>
    /// <param name="authConfiguration"></param>
    /// <returns></returns>
    public static Microsoft.AspNetCore.OpenApi.OpenApiOptions AddEntraOAuth2AndBearerSecuritySchemes(
        this Microsoft.AspNetCore.OpenApi.OpenApiOptions openApiOptions,
        AuthConfiguration authConfiguration
    )
    {
        if (authConfiguration.DisableAuth)
        {
            return openApiOptions;
        }

        return openApiOptions.AddDocumentTransformer(
            (document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??=
                    new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                };
                document.Components.SecuritySchemes["OAuth2"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        ClientCredentials = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri(
                                $"https://login.microsoftonline.com/{authConfiguration.TenantId}/oauth2/v2.0/token"
                            ),
                            Scopes = new Dictionary<string, string>
                            {
                                { authConfiguration.Scope, "Access API" },
                            },
                        },
                    },
                };
                return Task.CompletedTask;
            }
        );
    }
}

file static class Extensions
{
    /// <summary>
    /// Treat enums as string unions.
    /// </summary>
    /// <param name="schema"></param>
    /// <remarks>Requires <see cref="JsonStringEnumConverter"/> to work</remarks>
    /// <returns></returns>
    public static OpenApiSchema EnumsAsStringUnions(this OpenApiSchema schema)
    {
        if (schema.Enum is { Count: > 0 })
        {
            for (var i = schema.Enum.Count - 1; i >= 0; i--)
            {
                var value = schema.Enum[i];
                if (
                    value is null
                    || string.Equals(value.ToString(), "null", StringComparison.OrdinalIgnoreCase)
                )
                    schema.Enum.RemoveAt(i);
            }

            if (schema.Enum.Count > 0)
            {
                schema.Type ??= JsonSchemaType.String;
            }
        }

        return schema;
    }
}

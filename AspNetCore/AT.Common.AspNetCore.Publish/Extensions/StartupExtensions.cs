using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions;

/// <summary>
/// Extensions configuring an ASP.NET Core application.
/// </summary>
public static partial class StartupExtensions
{
    /// <summary>
    /// Adds Controllers, model validation, problem details, and health checks.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureMvcAction">Configures the AddControllers() call</param>
    /// <param name="configureProblemDetailsAction">Configures the AddProblemDetails() call</param>
    /// <param name="buildHealthChecksAction">Configures the IHealthCheckBuilder</param>
    /// <returns></returns>
    public static IServiceCollection ConfigureApi(
        this IServiceCollection services,
        Action<MvcOptions>? configureMvcAction = null,
        Action<ProblemDetailsOptions>? configureProblemDetailsAction = null,
        Action<IHealthChecksBuilder>? buildHealthChecksAction = null
    )
    {
        configureMvcAction ??= options => options.Filters.Add<RequestValidationFilter>();

        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        services.AddControllers(configureMvcAction);
        services.AddProblemDetails(configureProblemDetailsAction);
        var healthChecksBuilder = services.AddHealthChecks();

        buildHealthChecksAction?.Invoke(healthChecksBuilder);

        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry, including metrics, tracing, and logging.
    /// Adds instrumentation for ASP.NET Core and HTTP client, and exports data to an OpenTelemetry Protocol (OTLP) endpoint.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="appName">Will be converted to kebab case and used as serviceName</param>
    /// <returns></returns>
    public static IServiceCollection ConfigureOpenTelemetry(
        this IServiceCollection services,
        string appName
    )
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(r =>
                r.AddService(
                    serviceName: appName.ConvertToOtelServiceName(),
                    autoGenerateServiceInstanceId: true
                )
            )
            .WithMetrics(options =>
            {
                options.AddAspNetCoreInstrumentation();
                options.AddHttpClientInstrumentation();
                options.AddOtlpExporter();
            })
            .WithTracing(options =>
            {
                options.AddAspNetCoreInstrumentation();
                options.AddHttpClientInstrumentation();
                options.AddOtlpExporter();
            })
            .WithLogging(
                logging =>
                {
                    logging.AddOtlpExporter();
                },
                options =>
                {
                    options.IncludeFormattedMessage = true;
                }
            );
        return services;
    }

    /// <summary>
    /// Adds Swagger, allowing for API documentation generation.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureSwaggerGen">Configures the AddSwaggerGen() call</param>
    /// <returns></returns>
    public static IServiceCollection ConfigureSwagger(
        this IServiceCollection services,
        Action<SwaggerGenOptions>? configureSwaggerGen = null
    )
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(configureSwaggerGen);

        return services;
    }

    /// <summary>
    /// Adds API middleware to the application, including exception handling, HTTPS redirection, routing, authorization, and health checks ("/healthz").
    /// </summary>
    /// <param name="app"></param>
    /// <param name="configureExceptionHandling">Determines mapping from Exceptions to HTTP Status codes.</param>
    /// <returns></returns>
    public static WebApplication AddApi(
        this WebApplication app,
        Action<ExceptionHandlingOptions>? configureExceptionHandling = null
    )
    {
        var exceptionHandlingOptions = new ExceptionHandlingOptions();
        configureExceptionHandling?.Invoke(exceptionHandlingOptions);

        app.UseExceptionHandler(exceptionHandlerApp =>
            exceptionHandlerApp.Run(
                ApiExceptionHandler.CreateExceptionHandler(exceptionHandlingOptions)
            )
        );

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        app.MapControllers();

        app.UseHealthChecks("/healthz");

        return app;
    }

    /// <summary>
    /// Adds the Scalar API reference endpoint and configures Swagger to serve the OpenAPI document at "/openapi/{documentName}.json".
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication AddScalar(this WebApplication app)
    {
        app.MapScalarApiReference();
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "/openapi/{documentName}.json";
        });

        return app;
    }

    internal static string ConvertToOtelServiceName(this string serviceName)
    {
        var serviceNameAsCamelCase = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(
            serviceName
        );
        return CapitalLetterRegex().Replace(serviceNameAsCamelCase, "-$1").ToLower();
    }

    [GeneratedRegex("([A-Z])")]
    private static partial Regex CapitalLetterRegex();

    /// <summary>
    /// Configures CORS with the specified origins.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="allowedOrigins">Array of allowed origins. If empty or null, CORS will allow any origin in development.</param>
    /// <param name="allowCredentials">Whether to allow credentials in CORS requests.</param>
    /// <returns></returns>
    public static IServiceCollection ConfigureCors(
        this IServiceCollection services,
        string[]? allowedOrigins = null,
        bool allowCredentials = false
    )
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();

                if (env.IsDevelopment() && (allowedOrigins == null || allowedOrigins.Length == 0))
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
                else if (allowedOrigins != null && allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();

                    if (allowCredentials)
                    {
                        policy.AllowCredentials();
                    }
                }

                // No fallback else needed - if no origins specified in production, CORS won't work
            });
        });

        return services;
    }
}

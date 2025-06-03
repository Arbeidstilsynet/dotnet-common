using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
    /// Adds Controllers, adds model validation based on DataAnnotations attributes, and configures health checks.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="appName">This will be converted to kebab-case and used as the OTEL service name</param>
    /// <param name="env"></param>
    /// <param name="configureMvcAction">Action which configures MvcOptions.</param>
    /// <param name="configureSwaggerGen">Configure Swagger </param>
    /// <returns></returns>
    public static IServiceCollection ConfigureApi(
        this IServiceCollection services,
        string appName,
        IWebHostEnvironment env,
        Action<MvcOptions>? configureMvcAction = null,
        Action<SwaggerGenOptions>? configureSwaggerGen = null
    )
    {
        configureMvcAction ??= options => options.Filters.Add<RequestValidationFilter>();

        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        services.AddControllers(configureMvcAction);
        services.AddProblemDetails();
        services.AddHealthChecks();

        services.ConfigureSwagger(configureSwaggerGen);
        services.ConfigureOpenTelemetry(appName);
        services.ConfigureLogging(env);

        return services;
    }

    /// <summary>
    /// Adds API middleware to the application, including exception handling, HTTPS redirection, routing, authorization, and health checks ("/healthz").
    /// Also adds the Scalar API reference endpoint.
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

        app.AddScalar();

        return app;
    }

    /// <summary>
    /// Configures OpenTelemetry for the application, including metrics, tracing, and logging.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    private static IServiceCollection ConfigureOpenTelemetry(
        this IServiceCollection services,
        string serviceName
    )
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(r =>
                r.AddService(
                    serviceName: serviceName.ConvertToOtelServiceName(),
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
                logging => logging.AddOtlpExporter(),
                options => options.IncludeFormattedMessage = true
            );
        return services;
    }

    private static IServiceCollection ConfigureSwagger(
        this IServiceCollection services,
        Action<SwaggerGenOptions>? configureSwaggerGen
    )
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(configureSwaggerGen);

        return services;
    }

    private static IServiceCollection ConfigureLogging(
        this IServiceCollection services,
        IWebHostEnvironment env
    )
    {
        services.AddLogging(configure =>
        {
            configure.ClearProviders();
            configure.SetMinimumLevel(LogLevel.Information);
            if (env.EnvironmentName == Environments.Development)
            {
                configure.AddConsole();
            }
            else
            {
                configure.AddJsonConsole();
            }
        });

        return services;
    }

    private static IApplicationBuilder AddScalar(this WebApplication app)
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
}

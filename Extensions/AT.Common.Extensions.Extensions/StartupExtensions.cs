using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

namespace Arbeidstilsynet.Common.Extensions;

/// <summary>
/// Extensions configuring an ASP.NET Core application.
/// </summary>
public static partial class StartupExtensions
{
    /// <summary>
    /// Adds Controllers, and adds an action filter which does general model validation.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureApi(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        services.AddControllers(options =>
        {
            options.Filters.Add<RequestValidationFilter>();
        });
        services.AddProblemDetails();
        services.AddHealthChecks();

        return services;
    }

    /// <summary>
    /// Configures OpenTelemetry for the application, including metrics, tracing, and logging.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureOpenTelemetry(
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

    /// <summary>
    /// Configures Swagger for the application, allowing customization of the Swagger generation options.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureSwaggerGen"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services, Action<SwaggerGenOptions>? configureSwaggerGen)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(configureSwaggerGen);

        return services;
    }

    /// <summary>
    /// Configures logging for the application, setting up console logging in development and JSON console logging in production.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="env"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureLogging(
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

    /// <summary>
    /// Adds API middleware to the application, including exception handling, HTTPS redirection, routing, authorization, and health checks ("/healthz").
    /// </summary>
    /// <param name="app"></param>
    /// <param name="configureExceptionHandling"></param>
    /// <returns></returns>
    public static WebApplication AddApi(this WebApplication app, Action<ExceptionHandlingOptions>? configureExceptionHandling)
    {
        var exceptionHandlingOptions = new ExceptionHandlingOptions();
        configureExceptionHandling?.Invoke(exceptionHandlingOptions);
        
        app.UseExceptionHandler(exceptionHandlerApp =>
            exceptionHandlerApp.Run(ApiExceptionHandler.CreateExceptionHandler(exceptionHandlingOptions))
        );

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        app.MapControllers();

        app.UseHealthChecks("/healthz");

        return app;
    }

    /// <summary>
    /// Adds Scalar API reference endpoints to the application and configures Swagger to serve the OpenAPI document at a specific route.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder AddScalar(this WebApplication app)
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

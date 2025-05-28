using System.Text.RegularExpressions;
using Arbeidstilsynet.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

namespace Arbeidstilsynet.Common.Extensions;

internal static partial class StartupExtensions
{
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
                options.AddEntityFrameworkCoreInstrumentation();
                options.AddNpgsql();
                options.AddOtlpExporter();
            })
            .WithLogging(
                logging => logging.AddOtlpExporter(),
                options => options.IncludeFormattedMessage = true
            );
        return services;
    }

    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sak API", Version = "v1" });
        });

        return services;
    }

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

    public static IApplicationBuilder AddScalar(this WebApplication app)
    {
        app.MapScalarApiReference();
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "/openapi/{documentName}.json";
        });

        return app;
    }

    public static string ConvertToOtelServiceName(this string serviceName)
    {
        var serviceNameAsCamelCase = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(
            serviceName
        );
        return CapitalLetterRegex().Replace(serviceNameAsCamelCase, "-$1").ToLower();
    }

    [GeneratedRegex("([A-Z])")]
    private static partial Regex CapitalLetterRegex();
}

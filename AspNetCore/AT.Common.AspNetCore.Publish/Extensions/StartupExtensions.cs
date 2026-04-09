using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;

/// <summary>
/// Extensions configuring an ASP.NET Core application.
/// </summary>
public static partial class StartupExtensions
{
    /// <summary>
    /// Adds Controllers, model validation, problem details, and health checks.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="startupChecks">
    /// Optional <see cref="StartupChecks"/> delegate defining tasks to run before marking the application as ready.
    /// The delegate receives an <see cref="IServiceProvider"/> to resolve dependencies from the DI container.
    /// If null, no startup tasks are executed.
    /// </param>
    /// <param name="configureMvcAction">Configures the AddControllers() call</param>
    /// <param name="configureProblemDetailsAction">Configures the AddProblemDetails() call</param>
    /// <param name="buildHealthChecksAction">Configures the IHealthCheckBuilder</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    /// <code>
    /// // With startup tasks using DI
    /// builder.Services.ConfigureApi(
    ///     StartupChecks: (provider) =>
    ///     [
    ///         provider.GetRequiredService&lt;IDatabaseMigrator&gt;().MigrateAsync(),
    ///         provider.GetRequiredService&lt;ICacheWarmer&gt;().WarmUpAsync()
    ///     ]
    /// );
    ///
    /// // Without startup tasks
    /// builder.Services.ConfigureApi();
    /// </code>
    /// </example>
    [Obsolete(
        "Use ConfigureStandardApi for the standard API setup, and register startup checks, health checks, MVC options, and problem details explicitly as needed."
    )]
    public static IServiceCollection ConfigureApi(
        this IServiceCollection services,
        StartupChecks? startupChecks = null,
        Action<MvcOptions>? configureMvcAction = null,
        Action<ProblemDetailsOptions>? configureProblemDetailsAction = null,
        Action<IHealthChecksBuilder>? buildHealthChecksAction = null
    )
    {
        configureMvcAction ??= options => options.Filters.Add<RequestValidationFilter>();

        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        services
            .AddControllers(configureMvcAction)
            .ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new CustomControllerFeatureProvider());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonStringUriConverter());
            });
        services.AddProblemDetails(configureProblemDetailsAction);
        services.AddHostedService<StartupBackgroundService>();
        services.AddSingleton<StartupHealthCheck>();
        var healthChecksBuilder = services
            .AddHealthChecks()
            .AddCheck<StartupHealthCheck>("Startup");

        buildHealthChecksAction?.Invoke(healthChecksBuilder);

        return services;
    }

    /// <summary>
    /// Adds Controllers, model validation, problem details, and a health check for startup tasks.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureStandardApi(this IServiceCollection services)
    {
        services.ConfigureStandardMvc();
        services.AddStandardHealthChecks();

        return services;
    }

    /// <summary>
    /// Adds a health check for startup tasks, which will report "unhealthy" until all tasks defined in <see cref="StartupChecks"/> have completed successfully.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddStandardHealthChecks(this IServiceCollection services)
    {
        services.TryAddStartupHealthCheck();
        services.AddHealthChecks().AddCheck<StartupHealthCheck>("Startup");

        return services;
    }

    /// <summary>
    /// Adds Controllers with JSON options, including converters for string enums and URIs, and a global model validation filter.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureStandardMvc(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        services
            .AddControllers(options => options.Filters.Add<RequestValidationFilter>())
            .ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new CustomControllerFeatureProvider());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonStringUriConverter());
            });

        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Each added <see cref="StartupChecks"/> is executed in its own scope in a run-once hosted service (<see cref="StartupBackgroundService"/>)
    /// </summary>
    /// <remarks>
    /// The tasks should be short-lived, as they are run sequentially.
    /// </remarks>
    /// <returns></returns>
    public static IServiceCollection AddStartupChecks(
        this IServiceCollection services,
        StartupChecks startupChecks
    )
    {
        services.TryAddStartupHealthCheck();

        services.AddSingleton(startupChecks);

        return services;
    }

    /// <summary>
    /// Adds OpenAPI with a basic configuration.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="appName"></param>
    /// <returns></returns>
    public static IServiceCollection AddStandardOpenApi(
        this IServiceCollection services,
        string appName
    )
    {
        services.AddOpenApi(options =>
        {
            options.ConfigureBasicOpenApiSpec(appName);
        });

        return services;
    }

    /// <summary>
    /// Configures authentication and authorization based on the provided <see cref="AuthConfiguration"/>.
    ///
    /// Entra authentication is configured when <see cref="AuthConfiguration.DisableAuth"/> is false, using the tenant ID and client ID from the configuration.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="authConfiguration"></param>
    /// <returns></returns>
    /// <remarks>
    /// If <see cref="AuthConfiguration.DisableAuth"/> is true, a permissive authorization policy is registered that allows all requests, and a warning is logged to alert developers.
    /// </remarks>
    public static IServiceCollection AddStandardAuth(
        this IServiceCollection services,
        AuthConfiguration authConfiguration
    )
    {
        if (authConfiguration.DisableAuth)
        {
            services.AddAllowAllAuthorization();
        }
        else
        {
            services.AddEntraAuth(authConfiguration);
        }

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
                options.AddSource("Domain.Logic");
                // hexarch > v3
                options.AddSource("API.Adapters");
                options.AddSource("Infrastructure.Adapters");
                // hexarch < v3
                options.AddSource("App");
                options.AddSource("Infrastructure");
                options.AddAspNetCoreInstrumentation(options =>
                {
                    // Filter out requests to the health check endpoint
                    options.Filter = (httpContext) =>
                    {
                        // Adjust the path to match your health check endpoint
                        return !httpContext.Request.Path.StartsWithSegments("/healthz");
                    };
                });
                options.AddHttpClientInstrumentation();
                options.AddEntityFrameworkCoreInstrumentation();
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
    /// Adds OpenAPI, allowing for API documentation generation.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [Obsolete("Use AddStandardOpenApi instead.")]
    public static IServiceCollection ConfigureOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }

    /// <summary>
    /// Adds OpenAPI, allowing for API documentation generation.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="documentName"></param>
    /// <param name="openApiOptions"></param>
    /// <returns></returns>
    [Obsolete("Use AddStandardOpenApi instead.")]
    public static IServiceCollection ConfigureOpenApi(
        this IServiceCollection services,
        string documentName,
        Action<OpenApiOptions>? openApiOptions = null
    )
    {
        services.AddOpenApi(documentName, openApiOptions ?? (_ => { }));

        return services;
    }

    /// <summary>
    /// Adds API middleware to the application, including exception handling, HTTPS redirection, routing, authorization, and health checks ("/healthz").
    /// </summary>
    /// <param name="app"></param>
    /// <param name="configureExceptionHandling">Determines mapping from Exceptions to HTTP Status codes.</param>
    /// <returns></returns>
    [Obsolete(
        "Use AddStandardApi instead. If you do, you should also remove UseAuthentication, UseCors and AddScalar (if any) around this call, as it is incorporated into AddStandardApi"
    )]
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

        app.MapHealthChecks(
            "/healthz/ready",
            new HealthCheckOptions() { ResponseWriter = CustomHealthReport.WriteHealthCheckDetails }
        );
        app.MapHealthChecks("/healthz/live", new HealthCheckOptions() { Predicate = _ => false });

        return app;
    }

    /// <summary>
    /// Adds API middleware to the application, including authentication, exception handling, HTTPS redirection, routing, authorization, and health checks ("/healthz").
    /// </summary>
    /// <param name="app"></param>
    /// <param name="authConfiguration">If omitted, authentication is not used</param>
    /// <param name="configureExceptionHandling">Determines mapping from Exceptions to HTTP Status codes.</param>
    /// <param name="disableCors">Should only be used for development.</param>
    /// <returns></returns>
    public static WebApplication AddStandardApi(
        this WebApplication app,
        AuthConfiguration? authConfiguration,
        Action<ExceptionHandlingOptions>? configureExceptionHandling = null,
        bool disableCors = false
    )
    {
        var disableAuth = authConfiguration?.DisableAuth ?? true;

        if (!disableAuth)
        {
            app.UseAuthentication();
        }

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

        app.MapHealthChecks(
            "/healthz/ready",
            new HealthCheckOptions() { ResponseWriter = CustomHealthReport.WriteHealthCheckDetails }
        );
        app.MapHealthChecks("/healthz/live", new HealthCheckOptions() { Predicate = _ => false });

        if (!disableCors)
        {
            app.UseCors();
        }

        app.AddScalar();

        return app;
    }

    /// <summary>
    /// Adds the Scalar reference endpoint and configures Scalar to serve the OpenAPI document at "/openapi/{documentName}.json".
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <remarks>
    /// This is now incorporated into <see cref="AddStandardApi"/>, so it should not be necessary to call this method explicitly when using <see cref="AddStandardApi"/>.
    /// </remarks>
    public static WebApplication AddScalar(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Servers = [];
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
    /// <param name="isDevelopment">Whether the application is running in development mode</param>
    /// <returns></returns>
    public static IServiceCollection ConfigureCors(
        this IServiceCollection services,
        string[]? allowedOrigins = null,
        bool allowCredentials = false,
        bool isDevelopment = false
    )
    {
        return services.ConfigureCors(
            new CorsConfiguration
            {
                AllowedOrigins = allowedOrigins ?? [],
                AllowCredentials = allowCredentials,
            },
            isDevelopment
        );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="corsConfiguration"></param>
    /// <param name="isDevelopment"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureCors(
        this IServiceCollection services,
        CorsConfiguration corsConfiguration,
        bool isDevelopment = false
    )
    {
        var allowedOrigins = corsConfiguration.AllowedOrigins;

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (isDevelopment && (allowedOrigins.Length == 0))
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
                else if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();

                    if (corsConfiguration.AllowCredentials)
                    {
                        policy.AllowCredentials();
                    }
                }

                // No fallback else needed - if no origins specified in production, CORS won't work
            });
        });

        return services;
    }

    private static IServiceCollection AddAllowAllAuthorization(this IServiceCollection services)
    {
        services.AddStartupChecks((sp, _) => [LogWarningAsync(sp)]);

        // Register a permissive authorization policy that allows all requests
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
        });

        return services;

        Task LogWarningAsync(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<IAssemblyInfo>>();

            logger?.LogWarning(
                "Authentication is disabled. Update AuthenticationConfiguration to require authentication."
            );

            return Task.CompletedTask;
        }
    }

    private static IServiceCollection AddEntraAuth(
        this IServiceCollection services,
        AuthConfiguration authConfiguration
    )
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.Authority =
                    $"https://login.microsoftonline.com/{authConfiguration.TenantId}/v2.0";
                jwtOptions.Audience = authConfiguration.ClientId;
            });

        services.AddAuthorization();

        services.AddOpenApi(options =>
        {
            options.AddEntraOAuth2AndBearerSecuritySchemes(authConfiguration);
        });

        return services;
    }

    private static IServiceCollection TryAddStartupHealthCheck(this IServiceCollection services)
    {
        services.TryAddSingleton<StartupHealthCheck>();
        services.AddHostedService<StartupBackgroundService>(); // Required for StartupHealthCheck

        return services;
    }
}

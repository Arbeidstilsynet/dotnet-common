using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <summary>
/// A background service that executes startup tasks and updates the application readiness state.
/// </summary>
/// <remarks>
/// This service runs during application startup and executes all tasks defined in <see cref="StartupChecks"/>.
/// Once all tasks complete successfully, it marks <see cref="StartupHealthCheck"/> as completed,
/// allowing the application to receive traffic from load balancers and orchestrators.
/// </remarks>
public class StartupBackgroundService : BackgroundService
{
    private readonly StartupHealthCheck _healthCheck;
    private readonly StartupChecks _startupChecks;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StartupBackgroundService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupBackgroundService"/> class.
    /// </summary>
    /// <param name="healthCheck">The health check to update when startup tasks complete.</param>
    /// <param name="startupChecks">The startup tasks delegate to execute.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="logger"></param>
    public StartupBackgroundService(
        StartupHealthCheck healthCheck,
        StartupChecks startupChecks,
        IServiceProvider serviceProvider,
        ILogger<StartupBackgroundService> logger
    )
    {
        _healthCheck = healthCheck;
        _startupChecks = startupChecks;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Executes the startup tasks sequentially and marks the application as ready upon completion.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Creates a service scope to allow resolution of scoped services within the startup tasks.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        try
        {
            foreach (var taskBeforeStartup in _startupChecks(scope.ServiceProvider))
            {
                await taskBeforeStartup;
            }
            _healthCheck.StartupCompleted = true;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "An exception occured while executing the provided startup checks/tasks."
            );
            _healthCheck.ExceptionOnStartup = e.Message;
            _healthCheck.StartupCompleted = false;
        }
    }
}

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <summary>
/// A health check that reports the readiness state of the application based on startup completion.
/// This is useful for Kubernetes readiness probes or load balancer health checks to ensure
/// the application doesn't receive traffic until it's fully initialized.
/// </summary>
/// <remarks>
/// This health check is thread-safe and uses a volatile field to ensure visibility across threads.
/// Set <see cref="StartupCompleted"/> to <c>true</c> after all startup tasks have completed.
/// </remarks>
public class StartupHealthCheck : IHealthCheck
{
    private volatile bool _isReady;

    /// <summary>
    /// Gets or sets a value indicating whether the application startup has completed.
    /// </summary>
    /// <value>
    /// <c>true</c> if the application is ready to accept traffic; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property is thread-safe and can be safely accessed from multiple threads.
    /// Set this to <c>true</c> once all initialization tasks (database migrations, cache warming, etc.) are complete.
    /// </remarks>
    public bool StartupCompleted
    {
        get => _isReady;
        set => _isReady = value;
    }

    private volatile string? _exceptionOnStartup;

    /// <summary>
    /// Gets or sets the exception message from a failed startup task.
    /// </summary>
    /// <value>
    /// The exception message if a startup task failed; otherwise, <c>null</c>.
    /// </value>
    /// <remarks>
    /// This property is used to provide detailed error information in the health check response
    /// when a startup task encounters an exception. The health check will report as unhealthy
    /// with this message included in the response.
    /// </remarks>
    public string? ExceptionOnStartup
    {
        get => _exceptionOnStartup;
        set => _exceptionOnStartup = value;
    }

    /// <summary>
    /// Checks the health status of the application startup.
    /// </summary>
    /// <param name="context">The health check context containing information about the check being executed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the health check.</param>
    /// <returns>
    /// A task that represents the asynchronous health check operation. The task result contains:
    /// <list type="bullet">
    /// <item><description><see cref="HealthCheckResult.Healthy"/> if <see cref="StartupCompleted"/> is <c>true</c>.</description></item>
    /// <item><description><see cref="HealthCheckResult.Unhealthy"/> if <see cref="StartupCompleted"/> is <c>false</c>.</description></item>
    /// </list>
    /// </returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (StartupCompleted)
        {
            return Task.FromResult(HealthCheckResult.Healthy("The startup task has completed."));
        }
        if (string.IsNullOrEmpty(ExceptionOnStartup))
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("That startup task is still running.")
            );
        }
        else
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    $"The startup task failed. Reason: {ExceptionOnStartup}"
                )
            );
        }
    }
}

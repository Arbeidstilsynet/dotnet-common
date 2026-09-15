using Arbeidstilsynet.Common.Saksarkiv.DependencyInjection.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Arbeidstilsynet.Common.Saksarkiv.Implementation;

internal class SaksarkivHealthCheck(
    ISaksarkivHealthPinger pinger,
    SaksarkivConfiguration configuration
) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(configuration.HealthCheckTimeout);

        try
        {
            var response = await pinger.PongAsync(timeoutCts.Token);
            response ??= "<null>";

            return HealthCheckResult.Healthy(
                description: $"Saksarkiv is healthy. Response: {response}"
            );
        }
        catch (OperationCanceledException ex)
            when (!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested)
        {
            // The health check timed out waiting for Saksarkiv. Report Degraded (HTTP 200) so the
            // consumer's readiness probe is not taken down by a slow/unreachable dependency.
            return HealthCheckResult.Degraded(
                description: $"Saksarkiv health check timed out after {configuration.HealthCheckTimeout}",
                exception: ex
            );
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(
                description: "Saksarkiv is not healthy",
                exception: ex
            );
        }
    }
}

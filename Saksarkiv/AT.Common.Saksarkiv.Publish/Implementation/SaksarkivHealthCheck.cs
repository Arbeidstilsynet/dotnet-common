using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Arbeidstilsynet.Common.Saksarkiv.Implementation;

internal class SaksarkivHealthCheck(SaksarkivClient saksarkivClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new()
    )
    {
        try
        {
            var response = await saksarkivClient.Apiv2.Health.Pong.GetAsync(
                cancellationToken: cancellationToken
            );
            response ??= "<null>";

            return HealthCheckResult.Healthy(description: $"Saksarkiv is healthy. Response: {response}");
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

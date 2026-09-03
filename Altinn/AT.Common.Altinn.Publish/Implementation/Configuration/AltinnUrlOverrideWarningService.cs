using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Configuration;

/// <summary>
/// Logs a warning at startup when Altinn base URLs have been overridden.
/// </summary>
/// <remarks>
/// The warning is emitted from a hosted service rather than at registration time because no
/// <see cref="ILogger"/> is available while the service collection is still being built.
/// </remarks>
internal class AltinnUrlOverrideWarningService(
    AltinnResolution resolution,
    AltinnOverrideRegistry overrides,
    ILogger<AltinnUrlOverrideWarningService> logger
) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (overrides.Overrides.Count > 0)
        {
            logger.LogWarning(
                "Altinn is targeting {Target} with overridden base URL(s): {Overrides}. "
                    + "This is expected when testing against a mock server, but should not be the case otherwise.",
                resolution.Target,
                string.Join(", ", overrides.Overrides)
            );
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

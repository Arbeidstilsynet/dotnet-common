using Polly;
using Polly.Retry;

namespace Arbeidstilsynet.Common.EraClient.Adapters.DependencyInjection;

/// <summary>
/// Configuration file for DependencyInjection extensions to inject the EraClient
/// </summary>
public class EraClientConfiguration
{
    /// <summary>
    /// Default constructur. Does not require any parameters, but all parameters can be overwritten.
    /// </summary>
    /// <param name="authenticationUrl">The base URL for authentication. Per today, all EraClients share the same auth provider. If not set, the default will be selected based on the host environment.</param>
    /// <param name="timeoutInSeconds">Timeout in seconds for resilience pipeline.</param>
    /// <param name="retryStrategy">Retry strategy for resilience pipeline.</param>
    public EraClientConfiguration(
        string? authenticationUrl = null,
        int timeoutInSeconds = 30,
        RetryStrategyOptions? retryStrategy = null
    )
    {
        ResiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(
                retryStrategy
                    ?? new RetryStrategyOptions
                    {
                        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                        Delay = TimeSpan.FromSeconds(2),
                        MaxRetryAttempts = 5,
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                    }
            )
            .AddTimeout(TimeSpan.FromSeconds(timeoutInSeconds))
            .Build();
    }

    /// <summary>
    /// </summary>
    public string? AuthenticationUrl { get; set; }

    /// <summary>
    /// Resilience pipeline for API-kall.
    /// </summary>
    public ResiliencePipeline ResiliencePipeline { get; set; }
}

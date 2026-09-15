using System.Diagnostics;
using Arbeidstilsynet.Common.Saksarkiv.DependencyInjection.Configuration;
using Arbeidstilsynet.Common.Saksarkiv.Implementation;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Arbeidstilsynet.Common.Saksarkiv.Test.Unit;

public class SaksarkivHealthCheckTests
{
    private static SaksarkivConfiguration Config(TimeSpan? timeout = null) =>
        new()
        {
            BaseUrl = "https://saksarkiv.example.com/",
            Scope = "api://scope",
            HealthCheckTimeout = timeout ?? TimeSpan.FromMilliseconds(200),
        };

    [Fact]
    public async Task CheckHealthAsync_WhenPingSucceeds_ReturnsHealthy()
    {
        var pinger = Substitute.For<ISaksarkivHealthPinger>();
        pinger.PongAsync(Arg.Any<CancellationToken>()).Returns("pong");
        var sut = new SaksarkivHealthCheck(pinger, Config());

        var result = await sut.CheckHealthAsync(
            new HealthCheckContext(),
            TestContext.Current.CancellationToken
        );

        result.Status.ShouldBe(HealthStatus.Healthy);
        result.Description.ShouldNotBeNull().ShouldContain("pong");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenPingThrows_ReturnsDegraded()
    {
        var pinger = Substitute.For<ISaksarkivHealthPinger>();
        pinger
            .PongAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));
        var sut = new SaksarkivHealthCheck(pinger, Config());

        var result = await sut.CheckHealthAsync(
            new HealthCheckContext(),
            TestContext.Current.CancellationToken
        );

        result.Status.ShouldBe(HealthStatus.Degraded);
        result.Exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task CheckHealthAsync_WhenPingHangs_ReturnsDegradedQuicklyWithoutThrowing()
    {
        var timeout = TimeSpan.FromMilliseconds(150);
        var pinger = Substitute.For<ISaksarkivHealthPinger>();
        pinger
            .PongAsync(Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                var ct = call.Arg<CancellationToken>();
                // Simulate a slow/unreachable Saksarkiv that only returns when cancelled.
                await Task.Delay(TimeSpan.FromSeconds(30), ct);
                return (string?)"never";
            });
        var sut = new SaksarkivHealthCheck(pinger, Config(timeout));

        var stopwatch = Stopwatch.StartNew();
        var result = await sut.CheckHealthAsync(
            new HealthCheckContext(),
            TestContext.Current.CancellationToken
        );
        stopwatch.Stop();

        result.Status.ShouldBe(HealthStatus.Degraded);
        result.Description.ShouldNotBeNull().ShouldContain("timed out");
        // Must resolve well within a typical readiness probe budget, far below the 30s hang.
        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(5));
    }
}

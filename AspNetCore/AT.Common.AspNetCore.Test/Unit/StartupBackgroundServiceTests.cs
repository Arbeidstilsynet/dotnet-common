using Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Test.Unit;

public class StartupBackgroundServiceTests
{
    [Fact(Timeout = 5000)]
    public async Task ExecuteAsync_MultipleCheckGroups_AllChecksAreExecuted()
    {
        // Arrange
        var check1Executed = false;
        var check2Executed = false;
        var check3Executed = false;

        StartupChecks group1 = _ => [Task.Run(() => check1Executed = true)];
        StartupChecks group2 = _ => [Task.Run(() => check2Executed = true)];
        StartupChecks group3 = _ => [Task.Run(() => check3Executed = true)];

        var sut = CreateService([group1, group2, group3]);

        // Act
        await sut.StartAsync(TestContext.Current.CancellationToken);
        await sut.ExecuteTask!;
        await sut.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        check1Executed.ShouldBeTrue();
        check2Executed.ShouldBeTrue();
        check3Executed.ShouldBeTrue();
    }

    [Fact(Timeout = 5000)]
    public async Task ExecuteAsync_MultipleCheckGroups_ExecutesInRegistrationOrder()
    {
        // Arrange
        var executionOrder = new List<int>();

        StartupChecks group1 = _ => [Task.Run(() => executionOrder.Add(1))];
        StartupChecks group2 = _ => [Task.Run(() => executionOrder.Add(2))];
        StartupChecks group3 = _ => [Task.Run(() => executionOrder.Add(3))];

        var sut = CreateService([group1, group2, group3]);

        // Act
        await sut.StartAsync(TestContext.Current.CancellationToken);
        await sut.ExecuteTask!;
        await sut.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        executionOrder.ShouldBe([1, 2, 3]);
    }

    [Fact(Timeout = 5000)]
    public async Task ExecuteAsync_WhenCompleted_SetsHealthCheckToCompleted()
    {
        // Arrange
        var healthCheck = new StartupHealthCheck();
        StartupChecks group1 = _ => [Task.CompletedTask];
        StartupChecks group2 = _ => [Task.CompletedTask];

        var sut = CreateService([group1, group2], healthCheck);

        healthCheck.StartupCompleted.ShouldBeFalse();

        // Act
        await sut.StartAsync(TestContext.Current.CancellationToken);
        await sut.ExecuteTask!;
        await sut.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        healthCheck.StartupCompleted.ShouldBeTrue();
    }

    [Fact(Timeout = 5000)]
    public async Task ExecuteAsync_NoCheckGroups_SetsHealthCheckToCompleted()
    {
        // Arrange
        var healthCheck = new StartupHealthCheck();
        var sut = CreateService([], healthCheck);

        // Act
        await sut.StartAsync(TestContext.Current.CancellationToken);
        await sut.ExecuteTask!;
        await sut.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        healthCheck.StartupCompleted.ShouldBeTrue();
    }

    [Fact(Timeout = 5000)]
    public async Task ExecuteAsync_EachGroupGetsItsOwnScope()
    {
        // Arrange
        var scopeIds = new List<Guid>();

        StartupChecks group1 = provider =>
            [Task.Run(() => scopeIds.Add(provider.GetRequiredService<ScopedMarker>().Id))];
        StartupChecks group2 = provider =>
            [Task.Run(() => scopeIds.Add(provider.GetRequiredService<ScopedMarker>().Id))];

        var services = new ServiceCollection();
        services.AddScoped<ScopedMarker>();
        var serviceProvider = services.BuildServiceProvider();

        var sut = new StartupBackgroundService(
            new StartupHealthCheck(),
            [group1, group2],
            serviceProvider
        );

        // Act
        await sut.StartAsync(TestContext.Current.CancellationToken);
        await sut.ExecuteTask!;
        await sut.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        scopeIds.Count.ShouldBe(2);
        scopeIds[0].ShouldNotBe(scopeIds[1]);
    }

    private static StartupBackgroundService CreateService(
        List<StartupChecks> groups,
        StartupHealthCheck? healthCheck = null
    )
    {
        healthCheck ??= new StartupHealthCheck();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        return new StartupBackgroundService(healthCheck, groups, serviceProvider);
    }

    private class ScopedMarker
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
}

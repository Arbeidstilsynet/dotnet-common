using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;
using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Unit;

public class EnvironmentRegistrationTests
{
    [Fact]
    public async Task DevelopmentRegistersOnlyOfflinePort()
    {
        using var provider = Register("Development").BuildServiceProvider();
        using var scope = provider.CreateScope();
        scope
            .ServiceProvider.GetRequiredService<IEnhetsregisteret>()
            .ShouldBeOfType<EnhetsregisteretStub>();
        scope.ServiceProvider.GetService<EnhetsregisteretClient>().ShouldBeNull();
        scope.ServiceProvider.GetService<IHttpClientFactory>().ShouldBeNull();
        var stub = scope.ServiceProvider.GetRequiredService<IEnhetsregisteret>();
        var token = TestContext.Current.CancellationToken;
        (await stub.GetEnhet("974761211", token))!.Forretningsadresse.ShouldNotBeNull();
        (await stub.GetUnderenhet("974761076", token))!.Beliggenhetsadresse.ShouldNotBeNull();
        (
            await stub.SearchEnheter(cancellationToken: token)
        )!.Embedded!.Enheter.ShouldHaveSingleItem();
        (
            await stub.SearchUnderenheter(cancellationToken: token)
        )!.Embedded!.Underenheter.ShouldHaveSingleItem();
        (
            await stub.GetOppdateringerEnheter(cancellationToken: token)
        )!.Embedded!.OppdaterteEnheter.ShouldBeEmpty();
        (
            await stub.GetOppdateringerUnderenheter(cancellationToken: token)
        )!.Embedded!.OppdaterteUnderenheter.ShouldBeEmpty();
    }

    [Fact]
    public void DevelopmentEndpointOverrideEnablesRealAdapter()
    {
        using var provider = Register(
                "Development",
                new() { BrregApiBaseUrlOverwrite = "http://mock:8080/" }
            )
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        scope
            .ServiceProvider.GetRequiredService<IEnhetsregisteret>()
            .ShouldBeOfType<EnhetsregisteretAdapter>();
        scope.ServiceProvider.GetRequiredService<EnhetsregisteretClient>().ShouldNotBeNull();
        provider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(DependencyInjectionExtensions.Clientkey)
            .BaseAddress!.AbsoluteUri.ShouldBe("http://mock:8080/");
    }

    [Theory]
    [InlineData("Development", EnhetsregisteretEnvironment.Ppe, "https://data.ppe.brreg.no/")]
    [InlineData("QA", EnhetsregisteretEnvironment.Production, "https://data.brreg.no/")]
    [InlineData("Staging", EnhetsregisteretEnvironment.Production, "https://data.brreg.no/")]
    public void ExplicitTargetSelectsApi(
        string host,
        EnhetsregisteretEnvironment target,
        string url
    )
    {
        using var provider = Register(host, new() { Environment = target }).BuildServiceProvider();
        provider.GetRequiredService<IEnhetsregisteret>().ShouldBeOfType<EnhetsregisteretAdapter>();
        provider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(DependencyInjectionExtensions.Clientkey)
            .BaseAddress!.AbsoluteUri.ShouldBe(url);
    }

    [Fact]
    public void UnknownHostRequiresExplicitTarget()
    {
        Should.Throw<InvalidOperationException>(() => Register("QA"));
        using var provider = Register(
                "QA",
                new() { Environment = EnhetsregisteretEnvironment.Stub }
            )
            .BuildServiceProvider();
        provider.GetRequiredService<IEnhetsregisteret>().ShouldBeOfType<EnhetsregisteretStub>();
    }

    [Fact]
    public void ProductionRejectsOtherTargetsAndOverrides()
    {
        Should.Throw<InvalidOperationException>(() =>
            Register("Production", new() { Environment = EnhetsregisteretEnvironment.Stub })
        );
        Should.Throw<InvalidOperationException>(() =>
            Register("Production", new() { Environment = EnhetsregisteretEnvironment.Ppe })
        );
        Should.Throw<InvalidOperationException>(() =>
            Register("Production", new() { BrregApiBaseUrlOverwrite = "http://mock/" })
        );
        Register("Production", new() { BrregApiBaseUrlOverwrite = "https://data.brreg.no" });
    }

    [Theory]
    [InlineData("")]
    [InlineData("relative")]
    [InlineData("file:///C:/mock")]
    [InlineData("https://mock/?query=value")]
    [InlineData("https://mock/#fragment")]
    [InlineData("https://user:password@mock/")]
    public void InvalidUrlsFailAtRegistration(string url)
    {
        Should.Throw<ArgumentException>(() =>
            Register("Development", new() { BrregApiBaseUrlOverwrite = url })
        );
    }

    [Fact]
    public void UndefinedTargetIsRejected()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            Register("Development", new() { Environment = (EnhetsregisteretEnvironment)999 })
        );
    }

    [Theory]
    [InlineData("Staging", true, true)]
    [InlineData("Staging", false, false)]
    [InlineData("Development", true, false)]
    public async Task OverridesWarnOnlyInStaging(string host, bool overridden, bool expectWarning)
    {
        var logger = new RecordingLogger();
        var services = Register(
            host,
            new() { BrregApiBaseUrlOverwrite = overridden ? "http://mock/" : null }
        );
        services.AddLogging(builder => builder.AddProvider(logger));
        using var provider = services.BuildServiceProvider();
        foreach (var service in provider.GetServices<IHostedService>())
        {
            await service.StartAsync(TestContext.Current.CancellationToken);
            await service.StopAsync(TestContext.Current.CancellationToken);
        }
        logger.Levels.Contains(LogLevel.Warning).ShouldBe(expectWarning);
    }

    [Fact]
    public void OverrideWarningServiceHonorsCancellation()
    {
        using var provider = Register(
                "Staging",
                new() { BrregApiBaseUrlOverwrite = "http://mock/" }
            )
            .BuildServiceProvider();
        var service = provider.GetServices<IHostedService>().ShouldHaveSingleItem();
        var token = new CancellationToken(true);
        Should.Throw<OperationCanceledException>(() => service.StartAsync(token));
        Should.Throw<OperationCanceledException>(() => service.StopAsync(token));
    }

    [Fact]
    public void ConsumerCanReplaceStub()
    {
        var services = Register("Development");
        var replacement = Substitute.For<IEnhetsregisteret>();
        services.AddSingleton(replacement);
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IEnhetsregisteret>().ShouldBeSameAs(replacement);
    }

    private static ServiceCollection Register(string name, EnhetsregisteretConfig? config = null)
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(name);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEnhetsregisteret(environment, config);
        return services;
    }

    private sealed class RecordingLogger : ILoggerProvider, ILogger
    {
        public List<LogLevel> Levels { get; } = [];

        public ILogger CreateLogger(string categoryName) => this;

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) => Levels.Add(logLevel);

        public void Dispose() { }
    }
}

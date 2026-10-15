using Arbeidstilsynet.Common.GeoNorge.Adresser;
using Arbeidstilsynet.Common.GeoNorge.DependencyInjection;
using Arbeidstilsynet.Common.GeoNorge.Eiendom;
using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Arbeidstilsynet.Common.GeoNorge.KommuneInfo;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class EnvironmentRegistrationTests
{
    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void DefaultsToPublicApis(string environment)
    {
        using var provider = Register(environment).BuildServiceProvider();
        using var scope = provider.CreateScope();
        scope
            .ServiceProvider.GetRequiredService<IAddressSearch>()
            .ShouldBeOfType<AddressSearchClient>();
        scope
            .ServiceProvider.GetRequiredService<IFylkeKommuneApi>()
            .ShouldBeOfType<FylkeKommuneClient>();
        scope
            .ServiceProvider.GetRequiredService<IPropertySearch>()
            .ShouldBeOfType<PropertySearchClient>();
        scope.ServiceProvider.GetRequiredService<AdresserClient>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<KommuneInfoClient>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<EiendomClient>().ShouldNotBeNull();
    }

    [Fact]
    public void DevelopmentRegistersOnlyOfflinePorts()
    {
        using var provider = Register("Development").BuildServiceProvider();
        using var scope = provider.CreateScope();
        scope
            .ServiceProvider.GetRequiredService<IAddressSearch>()
            .ShouldBeOfType<AddressSearchStub>();
        scope
            .ServiceProvider.GetRequiredService<IFylkeKommuneApi>()
            .ShouldBeOfType<FylkeKommuneStub>();
        scope
            .ServiceProvider.GetRequiredService<IPropertySearch>()
            .ShouldBeOfType<PropertySearchStub>();
        scope.ServiceProvider.GetService<AdresserClient>().ShouldBeNull();
        scope.ServiceProvider.GetService<KommuneInfoClient>().ShouldBeNull();
        scope.ServiceProvider.GetService<EiendomClient>().ShouldBeNull();
        scope.ServiceProvider.GetService<IHttpClientFactory>().ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DevelopmentOverridesOnlyActivateCorrespondingClients(bool property)
    {
        var config = property
            ? new GeoNorgeConfig { PropertyBaseUrl = "http://mock:8080/" }
            : new GeoNorgeConfig { BaseUrl = "http://mock:8080/" };
        using var provider = Register("Development", config).BuildServiceProvider();
        using var scope = provider.CreateScope();
        if (property)
        {
            scope
                .ServiceProvider.GetRequiredService<IAddressSearch>()
                .ShouldBeOfType<AddressSearchStub>();
            scope
                .ServiceProvider.GetRequiredService<IFylkeKommuneApi>()
                .ShouldBeOfType<FylkeKommuneStub>();
            scope
                .ServiceProvider.GetRequiredService<IPropertySearch>()
                .ShouldBeOfType<PropertySearchClient>();
            scope.ServiceProvider.GetRequiredService<EiendomClient>();
            scope
                .ServiceProvider.GetRequiredService<EiendomRequestAdapter>()
                .BaseUrl.ShouldBe("http://mock:8080/eiendom/v1");
            scope.ServiceProvider.GetService<AdresserClient>().ShouldBeNull();
        }
        else
        {
            scope
                .ServiceProvider.GetRequiredService<IAddressSearch>()
                .ShouldBeOfType<AddressSearchClient>();
            scope
                .ServiceProvider.GetRequiredService<IFylkeKommuneApi>()
                .ShouldBeOfType<FylkeKommuneClient>();
            scope
                .ServiceProvider.GetRequiredService<IPropertySearch>()
                .ShouldBeOfType<PropertySearchStub>();
            scope.ServiceProvider.GetRequiredService<AdresserClient>();
            scope
                .ServiceProvider.GetRequiredService<AdresserRequestAdapter>()
                .BaseUrl.ShouldBe("http://mock:8080/adresser/v1");
            scope.ServiceProvider.GetService<EiendomClient>().ShouldBeNull();
        }
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("QA")]
    public void ExplicitProductionTargetEnablesRealClients(string host)
    {
        using var provider = Register(host, new() { Environment = GeoNorgeEnvironment.Production })
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        scope
            .ServiceProvider.GetRequiredService<IAddressSearch>()
            .ShouldBeOfType<AddressSearchClient>();
        scope
            .ServiceProvider.GetRequiredService<IPropertySearch>()
            .ShouldBeOfType<PropertySearchClient>();
    }

    [Fact]
    public void UnknownHostRequiresExplicitTarget()
    {
        Should.Throw<InvalidOperationException>(() => Register("QA"));
        using var provider = Register("QA", new() { Environment = GeoNorgeEnvironment.Stub })
            .BuildServiceProvider();
        provider.GetRequiredService<IAddressSearch>().ShouldBeOfType<AddressSearchStub>();
    }

    [Fact]
    public void ProductionRejectsStubAndEndpointOverrides()
    {
        Should.Throw<InvalidOperationException>(() =>
            Register("Production", new() { Environment = GeoNorgeEnvironment.Stub })
        );
        Should.Throw<InvalidOperationException>(() =>
            Register("Production", new() { BaseUrl = "http://mock/" })
        );
        Should.Throw<InvalidOperationException>(() =>
            Register("Production", new() { PropertyBaseUrl = "http://mock/" })
        );
        Register(
            "Production",
            new()
            {
                BaseUrl = "https://ws.geonorge.no",
                PropertyBaseUrl = "https://api.kartverket.no",
            }
        );
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
        Should.Throw<ArgumentException>(() => Register("Development", new() { BaseUrl = url }));
        Should.Throw<ArgumentException>(() =>
            Register("Development", new() { PropertyBaseUrl = url })
        );
    }

    [Fact]
    public void UndefinedTargetIsRejected()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            Register("Development", new() { Environment = (GeoNorgeEnvironment)999 })
        );
    }

    [Theory]
    [InlineData("Staging", true, true)]
    [InlineData("Staging", false, false)]
    [InlineData("Development", true, false)]
    public async Task OverridesWarnOnlyInStaging(string host, bool overridden, bool expectWarning)
    {
        var logger = new RecordingLogger();
        var services = Register(host, new() { BaseUrl = overridden ? "http://mock/" : null });
        services.AddLogging(builder => builder.AddProvider(logger));
        using var provider = services.BuildServiceProvider();
        foreach (var service in provider.GetServices<IHostedService>())
        {
            await service.StartAsync(TestContext.Current.CancellationToken);
        }
        logger.Levels.Contains(LogLevel.Warning).ShouldBe(expectWarning);
    }

    [Fact]
    public void DevelopmentHonorsApproximationAndConsumerReplacement()
    {
        var services = Register("Development", new() { UseApproximateSvalbardAndJanMayen = true });
        var replacement = Substitute.For<IAddressSearch>();
        services.AddSingleton(replacement);
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IAddressSearch>().ShouldBeSameAs(replacement);
        provider
            .GetRequiredService<IFylkeKommuneApi>()
            .ShouldBeOfType<ApproximateSvalbardAndJanMayenFylkeKommuneApi>();
    }

    private static ServiceCollection Register(string name, GeoNorgeConfig? config = null)
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(name);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGeoNorge(environment, config);
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

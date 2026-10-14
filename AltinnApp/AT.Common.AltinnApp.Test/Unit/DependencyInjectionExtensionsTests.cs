using Altinn.App.Core.Internal.App;
using Altinn.App.Core.Internal.Data;
using Altinn.App.Core.Internal.Instances;
using Arbeidstilsynet.Common.AltinnApp.DependencyInjection;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Model;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class DependencyInjectionExtensionsTests
{
    private static ServiceCollection CreateServicesWithAltinnSubstitutes()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IApplicationClient>());
        services.AddSingleton(Substitute.For<IDataClient>());
        services.AddSingleton(Substitute.For<IInstanceClient>());
        return services;
    }

    [Fact]
    public void AddStructuredData_BuildsServiceProvider_WithScopedFluentValidator()
    {
        // Arrange: FluentValidation's AddValidatorsFromAssembly registers IValidator<T> as scoped by default.
        // Reproducing: a scoped IValidator<T> must not be captured by a singleton StructuredDataManager.
        var services = CreateServicesWithAltinnSubstitutes();

        services.AddScoped<IValidator<DiTestStructuredData>, DiTestStructuredDataValidator>();

        services.AddStructuredData<DiTestDataModel, DiTestStructuredData, DiTestMapper>(
            CreateEnvironment(Environments.Production)
        );

        // Act + Assert: Building with validateScopes/validateOnBuild reveals captive dependencies.
        var act = () =>
            services.BuildServiceProvider(
                new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }
            );

        act.ShouldNotThrow();
    }

    [Fact]
    public void AddStructuredData_ResolvesManagerFromScope_WithScopedFluentValidator()
    {
        // Arrange
        var services = CreateServicesWithAltinnSubstitutes();
        services.AddScoped<IValidator<DiTestStructuredData>, DiTestStructuredDataValidator>();
        services.AddStructuredData<DiTestDataModel, DiTestStructuredData, DiTestMapper>(
            CreateEnvironment(Environments.Production)
        );

        using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true }
        );

        // Act
        using var scope = provider.CreateScope();
        var manager = scope.ServiceProvider.GetService<
            StructuredDataManager<DiTestDataModel, DiTestStructuredData>
        >();

        // Assert
        manager.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddStructuredData_WithMapper_ResolvesMapperWithInjectedDependencies()
    {
        // Arrange
        var services = CreateServicesWithAltinnSubstitutes();
        services.AddScoped<DiTestPrefixProvider>();
        services.AddStructuredData<DiTestDataModel, DiTestStructuredData, DiTestMapper>(
            CreateEnvironment(Environments.Production)
        );

        using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }
        );
        using var scope = provider.CreateScope();

        // Act
        var mapper = scope.ServiceProvider.GetRequiredService<
            IStructuredDataMapper<DiTestDataModel, DiTestStructuredData>
        >();
        var result = await mapper.Map(new DiTestDataModel { Name = "Test" });

        // Assert
        mapper.ShouldBeOfType<DiTestMapper>();
        result.Name.ShouldBe("prefix-Test");
    }

    [Theory]
    [InlineData("Production", false)]
    [InlineData("Development", true)]
    public void AddStructuredData_WithMapper_UsesEnvironmentDefaults(
        string environmentName,
        bool expectedIncludeErrorDetails
    )
    {
        // Arrange
        var services = CreateServicesWithAltinnSubstitutes();
        services.AddStructuredData<DiTestDataModel, DiTestStructuredData, DiTestMapper>(
            CreateEnvironment(environmentName)
        );

        using var provider = services.BuildServiceProvider();

        // Act
        var config = provider.GetRequiredService<StructuredDataManager<
            DiTestDataModel,
            DiTestStructuredData
        >.Config>();

        // Assert
        config.StructuredDataConfiguration.IncludeErrorDetails.ShouldBe(
            expectedIncludeErrorDetails
        );
    }

    [Fact]
    public void AddStructuredData_WithMapper_AppliesConfigureOnTopOfEnvironmentDefaults()
    {
        // Arrange
        var services = CreateServicesWithAltinnSubstitutes();
        services.AddStructuredData<DiTestDataModel, DiTestStructuredData, DiTestMapper>(
            CreateEnvironment(Environments.Development),
            c =>
                c with
                {
                    TaskIdFilter = ["Task_1"],
                    ExcludedAttachmentDataTypeIds = ["internal-data"],
                }
        );

        using var provider = services.BuildServiceProvider();

        // Act
        var config = provider.GetRequiredService<StructuredDataManager<
            DiTestDataModel,
            DiTestStructuredData
        >.Config>();

        // Assert
        config.StructuredDataConfiguration.TaskIdFilter.ShouldBe(["Task_1"]);
        config.StructuredDataConfiguration.ExcludedAttachmentDataTypeIds.ShouldBe(
            ["internal-data"]
        );
        config.StructuredDataConfiguration.IncludeErrorDetails.ShouldBeTrue();
    }

    [Fact]
    public async Task AddStructuredData_WithObsoleteMapFunc_RegistersDelegateMapper()
    {
        // Arrange
        var services = CreateServicesWithAltinnSubstitutes();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddStructuredData<DiTestDataModel, DiTestStructuredData>(
            m => new DiTestStructuredData { Name = m.Name },
            new StructuredDataConfiguration()
        );
#pragma warning restore CS0618

        using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }
        );
        using var scope = provider.CreateScope();

        // Act
        var mapper = scope.ServiceProvider.GetRequiredService<
            IStructuredDataMapper<DiTestDataModel, DiTestStructuredData>
        >();
        var result = await mapper.Map(new DiTestDataModel { Name = "Test" });

        // Assert
        result.Name.ShouldBe("Test");
        scope
            .ServiceProvider.GetService<
                StructuredDataManager<DiTestDataModel, DiTestStructuredData>
            >()
            .ShouldNotBeNull();
    }

    [Fact]
    public void AddStructuredDataPostProcessor_RegistersEachPostProcessorOnce()
    {
        // Arrange
        var services = CreateServicesWithAltinnSubstitutes();
        services.AddStructuredDataPostProcessor<FirstPostProcessor>();
        services.AddStructuredDataPostProcessor<SecondPostProcessor>();
        services.AddStructuredDataPostProcessor<FirstPostProcessor>();

        using var provider = services.BuildServiceProvider();

        // Act
        var postProcessors = provider.GetServices<IStructuredDataPostProcessor>().ToList();

        // Assert
        postProcessors.Count.ShouldBe(2);
        postProcessors[0].ShouldBeOfType<FirstPostProcessor>();
        postProcessors[1].ShouldBeOfType<SecondPostProcessor>();
    }

    [Fact]
    public void AddStructuredDataPostProcessor_CanBeResolvedByManager()
    {
        // Arrange
        var services = CreateServicesWithAltinnSubstitutes();
        services.AddScoped<IValidator<DiTestStructuredData>, DiTestStructuredDataValidator>();
        services.AddStructuredData<DiTestDataModel, DiTestStructuredData, DiTestMapper>(
            CreateEnvironment(Environments.Production)
        );
        services.AddStructuredDataPostProcessor<FirstPostProcessor>();

        // Act + Assert
        var act = () =>
            services.BuildServiceProvider(
                new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }
            );

        act.ShouldNotThrow();
    }

    public class FirstPostProcessor : IStructuredDataPostProcessor
    {
        public Task Process(StructuredDataPostProcessingContext context) => Task.CompletedTask;
    }

    public class SecondPostProcessor : IStructuredDataPostProcessor
    {
        public Task Process(StructuredDataPostProcessingContext context) => Task.CompletedTask;
    }

    private static IHostEnvironment CreateEnvironment(string environmentName)
    {
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(environmentName);
        return env;
    }

    public class DiTestPrefixProvider
    {
        public string Prefix => "prefix-";
    }

    public class DiTestMapper(DiTestPrefixProvider? prefixProvider = null)
        : IStructuredDataMapper<DiTestDataModel, DiTestStructuredData>
    {
        public Task<DiTestStructuredData> Map(DiTestDataModel dataModel) =>
            Task.FromResult(
                new DiTestStructuredData { Name = prefixProvider?.Prefix + dataModel.Name }
            );
    }

    public class DiTestDataModel
    {
        public string Name { get; set; } = string.Empty;
    }

    public class DiTestStructuredData
    {
        public string Name { get; set; } = string.Empty;
    }

    public class DiTestStructuredDataValidator : AbstractValidator<DiTestStructuredData>
    {
        public DiTestStructuredDataValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}

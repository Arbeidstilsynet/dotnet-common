using Altinn.App.Core.Internal.App;
using Altinn.App.Core.Internal.Data;
using Altinn.App.Core.Internal.Instances;
using Arbeidstilsynet.Common.AltinnApp.DependencyInjection;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
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

        services.AddStructuredData<DiTestDataModel, DiTestStructuredData>(
            m => new DiTestStructuredData { Name = m.Name },
            new StructuredDataConfiguration()
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
        services.AddStructuredData<DiTestDataModel, DiTestStructuredData>(
            m => new DiTestStructuredData { Name = m.Name },
            new StructuredDataConfiguration()
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

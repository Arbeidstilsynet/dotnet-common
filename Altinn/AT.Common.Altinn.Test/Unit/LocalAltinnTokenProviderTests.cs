using System.Net;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class LocalAltinnTokenProviderTests
{
    [Theory]
    [InlineData(null, "altinn:serviceowner/instances.read")]
    [InlineData(null, "altinn:serviceowner/instances.read", "test:scope/read+write&extra=value")]
    [InlineData(1, "altinn:serviceowner/instances.read")]
    [InlineData(2, "altinn:serviceowner/instances.read")]
    [InlineData(3, "altinn:serviceowner/instances.read", "test:scope/read+write&extra=value")]
    [InlineData(4, "altinn:serviceowner/instances.read")]
    public async Task GetToken_UsesConfiguredLevelAndPreservesOrganizationScopesAndToken(
        int? authenticationLevel,
        params string[] scopes
    )
    {
        const string token = "  local-test-token\r\n";
        string? path = null;
        string? query = null;
        await using var server = await StartServer(async context =>
        {
            path = context.Request.Path.Value;
            query = context.Request.QueryString.Value;
            await context.Response.WriteAsync(token, context.RequestAborted);
        });
        using var services = CreateProvider(server, authenticationLevel);
        var provider = services.GetRequiredService<IAltinnTokenProvider>();

        var result = await provider.GetToken(scopes, TestContext.Current.CancellationToken);

        result.ShouldBe(token);
        path.ShouldBe("/Home/GetTestOrgToken");
        query.ShouldBe(
            $"?org=test-org&authenticationLevel={authenticationLevel ?? 4}&orgNumber=&scopes="
                + Uri.EscapeDataString(string.Join(' ', scopes))
        );
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetToken_PropagatesHttpErrorsWithoutRetrying(HttpStatusCode status)
    {
        var requests = 0;
        await using var server = await StartServer(context =>
        {
            Interlocked.Increment(ref requests);
            context.Response.StatusCode = (int)status;
            return Task.CompletedTask;
        });
        using var services = CreateProvider(server);
        var provider = services.GetRequiredService<IAltinnTokenProvider>();

        var exception = await Should.ThrowAsync<HttpRequestException>(() =>
            provider.GetToken(
                ["altinn:serviceowner/instances.read"],
                TestContext.Current.CancellationToken
            )
        );

        exception.StatusCode.ShouldBe(status);
        requests.ShouldBe(1);
    }

    [Fact]
    public async Task GetToken_CancelsTheInFlightHttpRequest()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var aborted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var server = await StartServer(async context =>
        {
            using var registration = context.RequestAborted.Register(() => aborted.TrySetResult());
            started.TrySetResult();
            await aborted.Task;
        });
        using var services = CreateProvider(server);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken
        );
        var provider = services.GetRequiredService<IAltinnTokenProvider>();
        var request = provider.GetToken(["altinn:serviceowner/instances.read"], cancellation.Token);

        await started.Task.WaitAsync(
            TimeSpan.FromSeconds(10),
            TestContext.Current.CancellationToken
        );
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => request);
        await aborted.Task.WaitAsync(
            TimeSpan.FromSeconds(10),
            TestContext.Current.CancellationToken
        );
    }

    private static async Task<WebApplication> StartServer(RequestDelegate handler)
    {
        var builder = WebApplication.CreateSlimBuilder(
            new WebApplicationOptions { EnvironmentName = "Test" }
        );
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
        builder.Logging.ClearProviders();
        var server = builder.Build();
        server.Run(handler);
        await server.StartAsync(TestContext.Current.CancellationToken);
        return server;
    }

    private static ServiceProvider CreateProvider(
        WebApplication server,
        int? authenticationLevel = null
    )
    {
        var configuration = new AltinnConfiguration
        {
            OrgId = "test-org",
            Environment = AltinnEnvironment.Local,
            Overrides = new AltinnUrlOverrides { AppBaseUrl = new Uri(server.Urls.Single()) },
        };
        if (authenticationLevel.HasValue)
        {
            configuration = configuration with
            {
                LocalAuthenticationLevel = authenticationLevel.Value,
            };
        }

        var services = new ServiceCollection();
        services.AddAltinn(
            server.Environment,
            new MaskinportenConfiguration
            {
                PrivateKey = "unused-local-key",
                IntegrationId = "unused-local-integration",
            },
            configuration
        );
        return services.BuildServiceProvider();
    }
}

---
name: test-extensions
description: Testing utilities for .NET — WireMock server setup from OpenAPI specifications using AT.Common.TestExtensions. Use this skill when writing integration or contract tests that mock HTTP dependencies using an OpenAPI spec.
license: MIT
metadata:
  domain: testing
  tags: testing wiremock openapi mocking dotnet integration-tests contract-tests
---

# TestExtensions Skill — Arbeidstilsynet/dotnet-common

`Arbeidstilsynet.Common.TestExtensions` (`AT.Common.TestExtensions.Publish`) provides testing helpers, primarily for setting up WireMock servers from OpenAPI specifications.

---

## Installation

```bash
dotnet add package Arbeidstilsynet.Common.TestExtensions
```

---

## WireMock + OpenAPI (`AddOpenApiMappings`)

Automatically generate WireMock stub mappings from an OpenAPI spec. Example responses are generated from the schema definitions.

### Basic usage

```csharp
using WireMock.Server;
using Arbeidstilsynet.Common.TestExtensions.Extensions;

var server = WireMockServer.Start();

using var fileStream = File.Open(
    "path/to/your/openapi.json",
    FileMode.Open,
    FileAccess.Read
);

server.AddOpenApiMappings(fileStream);
```

### Custom mapping visitor

Transform generated mappings before they are registered (e.g. override response bodies or status codes):

```csharp
server.AddOpenApiMappings(
    openApiSpecStream: fileStream,
    mappingVisitor: mapping =>
    {
        // Override a specific endpoint's response
        if (mapping.Request?.Path?.Matchers?.Any(m => m.Pattern?.ToString()?.Contains("/my-endpoint") == true) == true)
        {
            mapping.Response.Body = """{"id": "overridden-id"}""";
        }
        return mapping;
    }
);
```

### Custom parser settings

```csharp
using WireMock.Net.OpenApiParser.Settings;

server.AddOpenApiMappings(
    openApiSpecStream: fileStream,
    parserSettings: new WireMockOpenApiParserSettings
    {
        ExampleValues = new WireMockOpenApiParserExampleValues(),
    }
);
```

---

## Generated Example Values

By default, `AddOpenApiMappings` uses a fixed `DateTime.UtcNow` snapshot for all date/datetime example values. This ensures deterministic responses within a single test run.

---

## `WireMockExtensions.AddOpenApiMappings` Signature

```csharp
public static void AddOpenApiMappings(
    this WireMockServer server,
    Stream openApiSpecStream,
    Func<MappingModel, MappingModel>? mappingVisitor = null,
    WireMockOpenApiParserSettings? parserSettings = null
)
```

---

## Limitations

- Endpoints that return `oneOf` in the response schema will **not** have stubs generated for them.
- The OpenAPI spec must be provided as a JSON stream.

---

## Typical Integration Test Setup

```csharp
public class MyServiceTests : IDisposable
{
    private readonly WireMockServer _server;

    public MyServiceTests()
    {
        _server = WireMockServer.Start();

        using var stream = File.OpenRead("Fixtures/external-api.openapi.json");
        _server.AddOpenApiMappings(stream);
    }

    [Fact]
    public async Task GetData_ReturnsExpectedResponse()
    {
        // Configure your HTTP client to point to _server.Urls[0]
        // Act and Assert
    }

    public void Dispose() => _server.Stop();
}
```

---

## Adding TestExtensions — Checklist

1. `dotnet add package Arbeidstilsynet.Common.TestExtensions`
2. Place the OpenAPI spec JSON in a `Fixtures/` folder of your test project
3. Mark the spec file as `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` in the `.csproj`
4. Call `server.AddOpenApiMappings(stream)` in test setup
5. Point your `HttpClient` base address to `server.Urls[0]`
6. Use `mappingVisitor` to override specific responses that need non-default values

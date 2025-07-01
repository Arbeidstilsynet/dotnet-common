# Introduction

This is a collection of useful patterns and extensions for unit testing in .NET.

## 📖 Installation

To install the TestExtensions, you can use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.TestExtensions
```

## 🧑‍💻 Usage

```csharp
_server = WireMockServer.Start();

using var fileStream = File.Open(
    "path/to/your/local/openapi.json"),
    FileMode.Open,
    FileAccess.Read
);

_server.AddOpenApiMappings(fileStream);
```

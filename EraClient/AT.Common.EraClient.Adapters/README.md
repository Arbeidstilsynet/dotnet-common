# 📖 Description

Client to interact with our external ERA APIs (OAuth protected)

# 🧑‍💻 Usage

Add service extension in your `Program.cs` file:

```csharp 
var builder = WebApplication.CreateBuilder(args);
var appSettings = builder.Configuration.GetRequired<AppSettings>();
var services = builder.Services;
var env = builder.Environment;
services.AddEraClient(env);
```

Use the need Client via DependencyInjection, available Clients are:

- IAuthenticationClient
- IEraClient

```csharp
public class MyClass(IAuthenticationClient authClient) {

    private async Task<AuthenticationResponseDto> GetAuth() {
        return await authClient.Authenticate(new AuthenticationRequestDto { ... });
    }
}    
```

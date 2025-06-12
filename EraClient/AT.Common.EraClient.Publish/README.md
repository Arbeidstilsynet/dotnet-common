# 📖 Description

Client to interact with our external ERA APIs (OAuth protected). This client is intended to be used in all projects, where we do not have access to our internal ERA APIs, e.g. at Altinn. In order to get access to client credentials, contact ``#team-godkjenning``. 

Why did we create this package?

In many altinn applications (10) we were integrating with our ERA systems. In all of them we have duplicated code, especially in the way we did authentication. By changing all applications to use this client, we increase maintainability and make (future) migrations easier.

# 🧑‍💻 Usage

Add service extension in your `Program.cs` file:

```csharp 
var builder = WebApplication.CreateBuilder(args);
var appSettings = builder.Configuration.GetRequired<AppSettings>();
var services = builder.Services;
services.AddEraAdapter();
```

Use the need Client via DependencyInjection, available Clients are:

- IAuthenticationClient
- IEraAsbestClient

TBD:
- Bemanning
- Bilpleie


```csharp
public class MyClass(IAuthenticationClient authClient) {

    private async Task<AuthenticationResponseDto> GetAuth() {
        return await authClient.Authenticate(new AuthenticationRequestDto { ... });
    }
}    
```

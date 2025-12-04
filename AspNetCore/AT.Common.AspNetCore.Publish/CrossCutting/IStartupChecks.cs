namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <summary>
/// A functional delegate that defines tasks to be executed during application startup before the application is marked as ready.
/// </summary>
/// <param name="serviceProvider">The service provider for resolving dependencies. This is a scoped service provider.</param>
/// <returns>A list of tasks to await during startup.</returns>
/// <remarks>
/// Use this delegate to define initialization tasks such as database migrations, cache warming,
/// or external service validations that must complete before the application can handle requests.
/// All tasks in the returned list will be awaited sequentially during application startup.
/// If any task fails, the startup will be marked as unhealthy.
/// The service provider parameter is scoped, allowing you to resolve both singleton and scoped services from the DI container.
/// </remarks>
/// <example>
/// <code>
/// // With scoped services (e.g., DbContext)
/// StartupChecks tasks = (provider) =>
/// [
///     provider.GetRequiredService&lt;IDatabaseMigrator&gt;().MigrateAsync(),
///     provider.GetRequiredService&lt;MyDbContext&gt;().Database.EnsureCreatedAsync()
/// ];
///
/// // With singleton services
/// StartupChecks tasks = (provider) =>
/// [
///     provider.GetRequiredService&lt;ICacheWarmer&gt;().WarmUpAsync()
/// ];
///
/// // Simple tasks without DI
/// StartupChecks tasks = (_) => [Task.Delay(1000)];
///
/// // No tasks
/// StartupChecks tasks = (_) => [];
/// </code>
/// </example>
public delegate List<Task> StartupChecks(IServiceProvider serviceProvider);

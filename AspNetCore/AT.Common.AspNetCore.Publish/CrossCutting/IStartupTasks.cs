namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <summary>
/// A functional delegate that defines tasks to be executed during application startup before the application is marked as ready.
/// </summary>
/// <param name="serviceProvider">The service provider for resolving dependencies.</param>
/// <returns>A list of tasks to await during startup.</returns>
/// <remarks>
/// Use this delegate to define initialization tasks such as database migrations, cache warming,
/// or external service validations that must complete before the application can handle requests.
/// All tasks in the returned list will be awaited sequentially during application startup.
/// If any task fails, the startup will be marked as unhealthy.
/// The service provider parameter allows you to resolve services from the DI container.
/// </remarks>
/// <example>
/// <code>
/// // With dependency injection
/// StartupTasks tasks = (provider) =>
/// [
///     provider.GetRequiredService&lt;IDatabaseMigrator&gt;().MigrateAsync(),
///     provider.GetRequiredService&lt;ICacheWarmer&gt;().WarmUpAsync()
/// ];
///
/// // Simple tasks without DI
/// StartupTasks tasks = (_) => [Task.Delay(1000)];
///
/// // No tasks
/// StartupTasks tasks = (_) => [];
/// </code>
/// </example>
public delegate List<Task> StartupTasks(IServiceProvider serviceProvider);

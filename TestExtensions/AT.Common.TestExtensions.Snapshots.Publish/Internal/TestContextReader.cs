using System.Diagnostics;
using System.Reflection;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Internal;

internal sealed record TestInfo(Type? TestClass, MethodInfo? Method, object?[]? Arguments);

/// <summary>
/// Reads the running test from xunit v3's <c>Xunit.TestContext.Current</c> via reflection, so the
/// package has no compile-time dependency on a specific xunit version. Falls back to inspecting the
/// call stack when no xunit v3 context is available (for example xunit v2).
/// </summary>
internal static class TestContextReader
{
    private static PropertyInfo? _currentProperty;

    public static TestInfo? Read(string memberName)
    {
        return FromXunitV3() ?? FromStackTrace(memberName);
    }

    private static TestInfo? FromXunitV3()
    {
        try
        {
            var current = FindCurrentProperty()?.GetValue(null);
            var test = GetProperty(current, "Test");
            if (test is null)
            {
                return null;
            }

            var testMethod = GetProperty(test, "TestMethod");
            var method = GetProperty(testMethod, "Method") as MethodInfo;
            var arguments = GetProperty(test, "TestMethodArguments") as object?[];
            var testClass = GetProperty(GetProperty(testMethod, "TestClass"), "Class") as Type;
            return new TestInfo(testClass ?? method?.DeclaringType, method, arguments);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static PropertyInfo? FindCurrentProperty()
    {
        if (_currentProperty is not null)
        {
            return _currentProperty;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name?.StartsWith("xunit", StringComparison.OrdinalIgnoreCase) != true)
            {
                continue;
            }

            var property = assembly
                .GetType("Xunit.TestContext", throwOnError: false)
                ?.GetProperty("Current", BindingFlags.Public | BindingFlags.Static);
            if (property is not null)
            {
                _currentProperty = property;
                return property;
            }
        }

        return null;
    }

    private static object? GetProperty(object? instance, string name)
    {
        if (instance is null)
        {
            return null;
        }

        var type = instance.GetType();
        var property =
            type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance)
            ?? type.GetInterfaces()
                .Select(i => i.GetProperty(name))
                .FirstOrDefault(p => p is not null);
        return property?.GetValue(instance);
    }

    private static TestInfo? FromStackTrace(string memberName)
    {
        if (string.IsNullOrEmpty(memberName))
        {
            return null;
        }

        foreach (var frame in new StackTrace().GetFrames())
        {
            var method = frame.GetMethod();
            var declaringType = method?.DeclaringType;
            if (method is null || declaringType is null)
            {
                continue;
            }

            if (method.Name == memberName && method is MethodInfo methodInfo)
            {
                return new TestInfo(declaringType, methodInfo, null);
            }

            // Async methods run inside a compiler-generated state machine named "<Method>d__N".
            if (
                method.Name == "MoveNext"
                && declaringType.Name.StartsWith($"<{memberName}>", StringComparison.Ordinal)
                && declaringType.DeclaringType is { } owner
            )
            {
                var original = owner
                    .GetMethods(
                        BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.Instance
                            | BindingFlags.Static
                    )
                    .FirstOrDefault(m => m.Name == memberName);
                return new TestInfo(owner, original, null);
            }
        }

        return null;
    }
}

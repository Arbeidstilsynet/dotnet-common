using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal sealed class UmamiDiagnosticsStartupFilter : IStartupFilter
{
    public const string TraceIdHeaderName = "x-trace-id";
    public const string ErrorTypeHeaderName = "x-error-type";
    public const string ErrorLocationHeaderName = "x-error-location";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(
                async (context, pipelineNext) =>
                {
                    SetTraceIdHeader(context);
                    try
                    {
                        await pipelineNext(context);
                    }
                    catch (Exception ex)
                    {
                        SetErrorHeaders(context, ex);
                        throw;
                    }
                }
            );
            next(app);
        };
    }

    private static void SetTraceIdHeader(HttpContext context)
    {
        var traceId = ResolveTraceId(context);
        if (!string.IsNullOrWhiteSpace(traceId))
        {
            context.Response.Headers[TraceIdHeaderName] = traceId;
        }
    }

    private static void SetErrorHeaders(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        SetTraceIdHeader(context);
        context.Response.Headers[ErrorTypeHeaderName] = exception.GetType().Name;
        var location = ResolveErrorLocation(exception);
        if (!string.IsNullOrWhiteSpace(location))
        {
            context.Response.Headers[ErrorLocationHeaderName] = location;
        }
    }

    internal static string? ResolveErrorLocation(Exception exception)
    {
        var trace = new StackTrace(exception, fNeedFileInfo: true);
        var frames = trace.GetFrames();
        if (frames is not { Length: > 0 })
        {
            return null;
        }

        var selectedFrame =
            frames.FirstOrDefault(IsApplicationFrame)
            ?? frames.FirstOrDefault(frame => frame.GetFileLineNumber() > 0)
            ?? frames[0];

        return FormatStackFrame(selectedFrame);
    }

    private static bool IsApplicationFrame(StackFrame frame)
    {
        var ns = frame.GetMethod()?.DeclaringType?.Namespace;
        if (string.IsNullOrWhiteSpace(ns))
        {
            return frame.GetFileLineNumber() > 0;
        }

        return !ns.StartsWith("System", StringComparison.Ordinal)
            && !ns.StartsWith("Microsoft", StringComparison.Ordinal);
    }

    private static string? FormatStackFrame(StackFrame frame)
    {
        var method = frame.GetMethod();
        var typeName = FormatDeclaringTypeName(method?.DeclaringType);
        var methodName = method?.Name;
        var fileName = Path.GetFileName(frame.GetFileName());
        var lineNumber = frame.GetFileLineNumber();

        var symbol =
            !string.IsNullOrWhiteSpace(typeName) && !string.IsNullOrWhiteSpace(methodName)
                ? $"{typeName}.{methodName}"
                : methodName ?? typeName;

        if (!string.IsNullOrWhiteSpace(fileName) && lineNumber > 0)
        {
            return string.IsNullOrWhiteSpace(symbol)
                ? $"{fileName}:{lineNumber}"
                : $"{symbol} ({fileName}:{lineNumber})";
        }

        return symbol;
    }

    private static string? FormatDeclaringTypeName(Type? declaringType)
    {
        if (declaringType is null)
        {
            return null;
        }

        if (declaringType.DeclaringType is { } outerType && declaringType.Name.StartsWith('<'))
        {
            return outerType.Name;
        }

        return declaringType.Name;
    }

    private static string? ResolveTraceId(HttpContext context)
    {
        if (Activity.Current is { } activity)
        {
            var activityTraceId = activity.TraceId.ToHexString();
            if (
                !string.IsNullOrWhiteSpace(activityTraceId)
                && activityTraceId != "00000000000000000000000000000000"
            )
            {
                return activityTraceId;
            }

            if (!string.IsNullOrWhiteSpace(activity.Id))
            {
                return activity.Id;
            }
        }

        return context.TraceIdentifier;
    }
}

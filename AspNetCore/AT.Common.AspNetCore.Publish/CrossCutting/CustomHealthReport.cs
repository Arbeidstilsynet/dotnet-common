using System.Text;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

internal static class CustomHealthReport
{
    /// <summary>
    /// Writes overall status, and status + description of each check as JSON to the response.
    /// Checks are ordered by severity: Unhealthy, Degraded, Healthy.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="report"></param>
    /// <returns></returns>
    public static Task WriteHealthCheckDetails(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var entriesBySeverity = report.Entries.OrderBy(e => (int)e.Value.Status);

        var response = new
        {
            status = report.Status.ToSummary(),
            checks = entriesBySeverity.Select(entry => $"{entry.Key}: {entry.Value.ToStatus()}"),
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static string ToSummary(this HealthStatus status)
    {
        var icon = status switch
        {
            HealthStatus.Healthy => "✅",
            HealthStatus.Degraded => "⚠️",
            HealthStatus.Unhealthy => "❌",
            _ => "❓",
        };

        return $"{icon} {status.ToString()}";
    }

    private static string ToStatus(this HealthReportEntry report)
    {
        var statusSb = new StringBuilder(report.Status.ToSummary());
        if (report.Description is { Length: > 0 } desc)
        {
            statusSb.Append($" - {desc}");
        }

        return statusSb.ToString();
    }
}

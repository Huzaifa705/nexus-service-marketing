namespace Nexus.Web.Infrastructure;

public sealed record HealthStatus(string Status, string App);

public static class HealthEndpoint
{
    public static HealthStatus Current() => new("ok", AppInfo.Name);
}

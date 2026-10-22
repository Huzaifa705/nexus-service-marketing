using Nexus.Web;
using Nexus.Web.Infrastructure;

namespace Nexus.Tests;

public class HealthEndpointTests
{
    [Fact]
    public void Reports_ok_for_this_application()
    {
        var status = HealthEndpoint.Current();

        Assert.Equal("ok", status.Status);
        Assert.Equal(AppInfo.Name, status.App);
    }
}

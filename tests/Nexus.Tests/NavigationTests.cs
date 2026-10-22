using Nexus.Web.Infrastructure;

namespace Nexus.Tests;

public class NavigationTests
{
    [Fact]
    public void Items_have_unique_routes_and_labels()
    {
        var routes = Navigation.Items.Select(i => $"{i.Controller}/{i.Action}").ToList();

        Assert.Equal(routes.Count, routes.Distinct().Count());
        foreach (var item in Navigation.Items)
        {
            Assert.True(item.Controller.Length > 0 && item.Action.Length > 0, "route");
            Assert.True(item.Label.Trim().Length > 0, "label");
        }
    }
}

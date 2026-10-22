namespace Nexus.Web.Infrastructure;

public sealed record NavItem(string Controller, string Action, string Label);

// Single source for the main navigation. Role-based menus are added from Node 3 on;
// only pages that exist belong here.
public static class Navigation
{
    public static readonly IReadOnlyList<NavItem> Items = [new("Home", "Index", "Home")];
}

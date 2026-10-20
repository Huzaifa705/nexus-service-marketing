export type NavItem = { href: string; label: string };

// Single source for the main navigation. Role-based menus are added from Node 3 on;
// only routes that exist belong here.
export const NAV_ITEMS: readonly NavItem[] = [{ href: "/", label: "Home" }];

import { describe, expect, it } from "vitest";
import { NAV_ITEMS } from "@/lib/navigation";

describe("navigation", () => {
  it("has unique, absolute, normalised links with labels", () => {
    const hrefs = NAV_ITEMS.map((item) => item.href);
    expect(new Set(hrefs).size).toBe(hrefs.length);
    for (const item of NAV_ITEMS) {
      expect(item.href.startsWith("/")).toBe(true);
      expect(item.href === "/" || !item.href.endsWith("/")).toBe(true);
      expect(item.label.trim().length).toBeGreaterThan(0);
    }
  });
});

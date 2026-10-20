import { describe, expect, it } from "vitest";
import { GET } from "@/app/health/route";
import { APP_NAME } from "@/lib/app-info";

describe("GET /health", () => {
  it("reports ok and is never cached", async () => {
    const res = GET();
    expect(res.status).toBe(200);
    expect(res.headers.get("cache-control")).toBe("no-store");
    expect(await res.json()).toEqual({ status: "ok", app: APP_NAME });
  });
});

import { spawnSync } from "node:child_process";
import { existsSync } from "node:fs";
import { describe, expect, it } from "vitest";

const DOCS = ["architecture.md", "decision-log.md", "known-issues.md", "requirements-matrix.md", "test-plan.md"];

describe("project documents", () => {
  it("exist under their standard names", () => {
    for (const name of DOCS) expect(existsSync(`docs/${name}`), name).toBe(true);
  });

  it("are consistent (scripts/check-docs.mjs reports no errors)", () => {
    const result = spawnSync(process.execPath, ["scripts/check-docs.mjs", "docs"], { encoding: "utf8" });
    expect(result.stdout).toContain("0 error(s)");
    expect(result.status).toBe(0);
  });
});

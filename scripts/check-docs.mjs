#!/usr/bin/env node
// NEXUS documentation consistency checker. No dependencies.
// Usage: node scripts/check-docs.mjs [docsDir]   (default: ./docs)
// Exit code 1 if any ERROR is found. WARN items are printed but do not fail.
import { readFileSync, readdirSync, existsSync } from "node:fs";
import { join, resolve } from "node:path";

const dir = resolve(process.argv[2] ?? "docs");
const EXPECTED = ["architecture.md", "decision-log.md", "known-issues.md", "requirements-matrix.md", "test-plan.md"];
const PREFIXES = "AUTH|ADM|PLN|CUS|ORD|FEA|CON|EQP|BIL|PAY|PRT|SCH|DOC|AUD|RPT|NFR";
const REQ_ID = new RegExp(`(?<![A-Za-z-])(${PREFIXES})-(\\d{2})\\b`, "g");
const errors = [];
const warns = [];
const err = (c, m) => errors.push(`[${c}] ${m}`);
const warn = (c, m) => warns.push(`[${c}] ${m}`);

// ---- C1 filenames --------------------------------------------------------
if (!existsSync(dir)) { console.error(`docs dir not found: ${dir}`); process.exit(2); }
const present = readdirSync(dir).filter((f) => f.endsWith(".md"));
for (const f of EXPECTED) if (!present.includes(f)) err("C1", `missing document: ${f}`);
for (const f of present) {
  if (!EXPECTED.includes(f) && !f.startsWith("node") ) warn("C1", `unexpected markdown file: ${f}`);
}
const read = (f) => (existsSync(join(dir, f)) ? readFileSync(join(dir, f), "utf8") : "");
const txt = Object.fromEntries(EXPECTED.map((f) => [f, read(f)]));

// ---- helpers ---------------------------------------------------------------
const cells = (line) => line.split("|").slice(1, -1).map((s) => s.trim());
const expand = (s) => {
  const out = new Set();
  for (const m of s.matchAll(new RegExp(`(${PREFIXES})-(\\d{2})\\s*…\\s*(?:\\1-)?(\\d{2})`, "g"))) {
    for (let i = +m[2]; i <= +m[3]; i++) out.add(`${m[1]}-${String(i).padStart(2, "0")}`);
  }
  for (const m of s.matchAll(REQ_ID)) out.add(`${m[1]}-${m[2]}`);
  return out;
};

// ---- C2 decision references -------------------------------------------------
const dl = txt["decision-log.md"];
const decisionIds = new Set([...dl.matchAll(/^## (D-\d{2}) /gm)].map((m) => m[1]));
for (const [f, t] of Object.entries(txt)) {
  for (const m of t.matchAll(/\bD-(\d{2})\b/g)) {
    if (!decisionIds.has(`D-${m[1]}`)) err("C2", `${f}: reference to undefined decision D-${m[1]}`);
  }
}
// summary table vs entries
const summaryIds = new Set([...dl.matchAll(/^\| (D-\d{2}) \|/gm)].map((m) => m[1]));
for (const d of decisionIds) if (!summaryIds.has(d)) err("C2", `${d} has an entry but is missing from the summary table`);
for (const d of summaryIds) if (!decisionIds.has(d)) err("C2", `${d} is in the summary table but has no entry`);
for (const row of dl.matchAll(/^\| (D-\d{2}) \|[^\n]*$/gm)) {
  if (!/\| DECIDED \|/.test(row[0])) err("C3", `${row[1]} is not DECIDED in the summary table`);
}
const entries = dl.split(/\n(?=## D-\d{2} )/).filter((e) => e.startsWith("## D-"));
const affects = {};
for (const e of entries) {
  const id = e.slice(3, 7);
  if (!/- \*\*Decision \(2026-\d\d-\d\d, by delegation\):\*\*/.test(e)) err("C3", `${id} has no recorded Decision line`);
  if (!/\*\*Status \/ Tier \/ Needed by:\*\* DECIDED/.test(e)) err("C3", `${id} entry status is not DECIDED`);
  const a = e.match(/\*\*Affects:\*\*([^\n]*)/);
  affects[id] = a ? expand(a[1]) : new Set();
}

// ---- C4 requirements ----------------------------------------------------------
const rm = txt["requirements-matrix.md"];
const reqs = new Map();
for (const line of rm.split("\n")) {
  const m = line.match(new RegExp(`^\\| (${PREFIXES})-(\\d{2}) \\|`));
  if (!m) continue;
  const c = cells(line);
  if (c.length !== 8) { err("C4", `malformed requirement row (${c.length} cells): ${line.slice(0, 60)}`); continue; }
  const id = c[0];
  if (reqs.has(id)) err("C4", `duplicate requirement ID ${id}`);
  reqs.set(id, { id, text: c[1], source: c[2], type: c[3], pri: c[4], node: c[5], dep: c[6], ac: c[7] });
}
for (const [f, t] of Object.entries(txt)) {
  for (const m of t.matchAll(REQ_ID)) {
    const id = `${m[1]}-${m[2]}`;
    if (!reqs.has(id)) err("C4", `${f}: reference to non-existent requirement ${id}`);
  }
}
for (const r of reqs.values()) {
  for (const id of expand(r.dep)) if (!reqs.has(id)) err("C4", `${r.id} depends on missing ${id}`);
  for (const id of expand(r.dep)) {
    const dep = reqs.get(id);
    if (dep && r.pri === "M" && dep.pri !== "M") err("C4", `${r.id} (Must) depends on ${id} which is ${dep.pri}`);
  }
}

// ---- C7 / C8 source references --------------------------------------------------
const VALID_SRC = /^(INT-[1-4]|EXS-[1-5]|PRO-[1-8]|NF-[1-9]|FIN-(1|DEP|DU|BB|LL|TAX)|FR-[1-9]|G1|Project instructions|—)$/;
for (const r of reqs.values()) {
  const toks = r.source.split(/,|\bvs\b/).map((s) => s.trim()).filter(Boolean);
  for (const tk of toks) {
    const t = tk.replace(/….*$/, "").trim();
    if (!VALID_SRC.test(t)) err("C8", `${r.id}: unrecognised source reference "${tk}"`);
  }
  if (r.type === "SRC⚠" && !/\bD-\d{2}\b/.test(r.dep + " " + r.ac)) err("C7", `${r.id} is SRC⚠ but cites no decision`);
  if (!["SRC", "SRC⚠", "REC"].includes(r.type)) err("C8", `${r.id}: bad type "${r.type}"`);
  if (!["M", "S", "C"].includes(r.pri)) err("C8", `${r.id}: bad priority "${r.pri}"`);
}

// ---- C9 decisions <-> requirements (both directions) ------------------------------
for (const r of reqs.values()) {
  for (const m of r.dep.matchAll(/\bD-(\d{2})\b/g)) {
    const d = `D-${m[1]}`;
    if (affects[d] && !affects[d].has(r.id)) err("C9", `${r.id} cites ${d} but ${d} "Affects" does not list it`);
  }
}
for (const [d, set] of Object.entries(affects)) {
  for (const id of set) {
    const r = reqs.get(id);
    if (!r) { err("C9", `${d} Affects non-existent ${id}`); continue; }
    if (!new RegExp(`\\b${d}\\b`).test(r.dep + " " + r.ac + " " + r.text)) warn("C9", `${d} lists ${id} in Affects but ${id} does not cite ${d}`);
  }
}

// ---- C5 / C6 tests and coverage -----------------------------------------------------
const tp = txt["test-plan.md"];
const tests = new Map();
for (const line of tp.split("\n")) {
  if (!/^\| T-[A-Z0-9]+-\d{2} \|/.test(line)) continue;
  const c = cells(line);
  if (c.length !== 6) { err("C5", `malformed test row (${c.length} cells): ${line.slice(0, 50)}`); continue; }
  if (tests.has(c[0])) err("C5", `duplicate test ID ${c[0]}`);
  tests.set(c[0], { id: c[0], level: c[1], scenario: c[2], expected: c[3], req: c[4], node: c[5] });
}
const covered = new Map();
for (const t of tests.values()) {
  const ids = expand(t.req);
  if (ids.size === 0 && !/all core/.test(t.req)) err("C5", `${t.id} references no requirement`);
  for (const id of ids) {
    if (!reqs.has(id)) err("C5", `${t.id} references missing ${id}`);
    covered.set(id, [...(covered.get(id) ?? []), t.id]);
  }
}
const uncoveredMust = [];
for (const r of reqs.values()) {
  if (r.pri === "M" && !covered.has(r.id)) uncoveredMust.push(r.id);
  if (r.pri === "C" && covered.has(r.id) && !tests.get(covered.get(r.id)[0])?.expected.match(/Not available|403|Denied|Rejected/)) {
    warn("C6", `${r.id} is Could (post-MVP) but has tests: ${covered.get(r.id).join(", ")}`);
  }
}
for (const id of uncoveredMust) err("C6", `Must requirement ${id} has no test case`);
for (const r of reqs.values()) if (r.pri === "S" && !covered.has(r.id)) warn("C6", `Should requirement ${r.id} has no test case`);

// ---- C10 measurable acceptance criteria (Must) ----------------------------------------
const VAGUE = /\b(clear(ly)?|usable|appropriate(ly)?|correct(ly)?|works?|properly|good|easy|reasonable|nice|as needed|agreed|handled|match(es)? reality)\b/i;
for (const r of reqs.values()) {
  if (!r.ac || r.ac === "—") err("C10", `${r.id} has no acceptance criterion`);
  else if (r.pri === "M" && VAGUE.test(r.ac)) warn("C10", `${r.id} acceptance criterion uses vague wording: "${r.ac.match(VAGUE)[0]}"`);
}

// ---- C11 stale wording ---------------------------------------------------------------------
for (const [f, t] of Object.entries(txt)) {
  for (const [re, why] of [[/\bPENDING\b/, "PENDING"], [/\bask now\b/i, "'ask now'"], [/please confirm/i, "'please confirm'"], [/Node 1 draft/i, "'Node 1 draft'"], [/\*\*Owner decision:\*\*/, "empty Owner decision line"]]) {
    if (re.test(t)) err("C11", `${f}: stale wording ${why}`);
  }
}

// ---- C12 section cross-references -------------------------------------------------------------
const headings = (t) => new Set([...t.matchAll(/^##+ (?:(\d+(?:\.\d+)*)\.?|Appendix (A\d?))\b/gm)].map((m) => m[1] ?? m[2]));
const hs = { "architecture.md": headings(txt["architecture.md"]), "test-plan.md": headings(txt["test-plan.md"]), "requirements-matrix.md": headings(txt["requirements-matrix.md"]) };
for (const [f, t] of Object.entries(txt)) {
  for (const m of t.matchAll(/(architecture|test-plan|requirements-matrix)\.md\s*§\s*(\d+(?:\.\d+)?)/g)) {
    if (!hs[`${m[1]}.md`].has(m[2])) err("C12", `${f}: reference to missing section ${m[1]}.md §${m[2]}`);
  }
  for (const m of t.matchAll(/Appendix (A\d)/g)) {
    if (!/^##+ (Appendix A|A\d\.)/m.test(txt["requirements-matrix.md"]) || !new RegExp(`^###? ${m[1]}\\.`, "m").test(txt["requirements-matrix.md"])) err("C12", `${f}: reference to missing Appendix ${m[1]}`);
  }
}

// ---- C13 known issues ---------------------------------------------------------------------------
const ki = txt["known-issues.md"];
const kiIds = new Set([...ki.matchAll(/^\| (KI-(?:R)?\d+) \|/gm)].map((m) => m[1]));
for (const [f, t] of Object.entries(txt)) for (const m of t.matchAll(/\bKI-(?:R)?\d+\b/g)) if (!kiIds.has(m[0]) && f !== "known-issues.md") err("C13", `${f}: unknown known-issue ${m[0]}`);
for (const line of ki.split("\n")) if (/^\| KI-\d+ \|/.test(line) && !/Resolved/.test(line)) err("C13", `spec gap not marked Resolved: ${line.slice(0, 40)}`);

// ---- C14 source-coverage table ---------------------------------------------------------------------
const wanted = ["INT-1", "INT-2", "INT-3", "INT-4", "EXS-1", "EXS-2", "EXS-3", "EXS-4", "EXS-5", "PRO-1", "PRO-2", "PRO-3", "PRO-4", "PRO-5", "PRO-6", "PRO-7", "PRO-8", "NF-1", "NF-2", "NF-3", "NF-4", "NF-5", "NF-6", "NF-7", "NF-8", "NF-9", "FIN-1", "FR-1", "FR-2", "FR-3", "FR-4", "FR-5", "FR-6", "FR-7", "FR-8", "FR-9"];
const covSection = rm.split("## 4. Source coverage check")[1]?.split("\n## ")[0] ?? "";
for (const w of wanted) if (!new RegExp(`^\\| ${w} \\|`, "m").test(covSection)) err("C14", `source item ${w} missing from the coverage table`);
for (const m of covSection.matchAll(REQ_ID)) if (!reqs.has(`${m[1]}-${m[2]}`)) err("C14", `coverage table cites missing ${m[1]}-${m[2]}`);

// ---- C15 header consistency ----------------------------------------------------------------------------
const hdr = (t, k) => (t.match(new RegExp(`^\\| ${k} \\| ([^|]+) \\|`, "m")) ?? [])[1]?.trim();
const versions = EXPECTED.map((f) => [f, hdr(txt[f], "Version"), hdr(txt[f], "Date")]);
const v0 = versions[0];
for (const [f, v, d] of versions) {
  if (!v) err("C15", `${f}: no Version row`);
  else if (v.split(" — ")[0] !== v0[1].split(" — ")[0]) err("C15", `${f}: version "${v}" differs from ${v0[0]} "${v0[1]}"`);
  if (d !== v0[2]) err("C15", `${f}: date ${d} differs from ${v0[2]}`);
}

// ---- report ------------------------------------------------------------------------------------------------
const pri = { M: 0, S: 0, C: 0 };
for (const r of reqs.values()) pri[r.pri]++;
console.log(`docs dir: ${dir}`);
console.log(`requirements: ${reqs.size} (M ${pri.M} / S ${pri.S} / C ${pri.C}); tests: ${tests.size}; decisions: ${decisionIds.size}; known issues: ${kiIds.size}`);
console.log(`Must requirements covered by >=1 test: ${[...reqs.values()].filter((r) => r.pri === "M" && covered.has(r.id)).length}/${pri.M}`);
for (const w of warns) console.log(`WARN  ${w}`);
for (const e of errors) console.log(`ERROR ${e}`);
console.log(`\nresult: ${errors.length} error(s), ${warns.length} warning(s)`);
process.exit(errors.length ? 1 : 0);

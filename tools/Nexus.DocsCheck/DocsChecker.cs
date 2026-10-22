using System.Text;
using System.Text.RegularExpressions;

namespace Nexus.DocsCheck;

public sealed record Report(
    int Requirements, int Must, int Should, int Could, int Tests, int Decisions, int KnownIssues,
    int MustCovered, IReadOnlyList<string> Errors, IReadOnlyList<string> Warnings);

/// <summary>
/// Consistency checker for the five NEXUS project documents (C# port of the original Node script).
/// ERROR items fail the check; WARN items are reported only.
/// </summary>
public static class DocsChecker
{
    public static readonly string[] ExpectedDocuments =
        ["architecture.md", "decision-log.md", "known-issues.md", "requirements-matrix.md", "test-plan.md"];

    private const string Prefixes = "AUTH|ADM|PLN|CUS|ORD|FEA|CON|EQP|BIL|PAY|PRT|SCH|DOC|AUD|RPT|NFR";
    private static readonly Regex RangeRe = new($@"({Prefixes})-(\d{{2}})\s*…\s*(?:\1-)?(\d{{2}})");
    private static readonly Regex ReqIdRe = new($@"(?<![A-Za-z-])({Prefixes})-(\d{{2}})\b");
    private static readonly Regex ReqRowRe = new($@"^\| ({Prefixes})-(\d{{2}}) \|", RegexOptions.Multiline);
    private static readonly Regex ValidSrc = new(@"^(INT-[1-4]|EXS-[1-5]|PRO-[1-8]|NF-[1-9]|FIN-(1|DEP|DU|BB|LL|TAX)|FR-[1-9]|G1|Project instructions|—)$");
    private static readonly Regex Vague = new(@"\b(clear(ly)?|usable|appropriate(ly)?|correct(ly)?|works?|properly|good|easy|reasonable|nice|as needed|agreed|handled|match(es)? reality)\b", RegexOptions.IgnoreCase);

    private sealed record Req(string Id, string Text, string Source, string Type, string Pri, string Node, string Dep, string Ac);
    private sealed record TestCase(string Id, string Level, string Scenario, string Expected, string Req, string Node);

    private static List<string> Cells(string line) =>
        line.Split('|').Skip(1).SkipLast(1).Select(s => s.Trim()).ToList();

    private static HashSet<string> Expand(string s)
    {
        var set = new HashSet<string>();
        foreach (Match m in RangeRe.Matches(s))
            for (var i = int.Parse(m.Groups[2].Value); i <= int.Parse(m.Groups[3].Value); i++)
                set.Add($"{m.Groups[1].Value}-{i:D2}");
        foreach (Match m in ReqIdRe.Matches(s))
            set.Add($"{m.Groups[1].Value}-{m.Groups[2].Value}");
        return set;
    }

    private static HashSet<string> Headings(string t) =>
        Regex.Matches(t, @"^##+ (?:(\d+(?:\.\d+)*)\.?|Appendix (A\d?))\b", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value).ToHashSet();

    public static Report Run(string dir)
    {
        var errors = new List<string>();
        var warns = new List<string>();
        void Err(string c, string m) => errors.Add($"[{c}] {m}");
        void Warn(string c, string m) => warns.Add($"[{c}] {m}");

        if (!Directory.Exists(dir)) throw new DirectoryNotFoundException(dir);

        // C1 filenames
        var present = Directory.GetFiles(dir, "*.md").Select(f => Path.GetFileName(f)).ToList();
        foreach (var f in ExpectedDocuments)
            if (!present.Contains(f)) Err("C1", $"missing document: {f}");
        foreach (var f in present)
            if (!ExpectedDocuments.Contains(f) && !f.StartsWith("node", StringComparison.Ordinal))
                Warn("C1", $"unexpected markdown file: {f}");

        string Read(string f)
        {
            var p = Path.Combine(dir, f);
            return File.Exists(p) ? File.ReadAllText(p).Replace("\r\n", "\n") : "";
        }
        var txt = ExpectedDocuments.ToDictionary(f => f, Read);

        // C2 / C3 decisions
        var dl = txt["decision-log.md"];
        var decisionIds = Regex.Matches(dl, @"^## (D-\d{2}) ", RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToHashSet();
        foreach (var (f, t) in txt)
            foreach (Match m in Regex.Matches(t, @"\bD-(\d{2})\b"))
                if (!decisionIds.Contains($"D-{m.Groups[1].Value}"))
                    Err("C2", $"{f}: reference to undefined decision D-{m.Groups[1].Value}");
        var summaryIds = Regex.Matches(dl, @"^\| (D-\d{2}) \|", RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToHashSet();
        foreach (var d in decisionIds) if (!summaryIds.Contains(d)) Err("C2", $"{d} has an entry but is missing from the summary table");
        foreach (var d in summaryIds) if (!decisionIds.Contains(d)) Err("C2", $"{d} is in the summary table but has no entry");
        foreach (Match row in Regex.Matches(dl, @"^\| (D-\d{2}) \|[^\n]*$", RegexOptions.Multiline))
            if (!Regex.IsMatch(row.Value, @"\| (DECIDED|SUPERSEDED) \|"))
                Err("C3", $"{row.Groups[1].Value} is not DECIDED or SUPERSEDED in the summary table");
        var affects = new Dictionary<string, HashSet<string>>();
        foreach (var e in Regex.Split(dl, @"\n(?=## D-\d{2} )").Where(e => e.StartsWith("## D-", StringComparison.Ordinal)))
        {
            var id = e.Substring(3, 4);
            if (!Regex.IsMatch(e, @"- \*\*Decision \(2026-\d\d-\d\d, by delegation\):\*\*")) Err("C3", $"{id} has no recorded Decision line");
            var st = Regex.Match(e, @"\*\*Status / Tier / Needed by:\*\* (DECIDED|SUPERSEDED)");
            if (!st.Success) Err("C3", $"{id} entry status is not DECIDED or SUPERSEDED");
            else if (st.Groups[1].Value == "SUPERSEDED" && !e.Contains("Superseded by D-", StringComparison.Ordinal))
                Err("C3", $"{id} is SUPERSEDED but does not say which decision replaces it");
            var a = Regex.Match(e, @"\*\*Affects:\*\*([^\n]*)");
            affects[id] = a.Success ? Expand(a.Groups[1].Value) : [];
        }

        // C4 requirements
        var rm = txt["requirements-matrix.md"];
        var reqs = new Dictionary<string, Req>();
        foreach (var line in rm.Split('\n'))
        {
            if (!ReqRowRe.IsMatch(line)) continue;
            var c = Cells(line);
            if (c.Count != 8) { Err("C4", $"malformed requirement row ({c.Count} cells): {Trunc(line, 60)}"); continue; }
            if (reqs.ContainsKey(c[0])) Err("C4", $"duplicate requirement ID {c[0]}");
            reqs[c[0]] = new Req(c[0], c[1], c[2], c[3], c[4], c[5], c[6], c[7]);
        }
        foreach (var (f, t) in txt)
            foreach (Match m in ReqIdRe.Matches(t))
            {
                var id = $"{m.Groups[1].Value}-{m.Groups[2].Value}";
                if (!reqs.ContainsKey(id)) Err("C4", $"{f}: reference to non-existent requirement {id}");
            }
        foreach (var r in reqs.Values)
            foreach (var id in Expand(r.Dep))
            {
                if (!reqs.TryGetValue(id, out var dep)) { Err("C4", $"{r.Id} depends on missing {id}"); continue; }
                if (r.Pri == "M" && dep.Pri != "M") Err("C4", $"{r.Id} (Must) depends on {id} which is {dep.Pri}");
            }

        // C7 / C8 source references and types
        foreach (var r in reqs.Values)
        {
            foreach (var tk in Regex.Split(r.Source, @",|\bvs\b").Select(s => s.Trim()).Where(s => s.Length > 0))
            {
                var t = Regex.Replace(tk, @"….*$", "").Trim();
                if (!ValidSrc.IsMatch(t)) Err("C8", $"{r.Id}: unrecognised source reference \"{tk}\"");
            }
            if (r.Type == "SRC⚠" && !Regex.IsMatch(r.Dep + " " + r.Ac, @"\bD-\d{2}\b")) Err("C7", $"{r.Id} is SRC⚠ but cites no decision");
            if (r.Type is not ("SRC" or "SRC⚠" or "REC")) Err("C8", $"{r.Id}: bad type \"{r.Type}\"");
            if (r.Pri is not ("M" or "S" or "C")) Err("C8", $"{r.Id}: bad priority \"{r.Pri}\"");
        }

        // C9 decisions <-> requirements
        foreach (var r in reqs.Values)
            foreach (Match m in Regex.Matches(r.Dep, @"\bD-(\d{2})\b"))
            {
                var d = $"D-{m.Groups[1].Value}";
                if (affects.TryGetValue(d, out var set) && !set.Contains(r.Id))
                    Err("C9", $"{r.Id} cites {d} but {d} \"Affects\" does not list it");
            }
        foreach (var (d, set) in affects)
            foreach (var id in set)
            {
                if (!reqs.TryGetValue(id, out var r)) { Err("C9", $"{d} Affects non-existent {id}"); continue; }
                if (!Regex.IsMatch(r.Dep + " " + r.Ac + " " + r.Text, $@"\b{d}\b"))
                    Warn("C9", $"{d} lists {id} in Affects but {id} does not cite {d}");
            }

        // C5 / C6 tests and coverage
        var tests = new Dictionary<string, TestCase>();
        foreach (var line in txt["test-plan.md"].Split('\n'))
        {
            if (!Regex.IsMatch(line, @"^\| T-[A-Z0-9]+-\d{2} \|")) continue;
            var c = Cells(line);
            if (c.Count != 6) { Err("C5", $"malformed test row ({c.Count} cells): {Trunc(line, 50)}"); continue; }
            if (tests.ContainsKey(c[0])) Err("C5", $"duplicate test ID {c[0]}");
            tests[c[0]] = new TestCase(c[0], c[1], c[2], c[3], c[4], c[5]);
        }
        var covered = new Dictionary<string, List<string>>();
        foreach (var t in tests.Values)
        {
            var ids = Expand(t.Req);
            if (ids.Count == 0 && !t.Req.Contains("all core", StringComparison.Ordinal)) Err("C5", $"{t.Id} references no requirement");
            foreach (var id in ids)
            {
                if (!reqs.ContainsKey(id)) Err("C5", $"{t.Id} references missing {id}");
                if (!covered.TryGetValue(id, out var list)) covered[id] = list = [];
                list.Add(t.Id);
            }
        }
        foreach (var r in reqs.Values)
        {
            if (r.Pri == "M" && !covered.ContainsKey(r.Id)) Err("C6", $"Must requirement {r.Id} has no test case");
            if (r.Pri == "C" && covered.TryGetValue(r.Id, out var ts) && !Regex.IsMatch(tests[ts[0]].Expected, "Not available|403|Denied|Rejected"))
                Warn("C6", $"{r.Id} is Could (post-MVP) but has tests: {string.Join(", ", ts)}");
            if (r.Pri == "S" && !covered.ContainsKey(r.Id)) Warn("C6", $"Should requirement {r.Id} has no test case");
        }

        // C10 measurable acceptance criteria
        foreach (var r in reqs.Values)
        {
            if (r.Ac.Length == 0 || r.Ac == "—") Err("C10", $"{r.Id} has no acceptance criterion");
            else if (r.Pri == "M" && Vague.IsMatch(r.Ac)) Warn("C10", $"{r.Id} acceptance criterion uses vague wording: \"{Vague.Match(r.Ac).Value}\"");
        }

        // C11 stale wording
        (Regex Re, string Why)[] stale =
        [
            (new Regex(@"\bPENDING\b"), "PENDING"), (new Regex(@"\bask now\b", RegexOptions.IgnoreCase), "'ask now'"),
            (new Regex("please confirm", RegexOptions.IgnoreCase), "'please confirm'"), (new Regex("Node 1 draft", RegexOptions.IgnoreCase), "'Node 1 draft'"),
            (new Regex(@"\*\*Owner decision:\*\*"), "empty Owner decision line"),
        ];
        foreach (var (f, t) in txt)
            foreach (var (re, why) in stale)
                if (re.IsMatch(t)) Err("C11", $"{f}: stale wording {why}");

        // C12 section cross-references
        var hs = new Dictionary<string, HashSet<string>>
        {
            ["architecture.md"] = Headings(txt["architecture.md"]),
            ["test-plan.md"] = Headings(txt["test-plan.md"]),
            ["requirements-matrix.md"] = Headings(rm),
        };
        foreach (var (f, t) in txt)
        {
            foreach (Match m in Regex.Matches(t, @"(architecture|test-plan|requirements-matrix)\.md\s*§\s*(\d+(?:\.\d+)?)"))
                if (!hs[$"{m.Groups[1].Value}.md"].Contains(m.Groups[2].Value))
                    Err("C12", $"{f}: reference to missing section {m.Groups[1].Value}.md §{m.Groups[2].Value}");
            foreach (Match m in Regex.Matches(t, @"Appendix (A\d)"))
                if (!Regex.IsMatch(rm, @"^##+ (Appendix A|A\d\.)", RegexOptions.Multiline) || !Regex.IsMatch(rm, $@"^###? {m.Groups[1].Value}\.", RegexOptions.Multiline))
                    Err("C12", $"{f}: reference to missing Appendix {m.Groups[1].Value}");
        }

        // C13 known issues
        var ki = txt["known-issues.md"];
        var kiIds = Regex.Matches(ki, @"^\| (KI-(?:R)?\d+) \|", RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToHashSet();
        foreach (var (f, t) in txt)
            foreach (Match m in Regex.Matches(t, @"\bKI-(?:R)?\d+\b"))
                if (!kiIds.Contains(m.Value) && f != "known-issues.md") Err("C13", $"{f}: unknown known-issue {m.Value}");
        foreach (var line in ki.Split('\n'))
            if (Regex.IsMatch(line, @"^\| KI-\d+ \|") && !line.Contains("Resolved", StringComparison.Ordinal))
                Err("C13", $"spec gap not marked Resolved: {Trunc(line, 40)}");

        // C14 source coverage table
        var wanted = new List<string>();
        for (var i = 1; i <= 4; i++) wanted.Add($"INT-{i}");
        for (var i = 1; i <= 5; i++) wanted.Add($"EXS-{i}");
        for (var i = 1; i <= 8; i++) wanted.Add($"PRO-{i}");
        for (var i = 1; i <= 9; i++) wanted.Add($"NF-{i}");
        wanted.Add("FIN-1");
        for (var i = 1; i <= 9; i++) wanted.Add($"FR-{i}");
        var parts = rm.Split("## 4. Source coverage check");
        var cov = parts.Length > 1 ? parts[1].Split("\n## ")[0] : "";
        foreach (var w in wanted)
            if (!Regex.IsMatch(cov, $@"^\| {w} \|", RegexOptions.Multiline)) Err("C14", $"source item {w} missing from the coverage table");
        foreach (Match m in ReqIdRe.Matches(cov))
            if (!reqs.ContainsKey($"{m.Groups[1].Value}-{m.Groups[2].Value}")) Err("C14", $"coverage table cites missing {m.Groups[1].Value}-{m.Groups[2].Value}");

        // C15 header consistency
        string? Hdr(string t, string k)
        {
            var m = Regex.Match(t, $@"^\| {k} \| ([^|]+) \|", RegexOptions.Multiline);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }
        var versions = ExpectedDocuments.Select(f => (f, v: Hdr(txt[f], "Version"), d: Hdr(txt[f], "Date"))).ToList();
        var v0 = versions[0];
        foreach (var (f, v, d) in versions)
        {
            if (v is null) Err("C15", $"{f}: no Version row");
            else if (v0.v is not null && v.Split(" — ")[0] != v0.v.Split(" — ")[0]) Err("C15", $"{f}: version \"{v}\" differs from {v0.f} \"{v0.v}\"");
            if (d != v0.d) Err("C15", $"{f}: date {d} differs from {v0.d}");
        }

        var must = reqs.Values.Count(r => r.Pri == "M");
        return new Report(reqs.Count, must, reqs.Values.Count(r => r.Pri == "S"), reqs.Values.Count(r => r.Pri == "C"),
            tests.Count, decisionIds.Count, kiIds.Count,
            reqs.Values.Count(r => r.Pri == "M" && covered.ContainsKey(r.Id)), errors, warns);
    }

    public static string Format(Report r, string dir)
    {
        var sb = new StringBuilder();
        sb.Append($"docs dir: {dir}\n");
        sb.Append($"requirements: {r.Requirements} (M {r.Must} / S {r.Should} / C {r.Could}); tests: {r.Tests}; decisions: {r.Decisions}; known issues: {r.KnownIssues}\n");
        sb.Append($"Must requirements covered by >=1 test: {r.MustCovered}/{r.Must}\n");
        foreach (var w in r.Warnings) sb.Append($"WARN  {w}\n");
        foreach (var e in r.Errors) sb.Append($"ERROR {e}\n");
        sb.Append($"\nresult: {r.Errors.Count} error(s), {r.Warnings.Count} warning(s)\n");
        return sb.ToString();
    }

    private static string Trunc(string s, int n) => s.Length <= n ? s : s[..n];
}

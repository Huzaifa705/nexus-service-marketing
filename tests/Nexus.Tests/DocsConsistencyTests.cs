using Nexus.DocsCheck;

namespace Nexus.Tests;

public class DocsConsistencyTests
{
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "NexusServiceMarketing.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Repository root (NexusServiceMarketing.sln) not found.");
    }

    [Fact]
    public void Documents_exist_under_their_standard_names()
    {
        foreach (var name in DocsChecker.ExpectedDocuments)
            Assert.True(File.Exists(Path.Combine(RepoRoot(), "docs", name)), name);
    }

    [Fact]
    public void Documents_are_consistent()
    {
        var report = DocsChecker.Run(Path.Combine(RepoRoot(), "docs"));

        Assert.Empty(report.Errors);
    }

    [Fact]
    public void Checker_detects_a_broken_decision_reference()
    {
        var temp = Path.Combine(Path.GetTempPath(), "nexus-docs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            foreach (var file in Directory.GetFiles(Path.Combine(RepoRoot(), "docs"), "*.md"))
                File.Copy(file, Path.Combine(temp, Path.GetFileName(file)));
            File.AppendAllText(Path.Combine(temp, "known-issues.md"), "\nBroken reference to D-99.\n");

            var report = DocsChecker.Run(temp);

            Assert.Contains(report.Errors, e => e.Contains("D-99"));
        }
        finally
        {
            Directory.Delete(temp, recursive: true);
        }
    }
}

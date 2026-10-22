using Nexus.DocsCheck;

// Usage: dotnet run --project tools/Nexus.DocsCheck -- [docsDir]   (default: ./docs)
var dir = Path.GetFullPath(args.Length > 0 ? args[0] : "docs");
if (!Directory.Exists(dir))
{
    Console.Error.WriteLine($"docs dir not found: {dir}");
    return 2;
}

var report = DocsChecker.Run(dir);
Console.Write(DocsChecker.Format(report, dir));
return report.Errors.Count > 0 ? 1 : 0;

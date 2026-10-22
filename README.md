# NEXUS Service Marketing System

Web application for Nexus Communication System: orders, feasibility, connections and account IDs,
equipment and vendors, billing and payments, a customer portal, and search. Built with ASP.NET Core
MVC (C#, .NET 10) and SQL Server.

**Status:** Node 2 baseline, re-platformed from the earlier Next.js scaffold (decision D-28; the old
version is kept under the Git tag `nextjs-baseline`). Tooling, layout and a health endpoint exist;
there are no business features and no database yet. The plan and every decision are in [`docs/`](docs/).

## Prerequisites (Windows)

- The .NET 10 SDK. Check with `dotnet --version`; it should print `10.0.x`. Visual Studio with the
  "ASP.NET and web development" workload also works, in a version that supports .NET 10.
- Git.
- From Node 3 on: SQL Server 2022 or 2025 (Express or Developer edition) and SQL Server Management
  Studio (SSMS). LocalDB also works for development. Not needed for Node 2.

## First run (PowerShell)

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Nexus.Web
```

Open the address printed in the console. The health check is `/health`.

## Commands

| Command | What it does |
|---|---|
| `.\scripts\verify.ps1` | Documentation check, restore, build, tests and vulnerable-package check in one go |
| `dotnet run --project tools/Nexus.DocsCheck -- docs` | Consistency check of the project documents in `docs/` |
| `dotnet test` | xUnit tests (unit and document tests) |
| `dotnet list package --vulnerable --include-transitive` | Lists vulnerable NuGet packages |

## Layout

```
NexusServiceMarketing.sln
global.json              SDK pin (.NET 10)
Directory.Build.props    shared settings: nullable, warnings as errors, NuGet audit
src/Nexus.Web/           ASP.NET Core MVC application
tests/Nexus.Tests/       xUnit tests
tools/Nexus.DocsCheck/   documentation consistency checker
docs/                    requirements, decisions, architecture, test plan, known issues
scripts/verify.ps1       one-command local verification
.github/workflows/       CI
```

## Configuration and secrets

Never commit secrets. Local values go in `dotnet user-secrets` (the web project already has a secrets
id) or environment variables. Connection strings for local development use Windows authentication.

## Hosting

Developer edition of SQL Server is for development and testing only; production needs Express or a
licensed edition (decision D-29). Before deployment, set `AllowedHosts` and configure HTTPS (known
issue KI-N2-07).

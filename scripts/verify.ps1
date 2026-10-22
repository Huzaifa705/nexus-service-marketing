# Runs the same checks as CI. From the repository root:  .\scripts\verify.ps1
$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

function Invoke-Step([string]$Name, [scriptblock]$Command) {
    Write-Host ""
    Write-Host "==> $Name" -ForegroundColor Cyan
    & $Command
    if ($LASTEXITCODE -ne 0) { throw "$Name failed (exit code $LASTEXITCODE)" }
}

Invoke-Step "Documentation check" { dotnet run --project tools/Nexus.DocsCheck -c Release -- docs }
Invoke-Step "Restore" { dotnet restore NexusServiceMarketing.sln }
Invoke-Step "Build" { dotnet build NexusServiceMarketing.sln -c Release --no-restore }
Invoke-Step "Tests" { dotnet test NexusServiceMarketing.sln -c Release --no-build }

Write-Host ""
Write-Host "==> Vulnerable packages" -ForegroundColor Cyan
$report = dotnet list NexusServiceMarketing.sln package --vulnerable --include-transitive 2>&1 | Out-String
$exit = $LASTEXITCODE
Write-Host $report
if ($exit -ne 0) { throw "The vulnerability check could not run (exit code $exit)" }
if ($report -match "has the following vulnerable packages") { throw "Vulnerable packages found" }

Write-Host "All checks passed." -ForegroundColor Green

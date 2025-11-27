param(
    [string]$Migration = $null
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Resolve-Path (Join-Path $scriptDir "..\..")

Write-Host "Updating database..." -ForegroundColor Cyan
Write-Host "Project: $projectDir" -ForegroundColor Gray

if ($Migration) {
    Write-Host "Target migration: $Migration" -ForegroundColor Gray
    dotnet ef database update $Migration `
        --project $projectDir `
        --context DataContext
} else {
    Write-Host "Applying all pending migrations" -ForegroundColor Gray
    dotnet ef database update `
        --project $projectDir `
        --context DataContext
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "Database updated successfully!" -ForegroundColor Green
} else {
    Write-Host "Failed to update database." -ForegroundColor Red
    exit 1
}

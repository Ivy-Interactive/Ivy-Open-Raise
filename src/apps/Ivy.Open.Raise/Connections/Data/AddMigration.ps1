param(
    [Parameter(Mandatory=$true)]
    [string]$Name
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Resolve-Path (Join-Path $scriptDir "..\..")
$migrationsDir = Join-Path $scriptDir "Migrations"

Write-Host "Adding migration '$Name'..." -ForegroundColor Cyan
Write-Host "Project: $projectDir" -ForegroundColor Gray
Write-Host "Migrations folder: $migrationsDir" -ForegroundColor Gray

dotnet ef migrations add $Name `
    --project $projectDir `
    --output-dir "Connections\Data\Migrations" `
    --context DataContext

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration '$Name' added successfully!" -ForegroundColor Green
} else {
    Write-Host "Failed to add migration '$Name'." -ForegroundColor Red
    exit 1
}

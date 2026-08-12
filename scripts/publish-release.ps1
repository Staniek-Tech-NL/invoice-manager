[CmdletBinding()]
param(
    [string]$OutputDirectory = "artifacts/release",
    [string]$DotNetPath = "dotnet",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$artifactsRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts"))
$releaseRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputDirectory))

if (-not $releaseRoot.StartsWith($artifactsRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Release output must stay inside $artifactsRoot."
}

$publishDirectory = Join-Path $releaseRoot "InvoiceManager-1.0.0-$Runtime"
$archivePath = Join-Path $releaseRoot "InvoiceManager-1.0.0-$Runtime.zip"

if (Test-Path -LiteralPath $publishDirectory) {
    Remove-Item -LiteralPath $publishDirectory -Recurse -Force
}

if (Test-Path -LiteralPath $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
}

New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null

& $DotNetPath publish (Join-Path $repositoryRoot "src/InvoiceManager.App/InvoiceManager.App.csproj") `
    --configuration Release `
    --runtime $Runtime `
    --self-contained true `
    --output $publishDirectory `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

Compress-Archive -Path (Join-Path $publishDirectory "*") -DestinationPath $archivePath -CompressionLevel Optimal
Write-Host "Release package: $archivePath"

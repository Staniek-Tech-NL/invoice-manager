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
$buildPropsPath = Join-Path $repositoryRoot "Directory.Build.props"

if (-not (Test-Path -LiteralPath $buildPropsPath -PathType Leaf)) {
    throw "Version source not found: $buildPropsPath"
}

[xml]$buildProps = Get-Content -LiteralPath $buildPropsPath -Raw
$versionNode = $buildProps.SelectSingleNode("/Project/PropertyGroup/Version")
$resolvedVersion = if ($null -eq $versionNode) { "" } else { $versionNode.InnerText.Trim() }

if ([string]::IsNullOrWhiteSpace($resolvedVersion)) {
    throw "Project version is empty in $buildPropsPath."
}

if ($resolvedVersion.IndexOfAny([System.IO.Path]::GetInvalidFileNameChars()) -ge 0) {
    throw "Project version '$resolvedVersion' contains characters that are invalid in a release file name."
}

if (-not $releaseRoot.StartsWith($artifactsRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Release output must stay inside $artifactsRoot."
}

$publishDirectory = Join-Path $releaseRoot "InvoiceManager-$resolvedVersion-$Runtime"
$archivePath = Join-Path $releaseRoot "InvoiceManager-$resolvedVersion-$Runtime.zip"

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

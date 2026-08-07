<#
.SYNOPSIS
Builds and runs the Uno Desktop / Skia (net10.0-desktop) target.

.DESCRIPTION
Builds only the Uno Desktop target framework and launches the generated
executable directly (no MSIX packaging, no Developer Mode required).

.EXAMPLE
.\uno.ps1                    # Build + run (foreground)
.\uno.ps1 -SkipRun           # Build only
.\uno.ps1 -Detach            # Build + launch in background (returns immediately)
.\uno.ps1 /p:Configuration=Release
#>
param(
    [Parameter(Position = 0)]
    [string]$Project,
    [switch]$SkipRun,
    [switch]$Detach,
    [switch]$UseMSBuild,
    [Parameter(ValueFromRemainingArguments)]
    [string[]]$ExtraArgs
)

$ErrorActionPreference = 'Stop'

# Disable MSBuild Server: Uno's TaskHostFactory tasks race with worker nodes
# on the .NET 10 preview SDK and fail intermittently with MSB4018/MSB0001.
$env:DOTNET_BUILD_USE_MSBUILD_SERVER = 'false'

# CLI-style aliases
if ($ExtraArgs -contains '--detach') { $Detach = $true; $ExtraArgs = $ExtraArgs | Where-Object { $_ -ne '--detach' } }
if ($ExtraArgs -contains '--use-msbuild') { $UseMSBuild = $true; $ExtraArgs = $ExtraArgs | Where-Object { $_ -ne '--use-msbuild' } }
$extraArgs = $ExtraArgs

# -- 1. Find csproj --
if (-not $Project) {
    $csprojFiles = Get-ChildItem -Path . -Filter "*.csproj" -Depth 0
    if ($csprojFiles.Count -eq 1) {
        $Project = $csprojFiles[0].Name
    } elseif ($csprojFiles.Count -gt 1) {
        Write-Error "Multiple .csproj files found. Specify: .\uno.ps1 <name>.csproj"
        exit 1
    } else {
        Write-Error "No .csproj file found in current directory."
        exit 1
    }
}

# -- 2. Detect desktop TFM from csproj --
$content = Get-Content $Project -Raw
$tfms = @()
if ($content -match '(?s)<TargetFrameworks>\s*(.*?)\s*</TargetFrameworks>') {
    $tfms = $matches[1] -split ';' | ForEach-Object { $_.Trim() } | Where-Object { $_ }
}
$desktopTfm = $tfms | Where-Object { $_ -match '-desktop' } | Select-Object -First 1
if (-not $desktopTfm) {
    Write-Error "No desktop TFM (net*-desktop) found in $Project"
    exit 1
}

# -- 3. Platform / config detection --
$detectedPlatform = if ($env:PROCESSOR_ARCHITECTURE -eq "ARM64") { "ARM64" } else { "x64" }
$detectedConfig = "Debug"
if ($extraArgs | Where-Object { $_ -match "^[/|-]p:Platform=(\w+)" }) { $detectedPlatform = $matches[1] }
if ($extraArgs | Where-Object { $_ -match "^[/|-]p:Configuration=(\w+)" }) { $detectedConfig = $matches[1] }

# Filter out platform/config/restore from extra args (we add our own)
$userArgs = $extraArgs | Where-Object {
    $_ -notmatch "^[/|-]p:Platform=" -and
    $_ -notmatch "^[/|-]p:Configuration=" -and
    $_ -notmatch "^[/|-]restore$" -and
    $_ -notmatch "^[/|-]t:restore$"
}

# -- 4. Build tool --
$msbuild = $null
if ($UseMSBuild) {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -property installationPath 2>$null
        if ($vsPath) {
            $candidate = Join-Path $vsPath "MSBuild\Current\Bin\MSBuild.exe"
            if (Test-Path $candidate) { $msbuild = $candidate }
        }
    }
    if (-not $msbuild) { Write-Host "--> VS MSBuild not found; using dotnet build." -ForegroundColor Yellow }
}

# -- 5. Build (desktop TFM only) --
Write-Host ""
Write-Host "--> Building Uno Desktop target ($desktopTfm, Platform: $detectedPlatform, Config: $detectedConfig)" -ForegroundColor Cyan
$buildExit = 0
try {
    if ($msbuild) {
        $allArgs = @("/nologo", "/v:m", "/m:1",
            "/p:Platform=$detectedPlatform", "/p:Configuration=$detectedConfig",
            "/p:TargetFramework=$desktopTfm", "/restore", $Project) + $userArgs
        & $msbuild $allArgs
        $buildExit = $LASTEXITCODE
    } else {
        $dotnetArgs = @($Project, "-nologo", "-v:m", "-m:1", "-f", $desktopTfm,
            "-p:Platform=$detectedPlatform", "-p:Configuration=$detectedConfig")
        $dotnetArgs += $userArgs | ForEach-Object {
            if ($_ -match "^/(.+)$") { "-$($matches[1])" } else { $_ }
        }
        & dotnet build @dotnetArgs
        $buildExit = $LASTEXITCODE
    }
}
finally {}

if ($buildExit -ne 0) {
    Write-Host ""
    Write-Host "BUILD FAILED (exit code $buildExit)" -ForegroundColor Red
    exit $buildExit
}
Write-Host ""
Write-Host "BUILD SUCCEEDED" -ForegroundColor Green

# -- 6. Run the executable directly (no MSIX) --
if ($SkipRun) { Write-Host "--> Skipping run (-SkipRun)" -ForegroundColor DarkGray; exit 0 }

$projectDir = Split-Path (Resolve-Path $Project) -Parent
if (-not $projectDir) { $projectDir = "." }
$outputDir = Join-Path $projectDir "bin\$detectedPlatform\$detectedConfig\$desktopTfm"
if (-not (Test-Path $outputDir)) {
    Write-Host "WARNING: Build output not found at $outputDir -- skipping run" -ForegroundColor Yellow
    exit 0
}

# Find the output exe (AssemblyName.exe). Fall back to first *.exe if needed.
$exe = $null
if ($content -match '(?s)<AssemblyName>\s*(.*?)\s*</AssemblyName>') {
    $exe = Join-Path $outputDir "$($matches[1]).exe"
}
if (-not $exe -or -not (Test-Path $exe)) {
    $exe = (Get-ChildItem $outputDir -Filter "*.exe" | Select-Object -First 1).FullName
}
if (-not $exe -or -not (Test-Path $exe)) {
    Write-Host "WARNING: No .exe found in $outputDir -- skipping run" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
if ($Detach) {
    Write-Host "--> Launching Uno Desktop app in background..." -ForegroundColor Cyan
    Write-Host "    exe: $exe" -ForegroundColor DarkGray
    $proc = Start-Process -FilePath $exe -PassThru
    Write-Host "    PID: $($proc.Id)"
} else {
    Write-Host "--> Launching Uno Desktop app: $exe" -ForegroundColor Cyan
    Write-Host "    The script will stay running while the app is open." -ForegroundColor DarkGray
    Write-Host ""
    & $exe
}

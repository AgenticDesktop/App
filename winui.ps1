<#
.SYNOPSIS
Builds and runs the WinUI 3 (net10.0-windows) target.

.DESCRIPTION
Builds only the Windows App SDK target framework and launches it via
`winapp run` (MSIX packaged). Requires Windows Developer Mode.

.EXAMPLE
.\winui.ps1                  # Build + run (foreground, --debug-output)
.\winui.ps1 -SkipRun         # Build only
.\winui.ps1 -Detach          # Build + launch in background (returns immediately)
.\winui.ps1 -Symbols         # Build + run with --debug-output --symbols
.\winui.ps1 /p:Configuration=Release
#>
param(
    [Parameter(Position = 0)]
    [string]$Project,
    [switch]$SkipRun,
    [switch]$Detach,
    [switch]$Symbols,
    [switch]$UseMSBuild,
    [Parameter(ValueFromRemainingArguments)]
    [string[]]$ExtraArgs
)

$ErrorActionPreference = 'Stop'

# Disable MSBuild Server: Uno's TaskHostFactory tasks race with worker nodes
# on the .NET 10 preview SDK and fail intermittently with MSB4018/MSB0001.
$env:DOTNET_BUILD_USE_MSBUILD_SERVER = 'false'

# CLI-style aliases
if ($ExtraArgs -contains '--detach')   { $Detach   = $true; $ExtraArgs = $ExtraArgs | Where-Object { $_ -ne '--detach' } }
if ($ExtraArgs -contains '--symbols')  { $Symbols  = $true; $ExtraArgs = $ExtraArgs | Where-Object { $_ -ne '--symbols' } }
if ($ExtraArgs -contains '--use-msbuild') { $UseMSBuild = $true; $ExtraArgs = $ExtraArgs | Where-Object { $_ -ne '--use-msbuild' } }
$extraArgs = $ExtraArgs

# -- 0. Developer Mode check (required for MSIX deployment) --
$devMode = $false
try {
    $regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock"
    if (Test-Path $regPath) {
        $val = Get-ItemProperty $regPath -Name AllowDevelopmentWithoutDevLicense -ErrorAction SilentlyContinue
        if ($val.AllowDevelopmentWithoutDevLicense -eq 1) { $devMode = $true }
    }
} catch {}
if (-not $devMode) {
    Write-Host "ERROR: Developer Mode is not enabled (required for MSIX deployment)." -ForegroundColor Red
    Write-Host "Enable it: Settings > System > For developers > Developer Mode" -ForegroundColor Yellow
    exit 1
}

# -- 1. Find csproj --
if (-not $Project) {
    $csprojFiles = Get-ChildItem -Path . -Filter "*.csproj" -Depth 0
    if ($csprojFiles.Count -eq 1) {
        $Project = $csprojFiles[0].Name
    } elseif ($csprojFiles.Count -gt 1) {
        Write-Error "Multiple .csproj files found. Specify: .\winui.ps1 <name>.csproj"
        exit 1
    } else {
        Write-Error "No .csproj file found in current directory."
        exit 1
    }
}

# -- 2. Detect Windows TFM from csproj --
$content = Get-Content $Project -Raw
$tfms = @()
if ($content -match '(?s)<TargetFrameworks>\s*(.*?)\s*</TargetFrameworks>') {
    $tfms = $matches[1] -split ';' | ForEach-Object { $_.Trim() } | Where-Object { $_ }
}
$winTfm = $tfms | Where-Object { $_ -match '-windows' } | Select-Object -First 1
if (-not $winTfm) {
    Write-Error "No Windows TFM (net*-windows*) found in $Project"
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

# -- 5. Build (Windows TFM only) --
Write-Host ""
Write-Host "--> Building WinUI target ($winTfm, Platform: $detectedPlatform, Config: $detectedConfig)" -ForegroundColor Cyan
$buildExit = 0
try {
    if ($msbuild) {
        $allArgs = @("/nologo", "/v:m", "/m:1",
            "/p:Platform=$detectedPlatform", "/p:Configuration=$detectedConfig",
            "/p:TargetFramework=$winTfm", "/restore", $Project) + $userArgs
        & $msbuild $allArgs
        $buildExit = $LASTEXITCODE
    } else {
        $dotnetArgs = @($Project, "-nologo", "-v:m", "-m:1", "-f", $winTfm,
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

# -- 6. Run with winapp --
if ($SkipRun) { Write-Host "--> Skipping run (-SkipRun)" -ForegroundColor DarkGray; exit 0 }

$rid = $detectedPlatform.ToLower()
$projectDir = Split-Path (Resolve-Path $Project) -Parent
if (-not $projectDir) { $projectDir = "." }
$binDir = Join-Path $projectDir "bin\$detectedPlatform\$detectedConfig"
if (-not (Test-Path $binDir)) {
    Write-Host "WARNING: Build output not found at $binDir -- skipping run" -ForegroundColor Yellow
    exit 0
}

# Find the Windows TFM folder, then win-<rid> subfolder
$tfmDir = Get-ChildItem $binDir -Directory | Where-Object { $_.Name -match "^net\d.*-windows" } |
    Sort-Object Name -Descending | Select-Object -First 1
if (-not $tfmDir) {
    Write-Host "WARNING: No Windows TFM folder in $binDir -- skipping run" -ForegroundColor Yellow
    exit 0
}
$outputDir = Join-Path $tfmDir.FullName "win-$rid"
if (-not (Test-Path $outputDir)) { $outputDir = $tfmDir.FullName }

$winapp = Get-Command winapp -ErrorAction SilentlyContinue
if (-not $winapp) {
    Write-Host "WARNING: winapp CLI not found in PATH -- skipping run" -ForegroundColor Yellow
    Write-Host "Build output at: $outputDir"
    exit 0
}

Write-Host ""
if ($Detach) {
    Write-Host "--> Launching WinUI app in background..." -ForegroundColor Cyan
    & winapp run $outputDir --detach --json
} else {
    $runArgs = @($outputDir, '--debug-output')
    if ($Symbols) { $runArgs += '--symbols' }
    Write-Host "--> Launching WinUI app: winapp run $($runArgs -join ' ')" -ForegroundColor Cyan
    Write-Host "    The script will stay running while the app is open." -ForegroundColor DarkGray
    Write-Host ""
    & winapp run @runArgs
}

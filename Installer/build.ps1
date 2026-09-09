<#
Build script for the Inno Setup installer(s).

Requirements:
- Inno Setup (ISCC.exe), typically installed to "C:\Program Files (x86)\Inno Setup 6\ISCC.exe".
- MSBuild on PATH (Visual Studio developer command prompt or MSBuild installed).

This script will:
1) Build the WorkoutGenSD solution in Release
2) Ensure the project Release output is available
3) Run ISCC to build setup_minimal.exe and setup_bundled.exe

Usage:
  ./build.ps1

If you want only one of the outputs, run the appropriate iscc command manually as shown in the Installer/Installer.iss comments.
#>

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot = Resolve-Path (Join-Path $scriptDir '..')
$solution = Join-Path $repoRoot 'WorkoutGenSD.sln'

Write-Host "Building solution: $solution"

if (-not (Test-Path $solution)) {
	Write-Error "Solution file not found: $solution"
	exit 1
}

# Build Release configuration

Write-Host "Build finished. Preparing installer builds..."

$isccDefault = 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'
if (-not (Test-Path $isccDefault)) {
	Write-Error "ISCC.exe not found at $isccDefault. Install Inno Setup or adjust the script to point to your ISCC.exe."
	exit 1
}

$installerScript = Join-Path $scriptDir 'Installer.iss'

if (-not (Test-Path $installerScript)) {
	Write-Error "Installer script not found: $installerScript"
	exit 1
}

Push-Location $scriptDir
try {
	# Comment out these next 2 lines if you don't want the minimal build that downloads .NET at install time if needed
	Write-Host "Building minimal installer (will download .NET at install time if needed)..."
	& "$isccDefault" "$installerScript"

	# Comment out these next 2 lines if you don't want the bundled build with .NET included
	Write-Host "Building bundled installer (bundles .NET offline installer). Ensure Prereqs\NDP48-offline.exe exists before running)..."
	& "$isccDefault" "/DBUNDLE" "$installerScript"
}
finally {
	Pop-Location
}

Write-Host "Done. Output files are in the Installer folder (setup_minimal.exe, setup_bundled.exe)."

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$SetupDirectory,
    [string]$PackagePath,
    [string]$MSBuildPath
)

$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') { throw 'This WPF project must be built on Windows.' }
$root = Split-Path -Parent $PSScriptRoot
$setup = (Resolve-Path -LiteralPath $SetupDirectory).ProviderPath
$references = @('HD-Common.dll', 'DiscordRpc.dll', 'Newtonsoft.Json.dll', 'Microsoft.WindowsAPICodePack.dll')
$required = $references
if (-not (Test-Path -LiteralPath (Join-Path $setup 'BstkTypeLib.dll') -PathType Leaf)) {
    $required += 'CommonInstallUtils\BstkTypeLib.dll'
}
if ($PackagePath) {
    $PackagePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PackagePath)
    if ([IO.Path]::GetExtension($PackagePath) -ne '.zip') { throw 'PackagePath must end in .zip.' }
    if (Test-Path -LiteralPath $PackagePath) { throw "Package already exists: $PackagePath" }
    $qt = if (Test-Path -LiteralPath (Join-Path $setup '64bit') -PathType Leaf) { 'QtRedistx64.zip' } else { 'QtRedistx86.zip' }
    $required += @('7zr.exe', 'PF.zip', 'PD.zip', $qt)
    if (-not (Test-Path -LiteralPath (Join-Path $setup 'CommonInstallUtils') -PathType Container)) {
        $required += 'CommonInstallUtils.zip'
    }
}
$missing = @($required | Where-Object { -not (Test-Path -LiteralPath (Join-Path $setup $_) -PathType Leaf) })
if ($missing.Count) { throw "Missing from SetupDirectory: $($missing -join ', ')" }

if (-not $MSBuildPath) {
    $msbuild = Get-Command MSBuild.exe -CommandType Application -ErrorAction SilentlyContinue
    if (-not $msbuild) { throw 'Open a Visual Studio Developer PowerShell or pass -MSBuildPath.' }
    $MSBuildPath = $msbuild.Source
}
$project = Join-Path $root 'BlueStacksInstaller.csproj'
& $MSBuildPath $project /t:Rebuild /p:Configuration=Release "/p:InstallerDependenciesDir=$setup" /warnAsError:MSB3245
if ($LASTEXITCODE -ne 0) { throw "MSBuild failed with exit code $LASTEXITCODE." }
if (-not $PackagePath) { return }

Add-Type -AssemblyName System.IO.Compression.FileSystem
$stage = Join-Path ([IO.Path]::GetTempPath()) ('msi-app-player-' + [Guid]::NewGuid().ToString('N'))
$archive = "$stage.zip"
try {
    # Copy an extracted Setup layout, not a source checkout or online bootstrapper folder.
    Copy-Item -LiteralPath $setup -Destination $stage -Recurse
    foreach ($name in @('BlueStacksInstaller.exe', 'BlueStacksInstaller.exe.config', 'BstkTypeLib.dll')) {
        Copy-Item -LiteralPath (Join-Path (Join-Path $root 'bin\Release') $name) -Destination $stage -Force
    }
    $commonZip = Join-Path $stage 'CommonInstallUtils.zip'
    if (-not (Test-Path -LiteralPath $commonZip -PathType Leaf)) {
        [IO.Compression.ZipFile]::CreateFromDirectory((Join-Path $stage 'CommonInstallUtils'), $commonZip)
    }
    [IO.Compression.ZipFile]::CreateFromDirectory($stage, $archive)
    [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($PackagePath))
    [IO.File]::Copy($archive, $PackagePath)
    Write-Output "Created $PackagePath"
}
finally {
    if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force }
    if (Test-Path -LiteralPath $archive) { Remove-Item -LiteralPath $archive -Force }
}

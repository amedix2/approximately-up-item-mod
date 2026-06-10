$ErrorActionPreference = 'Stop'

function Find-GameRoot {
    param([string]$StartPath)

    $current = (Resolve-Path -LiteralPath $StartPath).Path
    if ((Get-Item -LiteralPath $current) -is [System.IO.FileInfo]) {
        $current = Split-Path -Parent $current
    }

    for ($i = 0; $i -lt 6 -and $current; $i++) {
        if (Test-Path -LiteralPath (Join-Path $current 'ApproximatelyUp.exe')) {
            return $current
        }

        $parent = Split-Path -Parent $current
        if ($parent -eq $current) {
            break
        }

        $current = $parent
    }

    throw 'Could not find ApproximatelyUp.exe. Put this script in the game folder, Mods folder, or the cloned source folder inside the game folder.'
}

function Get-AllTypes {
    param($Types)

    foreach ($type in $Types) {
        $type
        if ($type.HasNestedTypes) {
            Get-AllTypes $type.NestedTypes
        }
    }
}

$gameRoot = Find-GameRoot -StartPath $PSScriptRoot
$assemblyPath = Join-Path $gameRoot 'MelonLoader\Il2CppAssemblies\UnityEngine.CoreModule.dll'
$cecilPath = Join-Path $gameRoot 'MelonLoader\net6\Mono.Cecil.dll'

if (!(Test-Path -LiteralPath $cecilPath)) {
    throw "Mono.Cecil.dll was not found: $cecilPath. Install MelonLoader first."
}

if (!(Test-Path -LiteralPath $assemblyPath)) {
    throw "UnityEngine.CoreModule.dll was not found: $assemblyPath. Start the game with MelonLoader once, close it, then run this script."
}

$backupPath = $assemblyPath + '.dupfix-backup'
if (!(Test-Path -LiteralPath $backupPath)) {
    Copy-Item -LiteralPath $assemblyPath -Destination $backupPath
}

Add-Type -Path $cecilPath

$bytes = [System.IO.File]::ReadAllBytes($assemblyPath)
$stream = New-Object System.IO.MemoryStream @(,$bytes)
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($stream)
$renamed = 0

foreach ($type in (Get-AllTypes $assembly.MainModule.Types | Where-Object { $_.Name -eq '<>O' })) {
    $renamed++
    $owner = $type.DeclaringType.FullName -replace '[^A-Za-z0-9_]+', '_'
    $type.Name = '<>O_' + $owner + '_' + $renamed
}

try {
    if ($renamed -gt 0) {
        $tmpPath = $assemblyPath + '.tmp'
        if (Test-Path -LiteralPath $tmpPath) {
            Remove-Item -LiteralPath $tmpPath -Force
        }

        $assembly.Write($tmpPath)
        Copy-Item -LiteralPath $tmpPath -Destination $assemblyPath -Force
        Remove-Item -LiteralPath $tmpPath -Force
    }
}
finally {
    $assembly.Dispose()
    $stream.Dispose()
}

Write-Host "Game folder: $gameRoot"
Write-Host "Renamed nested <>O types: $renamed"
Write-Host "Backup: $backupPath"

if ($renamed -eq 0) {
    Write-Host 'Nothing to patch. The generated UnityEngine.CoreModule.dll already looks fixed.'
} else {
    Write-Host 'Patch complete. Start the game again.'
}

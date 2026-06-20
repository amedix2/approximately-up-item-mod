$ErrorActionPreference = 'Stop'
$script:LogPath = Join-Path $PSScriptRoot 'PatchUnityCoreModule.log'
Set-Content -LiteralPath $script:LogPath -Value "Patch started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"

function Find-GameRoot {
    param([string]$StartPath)

    $current = (Resolve-Path -LiteralPath $StartPath).Path
    if ((Get-Item -LiteralPath $current) -is [System.IO.FileInfo]) {
        $current = Split-Path -Parent $current
    }

    for ($i = 0; $i -lt 8 -and $current; $i++) {
        if (Test-Path -LiteralPath (Join-Path $current 'ApproximatelyUp.exe')) {
            return $current
        }

        $parent = Split-Path -Parent $current
        if ($parent -eq $current) {
            break
        }

        $current = $parent
    }

    throw 'Could not find ApproximatelyUp.exe. Extract the release archive into the Approximately Up Demo game folder, then run this script from there.'
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

function Write-Log {
    param([string]$Message)

    Write-Host $Message
    if ($script:LogPath) {
        Add-Content -LiteralPath $script:LogPath -Value $Message
    }
}

function Patch-Assembly {
    param(
        [string]$AssemblyPath,
        [Mono.Cecil.IAssemblyResolver]$AssemblyResolver
    )

    $bytes = [System.IO.File]::ReadAllBytes($AssemblyPath)
    $stream = New-Object System.IO.MemoryStream @(,$bytes)
    $readerParameters = New-Object Mono.Cecil.ReaderParameters
    $readerParameters.AssemblyResolver = $AssemblyResolver
    $assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($stream, $readerParameters)
    $renamed = 0

    foreach ($type in (Get-AllTypes $assembly.MainModule.Types | Where-Object { $_.Name -eq '<>O' })) {
        $renamed++
        $owner = $type.DeclaringType.FullName -replace '[^A-Za-z0-9_]+', '_'
        $type.Name = '<>O_' + $owner + '_' + $renamed
    }

    try {
        if ($renamed -gt 0) {
            $backupPath = $AssemblyPath + '.dupfix-backup'
            if (!(Test-Path -LiteralPath $backupPath)) {
                Copy-Item -LiteralPath $AssemblyPath -Destination $backupPath
            }

            $tmpPath = $AssemblyPath + '.tmp'
            if (Test-Path -LiteralPath $tmpPath) {
                Remove-Item -LiteralPath $tmpPath -Force
            }

            $assembly.Write($tmpPath)
            Copy-Item -LiteralPath $tmpPath -Destination $AssemblyPath -Force
            Remove-Item -LiteralPath $tmpPath -Force
        }
    }
    finally {
        $assembly.Dispose()
        $stream.Dispose()
    }

    return $renamed
}

try {
    $gameRoot = Find-GameRoot -StartPath $PSScriptRoot
    $gameLogPath = Join-Path $gameRoot 'PatchUnityCoreModule.log'
    if ($gameLogPath -ne $script:LogPath) {
        Copy-Item -LiteralPath $script:LogPath -Destination $gameLogPath -Force
        $script:LogPath = $gameLogPath
    }

    Write-Log "Game folder: $gameRoot"

    if (Get-Process ApproximatelyUp -ErrorAction SilentlyContinue) {
        throw 'Approximately Up is currently running. Close the game, then run this script again.'
    }

    $cecilPath = Join-Path $gameRoot 'MelonLoader\net6\Mono.Cecil.dll'
    $assembliesPath = Join-Path $gameRoot 'MelonLoader\Il2CppAssemblies'

    if (!(Test-Path -LiteralPath $cecilPath)) {
        throw "Mono.Cecil.dll was not found: $cecilPath. Install MelonLoader first."
    }

    if (!(Test-Path -LiteralPath $assembliesPath)) {
        throw "Il2CppAssemblies folder was not found: $assembliesPath. Start the game with MelonLoader once, close it, then run this script."
    }

    Add-Type -Path $cecilPath

    $resolver = New-Object Mono.Cecil.DefaultAssemblyResolver
    $resolver.AddSearchDirectory($assembliesPath)
    $resolver.AddSearchDirectory((Join-Path $gameRoot 'MelonLoader\net6'))

    $totalRenamed = 0
    $patchedAssemblies = 0
    $assemblies = Get-ChildItem -LiteralPath $assembliesPath -Filter 'UnityEngine*.dll' -File

    foreach ($assemblyFile in $assemblies) {
        $renamed = Patch-Assembly -AssemblyPath $assemblyFile.FullName -AssemblyResolver $resolver
        if ($renamed -gt 0) {
            $patchedAssemblies++
            $totalRenamed += $renamed
            Write-Log "Patched $($assemblyFile.Name): renamed $renamed nested <>O type(s)."
        }
    }

    Write-Log "Patched assemblies: $patchedAssemblies"
    Write-Log "Renamed nested <>O types: $totalRenamed"

    if ($totalRenamed -eq 0) {
        Write-Log 'Nothing to patch. If MelonLoader still says "No Support Module Loaded", send PatchUnityCoreModule.log and MelonLoader\Latest.log to the mod author.'
    } else {
        Write-Log 'Patch complete. Start the game again.'
    }
}
catch {
    Write-Log "ERROR: $($_.Exception.Message)"
    Write-Log 'Please send PatchUnityCoreModule.log and MelonLoader\Latest.log to the mod author.'
}
finally {
    if ($script:LogPath) {
        Write-Host "Log saved to: $script:LogPath"
    }

    Write-Host ''
    Read-Host 'Press Enter to close this window'
}

[CmdletBinding()]
param(
    [string] $OutFile = 'manifest.json'
)

$ErrorActionPreference = 'Stop'
$root = [System.IO.Path]::GetFullPath($PSScriptRoot)
$output = [System.IO.Path]::GetFullPath((Join-Path $root $OutFile))
if ([System.IO.Path]::GetDirectoryName($output) -ne $root) {
    throw 'Manifest output must be directly in the repository root.'
}

$cases = Get-Content -LiteralPath (Join-Path $root 'cases.json') -Raw | ConvertFrom-Json
$utf8 = [System.Text.UTF8Encoding]::new($false, $true)
$files = foreach ($case in $cases.files) {
    $physical = [System.IO.Path]::GetFullPath((Join-Path $root $case.path))
    if (-not $physical.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Fixture escapes repository: $($case.path)"
    }
    $bytes = [System.IO.File]::ReadAllBytes($physical)
    $text = $utf8.GetString($bytes)
    if ($text.StartsWith([char]0xfeff)) {
        $text = $text.Substring(1)
    }
    $expected = foreach ($label in $case.expected) {
        if ([string]::IsNullOrWhiteSpace($label.value) -or $label.occurrence -lt 1) {
            throw "Invalid occurrence label in $($case.path)"
        }
        $offset = -1
        $next = 0
        for ($i = 0; $i -lt $label.occurrence; $i++) {
            $offset = $text.IndexOf($label.value, $next, [System.StringComparison]::Ordinal)
            if ($offset -lt 0) { throw "Missing labeled occurrence in $($case.path): $($label.value)" }
            $next = $offset + $label.value.Length
        }
        [ordered]@{
            originalDomain = $label.value
            offsetUtf16 = $offset
            lengthUtf16 = $label.value.Length
            role = $label.role
            explanation = $label.explanation
        }
    }
    [ordered]@{
        path = $case.path
        sha256 = [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes))
        expected = @($expected)
    }
}
$manifest = [ordered]@{
    selectionDescription = $cases.selectionDescription
    annotationMethod = $cases.annotationMethod
    files = @($files)
}
$content = ($manifest | ConvertTo-Json -Depth 10) + "`n"
$stream = [System.IO.File]::Open($output, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write)
try {
    $bytes = $utf8.GetBytes($content)
    $stream.Write($bytes)
}
finally {
    $stream.Dispose()
}
Write-Output "Created $([System.IO.Path]::GetFileName($output)) with $($files.Count) hashed fixtures."

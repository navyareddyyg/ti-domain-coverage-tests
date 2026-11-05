[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ReportPath
)

$ErrorActionPreference = 'Stop'
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'manifest.json') -Raw | ConvertFrom-Json
$report = Get-Content -LiteralPath $ReportPath -Raw | ConvertFrom-Json
$destinations = @($manifest.files.expected | Where-Object role -eq 'destination').Count
if ($report.annotatedDestinationOccurrences -ne $destinations -or
    $report.admittedAnnotatedDestinations -ne $destinations -or
    $report.admittedAnnotatedNonDestinations -ne 0 -or
    @($report.unannotatedAdmissions).Count -ne 0) {
    throw 'Destination or rejection acceptance failed.'
}
if (@($report.files).Count -ne @($manifest.files).Count) {
    throw 'Report does not cover every fixture.'
}
foreach ($file in $manifest.files) {
    $physical = Join-Path $PSScriptRoot $file.path
    $currentHash = (Get-FileHash -LiteralPath $physical -Algorithm SHA256).Hash
    $measured = @($report.files | Where-Object path -eq $file.path)
    if ($measured.Count -ne 1 -or $measured[0].sha256 -ne $file.sha256 -or $currentHash -ne $file.sha256) {
        throw "Source identity mismatch: $($file.path)"
    }
    if ($null -eq $measured[0].candidates) {
        throw "Missing candidate records: $($file.path)"
    }
    $text = [System.IO.File]::ReadAllText($physical, [System.Text.UTF8Encoding]::new($false, $true))
    $labels = [System.Collections.Generic.Dictionary[string, object]]::new([System.StringComparer]::Ordinal)
    foreach ($label in $file.expected) {
        $key = "$($label.offsetUtf16):$($label.lengthUtf16)"
        if (-not $labels.TryAdd($key, $label)) {
            throw "Duplicate annotation span: $($file.path)"
        }
    }
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $admitted = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($candidate in $measured[0].candidates) {
        $source = $candidate.source
        $offset = $source.offsetUtf16
        $length = $source.lengthUtf16
        $key = "$($offset):$($length)"
        if ($source.path -cne $file.path -or $source.sha256 -ne $file.sha256 -or
            $offset -lt 0 -or $length -lt 1 -or $offset -gt $text.Length - $length -or
            $text.Substring($offset, $length) -cne $candidate.originalDomain -or
            -not $seen.Add($key)) {
            throw "Invalid or duplicate candidate provenance: $($file.path)"
        }
        if ($candidate.admitted) {
            if (-not $labels.ContainsKey($key) -or $labels[$key].role -ne 'destination' -or
                $labels[$key].originalDomain -cne $candidate.originalDomain) {
                throw "Unexpected or non-destination admission: $($file.path)"
            }
            [void] $admitted.Add($key)
        }
    }
    foreach ($label in $file.expected | Where-Object role -eq 'destination') {
        if (-not $admitted.Contains("$($label.offsetUtf16):$($label.lengthUtf16)")) {
            throw "Missing destination admission: $($file.path)"
        }
    }
}
$baseline = @($report.files.baseline.lookupUrl | Sort-Object)
$expectedBaseline = @(
    'https://api.github.com/repos/octocat/Hello-World'
    'https://api.github.com/rate_limit'
    'https://api.github.com/zen'
)
if ($baseline.Count -ne $expectedBaseline.Count -or
    @((Compare-Object $expectedBaseline $baseline)).Count -ne 0) {
    throw 'Independent full-URL baseline paths were lost or changed.'
}
if (@($report.lookups | Where-Object reputationState -ne 'not-looked-up').Count -ne 0) {
    throw 'Fixture run must remain offline with no reputation verdicts.'
}
Write-Output "PASS: $destinations destinations admitted; no labeled negatives or unannotated values admitted; full URLs and source hashes preserved."

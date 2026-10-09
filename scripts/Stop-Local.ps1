$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$processFile = Join-Path $projectRoot '.local\processes.json'
if (-not (Test-Path -LiteralPath $processFile)) {
    Write-Host 'No CampusFix process record was found.'
    return
}

$records = Get-Content -LiteralPath $processFile -Raw | ConvertFrom-Json
$failedRecords = @()
foreach ($record in $records) {
    $process = Get-Process -Id $record.Id -ErrorAction SilentlyContinue
    if ($process -and $process.ProcessName -eq 'dotnet' -and $process.StartTime.ToUniversalTime().Ticks -eq ([datetime]$record.StartedAt).ToUniversalTime().Ticks) {
        # Only stop the original processes, never an unrelated process reusing a saved ID.
        & taskkill.exe /PID $process.Id /T /F | Out-Null
        if ($LASTEXITCODE -ne 0) {
            $failedRecords += $record
            Write-Warning ('Could not stop process ' + $process.Id + '. Its record was kept so you can retry.')
        }
    }
}
if ($failedRecords.Count -gt 0) {
    $failedRecords | ConvertTo-Json | Set-Content -LiteralPath $processFile -Encoding UTF8
    Write-Warning 'Some CampusFix processes could not be stopped. Retry this script.'
} else {
    Remove-Item -LiteralPath $processFile
    Write-Host 'CampusFix stopped. Your local accounts and reports are saved.'
}

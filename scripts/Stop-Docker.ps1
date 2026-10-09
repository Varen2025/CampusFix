$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Docker-Common.ps1')

try {
    $context = Get-CampusFixDockerContext
    if (-not (Test-Path -LiteralPath $context.Settings)) {
        throw 'The private .env file is missing from this project folder. Restore it before stopping this project.'
    }
    $engine = Invoke-CampusFixDockerCapture $context.Docker @('info', '--format', '{{.OSType}}')
    if ($engine.ExitCode -ne 0) {
        throw 'Docker is not responding. Open Docker Desktop so this button can stop the saved CampusFix containers.'
    }
    Write-Host 'Stopping only the CampusFix Docker containers...'
    # Never add --volumes here: the database must survive stopping the app.
    Invoke-CampusFixDocker $context.Docker ($context.Arguments + @('down'))
    Write-Host 'CampusFix Docker containers are stopped. Your accounts and reports are saved.' -ForegroundColor Green
    Write-Host 'Docker Desktop may still be running. To close it too, right-click its taskbar tray icon and choose Quit Docker Desktop.'
} catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

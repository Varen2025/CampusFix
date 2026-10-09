param([switch]$NoOpen)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Docker-Common.ps1')

try {
    $context = Get-CampusFixDockerContext
    Write-Host 'Starting CampusFix with Docker. No terminal commands are needed.'
    if (-not (Test-Path -LiteralPath $context.Settings)) {
        & (Join-Path $PSScriptRoot 'Setup-Docker.ps1')
    }

    $engine = Invoke-CampusFixDockerCapture $context.Docker @('info', '--format', '{{.OSType}}')
    if ($engine.ExitCode -ne 0) {
        $desktop = @(
            (Join-Path $env:LOCALAPPDATA 'Programs\DockerDesktop\Docker Desktop.exe'),
            (Join-Path $env:ProgramFiles 'Docker\Docker\Docker Desktop.exe')
        ) | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
        if (-not $desktop) { throw 'Open Docker Desktop and wait for its engine to run, then try again.' }
        Write-Host 'Opening Docker Desktop. Waiting for its engine...'
        Start-Process -FilePath $desktop -WindowStyle Hidden
        $deadline = [DateTime]::UtcNow.AddSeconds(90)
        do {
            Start-Sleep -Seconds 2
            $engine = Invoke-CampusFixDockerCapture $context.Docker @('info', '--format', '{{.OSType}}')
        } while ($engine.ExitCode -ne 0 -and [DateTime]::UtcNow -lt $deadline)
        if ($engine.ExitCode -ne 0) {
            throw 'Docker has not started. Open Docker Desktop and read its error message, then try again.'
        }
    }
    if ($engine.Output -ne 'linux') { throw 'Choose Linux containers in Docker Desktop, then try again.' }

    $validation = Invoke-CampusFixDockerCapture $context.Docker ($context.Arguments + @('config', '--quiet'))
    if ($validation.ExitCode -ne 0) { throw $validation.Output }

    # The local launcher records its own process IDs and start times. It will not stop unrelated apps.
    & (Join-Path $PSScriptRoot 'Stop-Local.ps1')
    Write-Host 'Building and starting the website, API and database. The first run can take several minutes.'
    Invoke-CampusFixDocker $context.Docker ($context.Arguments + @('up', '--build', '--detach'))

    Write-Host 'Checking that the website and database connection are ready...'
    $deadline = [DateTime]::UtcNow.AddSeconds(90)
    $ready = $false
    do {
        try {
            $api = Invoke-WebRequest -Uri 'http://localhost:5081/health' -UseBasicParsing -TimeoutSec 3
            $web = Invoke-WebRequest -Uri 'http://localhost:5080/health' -UseBasicParsing -TimeoutSec 3
            $ready = $api.StatusCode -eq 200 -and $web.StatusCode -eq 200
        } catch { $ready = $false }
        if (-not $ready) { Start-Sleep -Seconds 2 }
    } while (-not $ready -and [DateTime]::UtcNow -lt $deadline)
    if (-not $ready) {
        Invoke-CampusFixDocker $context.Docker ($context.Arguments + @('ps'))
        throw 'CampusFix is not ready yet. Check the api and web containers in Docker Desktop. Stop-Docker.cmd safely stops this project.'
    }

    Write-Host ''
    Write-Host 'CampusFix is ready: http://localhost:5080' -ForegroundColor Green
    Write-Host 'Register a student account on the website.'
    Write-Host 'For staff login, open .env in Visual Studio and use SEED_STAFF_EMAIL and SEED_STAFF_PASSWORD.'
    Write-Host 'Closing this window does not stop the website.'
    Write-Host 'When finished, double-click Stop-Docker.cmd. Your accounts and reports will be saved.'
    if (-not $NoOpen) { Start-Process 'http://localhost:5080' }
} catch {
    Write-Host ''
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

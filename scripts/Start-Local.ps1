param(
    [switch]$NoBuild,
    [switch]$NoOpen
)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$localFolder = Join-Path $projectRoot '.local'
New-Item -ItemType Directory -Force -Path $localFolder | Out-Null

function New-LocalSecret {
    $bytes = New-Object byte[] 32
    $generator = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $generator.GetBytes($bytes) } finally { $generator.Dispose() }
    return [BitConverter]::ToString($bytes).Replace('-', '').ToLowerInvariant()
}

function Test-LocalPort([int]$Port) {
    $client = New-Object Net.Sockets.TcpClient
    try {
        $task = $client.ConnectAsync('127.0.0.1', $Port)
        return ($task.Wait(1000) -and $client.Connected)
    } catch { return $false } finally { $client.Dispose() }
}

function Wait-ForService([string]$Url, [Diagnostics.Process]$Process, [string]$ErrorLog) {
    for ($attempt = 0; $attempt -lt 90; $attempt++) {
        $Process.Refresh()
        if ($Process.HasExited) {
            throw "The service stopped. Read $ErrorLog and the matching output log."
        }
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2
            if ($response.StatusCode -eq 200) { return }
        } catch { }
        Start-Sleep -Seconds 1
    }
    throw "The service did not become ready at $Url. Read the logs in $localFolder."
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'Install the .NET 10 SDK first. See README.md.'
}
foreach ($port in @(5080, 5081)) {
    if (Test-LocalPort $port) {
        throw "Port $port is already in use. If CampusFix is running, visit http://localhost:5080. To restart it, run scripts\Stop-Local.ps1 first."
    }
}

if (-not $NoBuild) {
    & dotnet build (Join-Path $projectRoot 'CampusFix.App.slnf') --configuration Debug --nologo
    if ($LASTEXITCODE -ne 0) { throw 'The build failed. Fix the error shown above before starting the website.' }
}

$settingsPath = Join-Path $localFolder 'local-settings.json'
if (-not (Test-Path -LiteralPath $settingsPath)) {
    @{
        JwtKey = New-LocalSecret
        StaffEmail = 'staff@campusfix.local'
        StaffPassword = ('Cf!' + (New-LocalSecret).Substring(0, 24))
    } | ConvertTo-Json | Set-Content -LiteralPath $settingsPath -Encoding UTF8
}
$settings = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
$environmentNames = @('ASPNETCORE_ENVIRONMENT', 'DbProvider', 'ConnectionStrings__CampusFix', 'Jwt__Key', 'Seed__StaffEmail', 'Seed__StaffPassword', 'Api__BaseUrl')
$previousEnvironment = @{}
foreach ($name in $environmentNames) { $previousEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
$startedProcesses = @()

try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:DbProvider = 'Sqlite'
    $env:ConnectionStrings__CampusFix = 'Data Source=' + (Join-Path $localFolder 'campusfix.db')
    $env:Jwt__Key = $settings.JwtKey
    $env:Seed__StaffEmail = $settings.StaffEmail
    $env:Seed__StaffPassword = $settings.StaffPassword
    $env:Api__BaseUrl = 'http://localhost:5081/'

    foreach ($service in @(@{ Name = 'Api'; Port = 5081 }, @{ Name = 'Web'; Port = 5080 })) {
        $serviceProject = Join-Path $projectRoot ('src\CampusFix.' + $service.Name + '\CampusFix.' + $service.Name + '.csproj')
        $errorLog = Join-Path $localFolder ($service.Name.ToLowerInvariant() + '.error.log')
        $outputLog = Join-Path $localFolder ($service.Name.ToLowerInvariant() + '.output.log')
        $serviceFolder = Split-Path -Parent $serviceProject
        $assemblyPath = Join-Path $serviceFolder ('bin\Debug\net10.0\CampusFix.' + $service.Name + '.dll')
        if (-not (Test-Path -LiteralPath $assemblyPath)) { throw 'Build files are missing. Run Start-Local.ps1 without -NoBuild.' }
        $arguments = '"' + $assemblyPath + '" --urls http://localhost:' + $service.Port
        $process = Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList $arguments -WorkingDirectory $serviceFolder -WindowStyle Hidden -RedirectStandardOutput $outputLog -RedirectStandardError $errorLog -PassThru
        $startedProcesses += $process
        Wait-ForService ('http://localhost:' + $service.Port + '/health') $process $errorLog
    }
    @($startedProcesses | ForEach-Object { @{ Id = $_.Id; StartedAt = $_.StartTime.ToUniversalTime().ToString('o') } }) |
        ConvertTo-Json | Set-Content -LiteralPath (Join-Path $localFolder 'processes.json') -Encoding UTF8

    Write-Host ''
    Write-Host 'CampusFix is ready: http://localhost:5080' -ForegroundColor Green
    Write-Host 'Create a student account using Register. Use fictional details for your class demo.'
    Write-Host ('Staff email: ' + $settings.StaffEmail)
    Write-Host ('Staff password: ' + $settings.StaffPassword)
    Write-Host ('Local credentials and data stay in ' + $localFolder + '. Never upload this folder.')
    Write-Host 'API documentation: http://localhost:5081/swagger'
    Write-Host 'Stop the app with scripts\Stop-Local.ps1.'
    if (-not $NoOpen) { Start-Process 'http://localhost:5080' }
} catch {
    foreach ($process in $startedProcesses) {
        $process.Refresh()
        if (-not $process.HasExited) { & taskkill.exe /PID $process.Id /T /F | Out-Null }
    }
    throw
} finally {
    foreach ($name in $environmentNames) { [Environment]::SetEnvironmentVariable($name, $previousEnvironment[$name], 'Process') }
}

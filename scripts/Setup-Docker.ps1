param([switch]$Quiet)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$settingsPath = Join-Path $projectRoot '.env'

if (Test-Path -LiteralPath $settingsPath) {
    Write-Host 'A .env file already exists. It has been left unchanged.'
    Write-Host 'Open .env in Visual Studio if you need to check its settings or staff login.'
    Write-Host 'Changing a password here does not change an existing database or staff account.'
    return
}

function New-DockerSecret {
    $bytes = New-Object byte[] 32
    $generator = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $generator.GetBytes($bytes) } finally { $generator.Dispose() }
    return [BitConverter]::ToString($bytes).Replace('-', '').ToLowerInvariant()
}

$databasePassword = New-DockerSecret
$signingKey = New-DockerSecret
$staffPassword = 'Cf!' + (New-DockerSecret).Substring(0, 24)
$settings = @(
    '# Private local Docker settings. Never upload this file or show it in screenshots.'
    ('POSTGRES_PASSWORD=' + $databasePassword)
    ('JWT_KEY=' + $signingKey)
    'SEED_STAFF_EMAIL=staff@campusfix.local'
    ('SEED_STAFF_PASSWORD=' + $staffPassword)
)

# CreateNew refuses to replace an existing file, even if another process just created it.
$stream = [IO.File]::Open($settingsPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
$writer = New-Object IO.StreamWriter($stream, (New-Object Text.UTF8Encoding($false)))
try { $writer.WriteLine(($settings -join [Environment]::NewLine)) } finally { $writer.Dispose() }

Write-Host 'Your Docker configuration is ready.' -ForegroundColor Green
Write-Host 'Docker itself has not been installed or started by this script.'
Write-Host 'Docker staff email: staff@campusfix.local'
if (-not $Quiet) { Write-Host ('Docker staff password: ' + $staffPassword) }
Write-Host 'This password is saved in .env. Keep it private.'
Write-Host 'Next, open CampusFix.sln in Visual Studio and run the docker-compose project.'
Write-Host 'The Docker database starts separately from the earlier local demo database.'

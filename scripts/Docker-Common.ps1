function Get-CampusFixDockerContext {
    $root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
    $command = Get-Command docker.exe -ErrorAction SilentlyContinue
    $docker = if ($command) { $command.Source } else { $null }
    if (-not $docker) {
        foreach ($candidate in @(
            (Join-Path $env:LOCALAPPDATA 'Programs\DockerDesktop\resources\bin\docker.exe'),
            (Join-Path $env:ProgramFiles 'Docker\Docker\resources\bin\docker.exe')
        )) {
            if (Test-Path -LiteralPath $candidate) { $docker = $candidate; break }
        }
    }
    if (-not $docker) { throw 'Docker Desktop is not installed. Install it, then try this button again.' }
    [pscustomobject]@{
        Root = $root
        Docker = $docker
        Settings = Join-Path $root '.env'
        Arguments = @('compose', '--project-directory', $root, '--project-name', 'campusfix',
            '--env-file', (Join-Path $root '.env'), '-f', (Join-Path $root 'docker-compose.yml'))
    }
}

function Invoke-CampusFixDockerCapture {
    param([string]$Docker, [string[]]$Arguments)
    # Windows PowerShell can otherwise treat a native program's stderr as a terminating error.
    $ErrorActionPreference = 'Continue'
    $output = & $Docker @Arguments 2>&1
    [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output | Out-String).Trim() }
}

function Invoke-CampusFixDocker {
    param([string]$Docker, [string[]]$Arguments)
    $ErrorActionPreference = 'Continue'
    & $Docker @Arguments
    if ($LASTEXITCODE -ne 0) { throw 'Docker could not complete this step. Read the message above.' }
}

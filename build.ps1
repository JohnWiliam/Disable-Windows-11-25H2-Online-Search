[CmdletBinding()]
param(
    [switch]$NoPause
)

$ErrorActionPreference = 'Stop'

function Pause-BuildScript {
    if (-not $NoPause -and [Environment]::UserInteractive) {
        Read-Host "Pressione ENTER para fechar..."
    }
}

function Resolve-DotNetExecutable {
    $dotnetCommand = Get-Command "dotnet" -ErrorAction SilentlyContinue
    if ($dotnetCommand) {
        return $dotnetCommand.Source
    }

    Write-Host "Aviso: 'dotnet' nao encontrado no PATH global." -ForegroundColor Yellow

    $defaultRoots = @(
        ${env:ProgramFiles},
        ${env:ProgramFiles(x86)}
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    foreach ($root in $defaultRoots) {
        $defaultPath = Join-Path $root "dotnet\dotnet.exe"
        if (Test-Path $defaultPath) {
            Write-Host "Encontrado em: $defaultPath. Usando este executavel." -ForegroundColor Green
            return $defaultPath
        }
    }

    throw "O .NET SDK 10 nao foi encontrado. Instale o SDK do .NET 10 e tente novamente."
}

function Assert-DotNet10Sdk {
    param([Parameter(Mandatory)][string]$DotNetExe)

    $sdkVersion = & $DotNetExe --version
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sdkVersion)) {
        throw "Nao foi possivel consultar a versao do SDK do .NET."
    }

    $sdkVersion = $sdkVersion.Trim()
    Write-Host "SDK .NET detectado: $sdkVersion" -ForegroundColor Gray

    $majorVersionText = ($sdkVersion -split '\.')[0]
    $majorVersion = 0
    if (-not [int]::TryParse($majorVersionText, [ref]$majorVersion) -or $majorVersion -lt 10) {
        throw "Este projeto requer .NET SDK 10 ou superior. Versao detectada: $sdkVersion"
    }
}

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " Disable Windows 11 Online Search - Build Script (PS)" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""

$scriptPath = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($scriptPath)) {
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
}

$projectDir = Join-Path $scriptPath "DisableWin11Search"
$projectFile = Join-Path $projectDir "DisableWin11Search.csproj"
$outputDir  = Join-Path $scriptPath "Build"

try {
    if (-not (Test-Path $projectFile)) {
        throw "Arquivo de projeto nao encontrado: $projectFile"
    }

    $dotNetExe = Resolve-DotNetExecutable
    Assert-DotNet10Sdk -DotNetExe $dotNetExe

    if (Test-Path $outputDir) {
        Write-Host "Limpando pasta de build antiga..." -ForegroundColor Gray
        Remove-Item -Path $outputDir -Recurse -Force
    }

    Write-Host "Iniciando compilacao para Windows x64..." -ForegroundColor White

    $publishArgs = @(
        "publish", $projectFile,
        "-c", "Release",
        "-r", "win-x64",
        "--self-contained", "true",
        "-p:PublishSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-p:EnableCompressionInSingleFile=true",
        "-p:EnableWindowsTargeting=true",
        "-o", $outputDir
    )

    & $dotNetExe @publishArgs
    if ($LASTEXITCODE -ne 0) {
        throw "O comando dotnet publish retornou o codigo de erro $LASTEXITCODE."
    }

    Write-Host ""
    Write-Host "========================================================" -ForegroundColor Green
    Write-Host " Build completo com sucesso!" -ForegroundColor Green
    Write-Host " Executavel gerado em: $outputDir" -ForegroundColor White
    Write-Host "========================================================" -ForegroundColor Green
    Pause-BuildScript
    exit 0
}
catch {
    Write-Host ""
    Write-Host "========================================================" -ForegroundColor Red
    Write-Host " Falha no build." -ForegroundColor Red
    Write-Host " Erro: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "========================================================" -ForegroundColor Red
    Pause-BuildScript
    exit 1
}

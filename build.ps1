# Requer privilégios de execução de script
# Para permitir, execute no terminal: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " Disable Windows 11 Online Search - Build Script (PS)" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Definição de Caminhos (Baseado na localização do script)
$ScriptPath = $PSScriptRoot
$ProjectDir = Join-Path $ScriptPath "DisableWin11Search"
$OutputDir  = Join-Path $ScriptPath "Build"

# 2. Verificação de Ambiente (Correção do erro 'dotnet não reconhecido')
if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Host "Aviso: 'dotnet' não encontrado no PATH global." -ForegroundColor Yellow
    
    # Tentativa de correção automática para instalação padrão
    $DefaultPath = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $DefaultPath) {
        Write-Host "Encontrado em: $DefaultPath. Usando este executável." -ForegroundColor Green
        $DotNetExe = $DefaultPath
    }
    else {
        Write-Host "Erro: O .NET SDK 10 não foi encontrado. Verifique a instalação." -ForegroundColor Red
        Read-Host "Pressione ENTER para sair..."
        exit
    }
}
else {
    $DotNetExe = "dotnet"
}

# 3. Limpeza (Opcional)
if (Test-Path $OutputDir) {
    Write-Host "Limpando pasta de build antiga..." -ForegroundColor Gray
    Remove-Item -Path $OutputDir -Recurse -Force
}

# 4. Execução do Build
Write-Host "Iniciando compilação para Windows x64..." -ForegroundColor White

try {
    # Argumentos replicados do build.bat original
    & $DotNetExe publish "$ProjectDir" `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -o "$OutputDir"

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================================" -ForegroundColor Green
        Write-Host " Build Completo com Sucesso!" -ForegroundColor Green
        Write-Host " Executável gerado em: $OutputDir" -ForegroundColor White
        Write-Host "========================================================" -ForegroundColor Green
    }
    else {
        throw "O comando dotnet retornou um código de erro."
    }
}
catch {
    Write-Host ""
    Write-Host "========================================================" -ForegroundColor Red
    Write-Host " Falha no Build." -ForegroundColor Red
    Write-Host " Erro: $_" -ForegroundColor Red
    Write-Host "========================================================" -ForegroundColor Red
}

Read-Host "Pressione ENTER para fechar..."

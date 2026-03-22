@echo off
setlocal

echo ============================================
echo   Branch Analyzer - Publicacao
echo ============================================
echo.

:: Limpar publicacao anterior
if exist "publish" rmdir /s /q "publish"

echo [1/3] Compilando em modo Release...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish

if errorlevel 1 (
    echo.
    echo ERRO: Falha na compilacao!
    pause
    exit /b 1
)

echo.
echo [2/3] Criando pacote ZIP...

:: Criar pasta temporaria com nome limpo
set "RELEASE_DIR=publish\BranchAnalyzer"
mkdir "%RELEASE_DIR%"
copy "publish\BranchAnalyzer.exe" "%RELEASE_DIR%\"
copy "README.md" "%RELEASE_DIR%\" 2>nul

:: Gerar ZIP usando PowerShell
powershell -Command "Compress-Archive -Path 'publish\BranchAnalyzer\*' -DestinationPath 'publish\BranchAnalyzer.zip' -Force"

echo.
echo [3/3] Concluido!
echo.
echo ============================================
echo   Arquivos gerados em: publish\
echo.
echo   BranchAnalyzer.exe  - Executavel unico
echo   BranchAnalyzer.zip  - Pacote para distribuir
echo ============================================
echo.
echo Envie o ZIP ou o .exe para sua equipe.
echo O app precisa apenas do Git instalado na maquina.
echo.

pause

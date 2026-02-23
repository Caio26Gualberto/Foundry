@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

REM Caminho final onde os executáveis serão copiados
set "PUBLISH_DIR=E:\Projetos\Pessoal\Foundry\Foundry\executors"

REM Target framework
set "TFM=net9.0"

REM Lista de runtimes x64
set RUNTIMES=win-x64 linux-x64 osx-x64

for %%R in (%RUNTIMES%) do (
    echo ================================
    echo Publicando para %%R...
    echo ================================

    dotnet publish -c Release -r %%R --self-contained true /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true

    set "SOURCE=bin\Release\%TFM%\%%R\publish\*"
    set "TARGET=%PUBLISH_DIR%\%%R"

    if not exist "!TARGET!" mkdir "!TARGET!"

    echo Copiando binários para !TARGET!...
    xcopy /Y /E "!SOURCE!" "!TARGET!\"
)

echo ================================
echo Publicação concluída para todos os runtimes x64!
echo ================================
pause
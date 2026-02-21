@echo off
title Boilerplate Customizer
echo Iniciando Boilerplate Customizer...
echo.
cd /d "%~dp0publish"
if exist "BoilerplateCustomizer.exe" (
    BoilerplateCustomizer.exe
) else (
    echo Erro: BoilerplateCustomizer.exe nao encontrado!
    echo Certifique-se de que o projeto foi compilado corretamente.
    pause
)

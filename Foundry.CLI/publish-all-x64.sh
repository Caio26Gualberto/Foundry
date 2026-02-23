#!/bin/bash
set -e

# Caminho final onde os executáveis serão copiados (pode ajustar)
PUBLISH_DIR="$(pwd)/executors"

# Target framework
TFM="net9.0"

# Lista de runtimes x64
RUNTIMES=("win-x64" "linux-x64" "osx-x64")

for RUNTIME in "${RUNTIMES[@]}"
do
    echo "==============================="
    echo "Publicando para $RUNTIME..."
    echo "==============================="

    dotnet publish -c Release -r $RUNTIME --self-contained true /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true

    SOURCE="bin/Release/$TFM/$RUNTIME/publish/*"
    TARGET="$PUBLISH_DIR/$RUNTIME"
    mkdir -p "$TARGET"

    echo "Copiando binários para $TARGET..."
    cp -r $SOURCE "$TARGET"
done

echo "==============================="
echo "Publicação concluída para Windows, Linux e macOS x64!"
echo "==============================="
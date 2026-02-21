#!/bin/bash

echo "=========================================="
echo "        BOILERPLATE MIGRATOR"
echo "=========================================="
echo

# Vai para a pasta onde está o script
cd "$(dirname "$0")"

dotnet ef database update \
  --project Boilerplate.Infra.Data \
  --startup-project Boilerplate.Infra.Data

echo
echo "Done."

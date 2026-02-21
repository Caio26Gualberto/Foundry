@echo off

echo ==========================================
echo        BOILERPLATE MIGRATOR
echo ==========================================
echo.

echo Applying EF Core migrations...
echo.

cd /d %~dp0

dotnet ef database update ^
  --project Boilerplate.Infra.Data ^
  --startup-project Boilerplate.Infra.Data

echo.
echo Done.
pause

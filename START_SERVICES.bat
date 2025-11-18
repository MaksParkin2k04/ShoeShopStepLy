@echo off
echo Запуск микросервисов ShoeShop...

echo.
echo 1. Запуск API (порт 7001)...
start "ShoeShop API" cmd /k "cd ShoeShop.API && dotnet run --urls https://localhost:7001"

timeout /t 5

echo.
echo 2. Запуск Telegram Bot + Mini App (порт 7003)...
start "Telegram Bot" cmd /k "cd ShoeShop.TelegramBot && dotnet run --urls https://localhost:7003"

timeout /t 2

echo.
echo 3. Запуск Web сайта (порт 7002)...
start "ShoeShop Web" cmd /k "cd ShoeShop && dotnet run --urls https://localhost:7002"

echo.
echo Все сервисы запущены!
echo.
echo API Swagger: https://localhost:7001/swagger
echo Web сайт: https://localhost:7002
echo.
pause
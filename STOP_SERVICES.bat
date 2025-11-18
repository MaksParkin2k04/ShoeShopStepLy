@echo off
echo Остановка всех сервисов ShoeShop...

echo Остановка процессов dotnet...
taskkill /F /IM dotnet.exe 2>nul

echo Остановка процессов ShoeShop...
taskkill /F /IM ShoeShop.exe 2>nul
taskkill /F /IM ShoeShop.API.exe 2>nul
taskkill /F /IM ShoeShop.TelegramBot.exe 2>nul
taskkill /F /IM ShoeShop.VKBot.exe 2>nul

echo.
echo Все сервисы остановлены!
pause
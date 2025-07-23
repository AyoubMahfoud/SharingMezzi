@echo off
echo ==============================================
echo AVVIO SISTEMA SHARINGMEZZI
echo ==============================================

echo.
echo Avvio del Backend API (porta 5000)...
start "Backend API" cmd /k "cd /d \"c:\Users\UNIVERSITA\Desktop\aa24-25-gruppo06-main\SharingMezzi.Api\" && dotnet run"

echo.
echo Attendo 10 secondi per l'avvio del backend...
timeout /t 10 /nobreak

echo.
echo Avvio del Frontend Web (porta 5050)...
start "Frontend Web" cmd /k "cd /d \"c:\Users\UNIVERSITA\Desktop\aa24-25-gruppo06-main\SharingMezzi.Web\" && dotnet run"

echo.
echo ==============================================
echo Sistema avviato!
echo - Backend API: http://localhost:5000
echo - Frontend Web: http://localhost:5050
echo - Admin Login: admin@sharingmezzi.it / admin123
echo ==============================================

pause

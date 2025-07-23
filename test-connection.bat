@echo off
echo =================================
echo TEST CONNESSIONE FRONTEND-BACKEND
echo =================================

echo.
echo 1. Test API Health Check...
curl -s http://localhost:5000/health
echo.

echo.
echo 2. Test creazione admin...
curl -s -X POST http://localhost:5000/api/auth/create-admin
echo.

echo.
echo 3. Test esistenza admin...
curl -s -X POST http://localhost:5000/api/test-login
echo.

echo.
echo 4. Test login admin...
curl -s -X POST http://localhost:5000/api/auth/login ^
  -H "Content-Type: application/json" ^
  -d "{\"Email\":\"admin@sharingmezzi.it\",\"Password\":\"admin123\"}"
echo.

echo.
echo =================================
echo Test completato!
echo =================================
pause

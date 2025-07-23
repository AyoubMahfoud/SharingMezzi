# 🚀 GUIDA AL TEST DEL SISTEMA SHARINGMEZZI

## Problemi risolti:
✅ Configurazione porte (Frontend 5050 → Backend 5000)
✅ CORS configurato per permettere comunicazione
✅ JWT configurazione corretta
✅ Gestione errori migliorata

## Come testare:

### OPZIONE 1 - Avvio Automatico:
1. Fai doppio clic su `start-system.bat`
2. Attendi che si aprano entrambe le finestre del terminale
3. Vai su http://localhost:5050
4. Prova il login con: admin@sharingmezzi.it / admin123

### OPZIONE 2 - Avvio Manuale:
1. Apri terminale 1 e vai in SharingMezzi.Api
   ```
   cd "c:\Users\UNIVERSITA\Desktop\aa24-25-gruppo06-main\SharingMezzi.Api"
   dotnet run
   ```

2. Apri terminale 2 e vai in SharingMezzi.Web
   ```
   cd "c:\Users\UNIVERSITA\Desktop\aa24-25-gruppo06-main\SharingMezzi.Web"
   dotnet run
   ```

### OPZIONE 3 - Test di Connessione:
Esegui `test-connection.bat` per verificare che l'API risponda

## Credenziali di test:
- **Email:** admin@sharingmezzi.it
- **Password:** admin123

## URL importanti:
- Frontend: http://localhost:5050
- Backend API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Health Check: http://localhost:5000/health

## Se continua a non funzionare:
1. Verifica che entrambi i servizi siano avviati
2. Controlla i log nel terminale per errori
3. Verifica che le porte 5000 e 5050 siano libere
4. Prova a ricompilare entrambi i progetti:
   ```
   dotnet clean
   dotnet build
   ```

## Log utili per debug:
Il sistema ora logga tutto nel terminale, inclusi:
- Tentativi di login
- Richieste HTTP
- Errori di connessione
- Risposte dell'API

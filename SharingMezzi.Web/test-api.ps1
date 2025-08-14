# Test API SharingMezzi con endpoint corretti dalla documentazione
Write-Host "🔍 Testing SharingMezzi API - Endpoint Corretti" -ForegroundColor Cyan

$baseUrl = "http://localhost:5000"

# Test endpoint pubblici (senza autenticazione)
$publicEndpoints = @(
    @{url="/api/mezzi"; desc="Tutti i mezzi"},
    @{url="/api/mezzi/disponibili"; desc="Mezzi disponibili"},
    @{url="/api/parcheggi"; desc="Tutti i parcheggi"}
)

Write-Host "`n📊 Testing Endpoint Pubblici:" -ForegroundColor Yellow

foreach ($endpoint in $publicEndpoints) {
    $fullUrl = "$baseUrl$($endpoint.url)"
    Write-Host "`n🔗 $($endpoint.desc): $fullUrl" -ForegroundColor Blue
    
    try {
        $response = Invoke-RestMethod -Uri $fullUrl -Method GET -TimeoutSec 10
        
        if ($response) {
            if ($response -is [Array]) {
                Write-Host "  ✅ OK - Array con $($response.Length) elementi" -ForegroundColor Green
                
                # Mostra dettagli per mezzi
                if ($endpoint.url -like "*mezzi*") {
                    $disponibili = ($response | Where-Object { $_.stato -eq "Disponibile" }).Count
                    Write-Host "     📊 Mezzi disponibili: $disponibili/$($response.Length)" -ForegroundColor Magenta
                    
                    # Mostra tipi mezzi
                    $tipi = $response | Group-Object tipo | ForEach-Object { "$($_.Name): $($_.Count)" }
                    Write-Host "     📋 Tipi: $($tipi -join ', ')" -ForegroundColor Magenta
                }
                
                # Mostra dettagli per parcheggi
                if ($endpoint.url -like "*parcheggi*") {
                    $postiTotali = ($response | Measure-Object capacita -Sum).Sum
                    $postiLiberi = ($response | Measure-Object postiLiberi -Sum).Sum
                    Write-Host "     📊 Posti: $postiLiberi/$postiTotali liberi" -ForegroundColor Magenta
                }
            } else {
                Write-Host "  ✅ OK - Oggetto ricevuto" -ForegroundColor Green
            }
        } else {
            Write-Host "  ⚠️  VUOTO - Risposta vuota" -ForegroundColor Yellow
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "  ❌ ERRORE - $statusCode $($_.Exception.Message)" -ForegroundColor Red
        
        if ($statusCode -eq 401) {
            Write-Host "     💡 Questo endpoint richiede autenticazione" -ForegroundColor Cyan
        }
    }
}

# Test endpoint che richiedono autenticazione
Write-Host "`n🔐 Testing Endpoint con Autenticazione:" -ForegroundColor Yellow

# Prima prova login
Write-Host "`n🔑 Tentativo login con account test..." -ForegroundColor Blue
try {
    $loginData = @{
        email = "mario@test.com"
        password = "user123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginData -ContentType "application/json" -TimeoutSec 10
    
    if ($loginResponse.token) {
        Write-Host "  ✅ LOGIN OK - Token ricevuto" -ForegroundColor Green
        $token = $loginResponse.token
        
        # Test endpoint autenticati
        $authEndpoints = @(
            @{url="/api/corse/storico"; desc="Storico corse"},
            @{url="/api/corse/attive"; desc="Corse attive"},
            @{url="/api/user/profile"; desc="Profilo utente"}
        )
        
        foreach ($endpoint in $authEndpoints) {
            $fullUrl = "$baseUrl$($endpoint.url)"
            Write-Host "`n🔗 $($endpoint.desc): $fullUrl" -ForegroundColor Blue
            
            try {
                $headers = @{Authorization = "Bearer $token"}
                $response = Invoke-RestMethod -Uri $fullUrl -Method GET -Headers $headers -TimeoutSec 10
                
                if ($response) {
                    if ($response -is [Array]) {
                        Write-Host "  ✅ OK - Array con $($response.Length) elementi" -ForegroundColor Green
                    } else {
                        Write-Host "  ✅ OK - Oggetto ricevuto" -ForegroundColor Green
                        
                        # Mostra dettagli profilo
                        if ($endpoint.url -like "*profile*") {
                            Write-Host "     👤 Utente: $($response.nome) $($response.cognome)" -ForegroundColor Magenta
                            Write-Host "     💰 Credito: €$($response.credito)" -ForegroundColor Magenta
                            Write-Host "     🚲 Corse totali: $($response.totaleCorse)" -ForegroundColor Magenta
                        }
                    }
                } else {
                    Write-Host "  ⚠️  VUOTO - Risposta vuota" -ForegroundColor Yellow
                }
            }
            catch {
                Write-Host "  ❌ ERRORE - $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "  ❌ LOGIN FALLITO - Nessun token ricevuto" -ForegroundColor Red
    }
}
catch {
    Write-Host "  ❌ LOGIN FALLITO - $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "     💡 Verifica che l'account mario@test.com / user123 esista" -ForegroundColor Cyan
}

# Test endpoint admin
Write-Host "`n👨‍💼 Testing Endpoint Admin:" -ForegroundColor Yellow
try {
    $adminLoginData = @{
        email = "admin@test.com"
        password = "admin123"
    } | ConvertTo-Json

    $adminResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $adminLoginData -ContentType "application/json" -TimeoutSec 10
    
    if ($adminResponse.token) {
        Write-Host "  ✅ ADMIN LOGIN OK" -ForegroundColor Green
        $adminToken = $adminResponse.token
        
        # Test system status
        try {
            $headers = @{Authorization = "Bearer $adminToken"}
            $systemStatus = Invoke-RestMethod -Uri "$baseUrl/api/admin/system-status" -Method GET -Headers $headers -TimeoutSec 10
            
            Write-Host "  ✅ SYSTEM STATUS:" -ForegroundColor Green
            Write-Host "     📊 Mezzi totali: $($systemStatus.totalMezzi)" -ForegroundColor Magenta
            Write-Host "     🟢 Mezzi disponibili: $($systemStatus.mezziDisponibili)" -ForegroundColor Magenta
            Write-Host "     🔴 Mezzi in uso: $($systemStatus.mezziInUso)" -ForegroundColor Magenta
            Write-Host "     🅿️  Parcheggi: $($systemStatus.totaleParcheggi)" -ForegroundColor Magenta
            Write-Host "     🏃 Corse attive: $($systemStatus.corsaAttive)" -ForegroundColor Magenta
            Write-Host "     📡 IoT devices: $($systemStatus.ioTDevicesConnected)" -ForegroundColor Magenta
        }
        catch {
            Write-Host "  ❌ System status fallito - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}
catch {
    Write-Host "  ❌ ADMIN LOGIN FALLITO - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n📋 RIASSUNTO:" -ForegroundColor Cyan
Write-Host "- Se vedi ✅ OK, gli endpoint funzionano" -ForegroundColor White
Write-Host "- Se vedi ❌ 401, serve autenticazione (normale per homepage pubblica)" -ForegroundColor White
Write-Host "- Se vedi ❌ 404, l'endpoint non esiste" -ForegroundColor White
Write-Host "- Il frontend userà principalmente /api/mezzi/disponibili e /api/parcheggi" -ForegroundColor White

Write-Host "`nPress Enter to continue..." -ForegroundColor Gray
Read-Host
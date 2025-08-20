// Billing.js - Gestione completa della fatturazione
class BillingManager {
    constructor() {
        this.currentBalance = 0;
        this.ecoPoints = 0;
        this.recharges = [];
        this.init();
    }

    async init() {
        console.log('🚀 BillingManager inizializzato');
        await this.loadData();
        this.setupEventListeners();
    }

    async loadData() {
        try {
            console.log('📊 Caricamento dati fatturazione...');
            this.showLoading();

            // Carica profilo utente
            const profileResponse = await fetch('/api/billing/profile');
            if (!profileResponse.ok) {
                throw new Error(`Errore profilo: ${profileResponse.status}`);
            }
            const profile = await profileResponse.json();
            console.log('✅ Profilo caricato:', profile);

            // Aggiorna UI con i dati del profilo
            this.updateProfileUI(profile);

            // Carica ricariche
            const rechargesResponse = await fetch('/api/billing/recharges');
            if (!rechargesResponse.ok) {
                throw new Error(`Errore ricariche: ${rechargesResponse.status}`);
            }
            const recharges = await rechargesResponse.json();
            console.log('✅ Ricariche caricate:', recharges);

            // Aggiorna UI con le ricariche
            this.updateRechargesUI(recharges);

            this.hideLoading();
            console.log('🎉 Dati caricati con successo');

        } catch (error) {
            console.error('❌ Errore nel caricamento dati:', error);
            this.showError(error.message);
        }
    }

    updateProfileUI(profile) {
        // Aggiorna saldo
        this.currentBalance = profile.credito || profile.Credito || 0;
        const balanceElement = document.getElementById('currentBalance');
        if (balanceElement) {
            balanceElement.textContent = this.currentBalance.toFixed(2);
        }

        // Aggiorna punti eco
        this.ecoPoints = profile.puntiEco || profile.PuntiEco || 0;
        const ecoElement = document.getElementById('ecoPoints');
        if (ecoElement) {
            ecoElement.textContent = this.ecoPoints;
        }
    }

    updateRechargesUI(recharges) {
        this.recharges = recharges || [];
        const tbody = document.querySelector('.table tbody');
        
        if (!tbody) {
            console.warn('Tabella ricariche non trovata');
            return;
        }

        tbody.innerHTML = '';

        if (this.recharges.length === 0) {
            // Mostra stato vuoto
            const emptyRow = document.createElement('tr');
            emptyRow.innerHTML = `
                <td colspan="6" class="text-center py-4">
                    <i class="fas fa-receipt fa-3x text-muted mb-3 d-block"></i>
                    <h5>Nessuna ricarica effettuata</h5>
                    <p class="text-muted">Quando effettuerai la prima ricarica, apparirà qui lo storico</p>
                    <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#rechargeModal">
                        <i class="fas fa-plus"></i> Effettua prima ricarica
                    </button>
                </td>
            `;
            tbody.appendChild(emptyRow);
            return;
        }

        // Popola la tabella con le ricariche
        this.recharges.forEach(recharge => {
            const tr = document.createElement('tr');
            const date = new Date(recharge.dataRicarica || recharge.DataRicarica || Date.now()).toLocaleString();
            const importo = Number(recharge.importo ?? recharge.Importo ?? 0).toFixed(2);
            const metodo = recharge.metodoPagamento ?? recharge.MetodoPagamento ?? '—';
            const stato = recharge.stato ?? recharge.Stato ?? '—';
            const saldoFinale = Number(recharge.saldoFinale ?? recharge.SaldoFinale ?? 0).toFixed(2);

            tr.innerHTML = `
                <td>${date}</td>
                <td class="fw-bold text-success">+€${importo}</td>
                <td><i class="fas fa-wallet me-1"></i>${metodo}</td>
                <td><span class="badge bg-success">${stato}</span></td>
                <td class="fw-bold">€${saldoFinale}</td>
                <td><button class="btn btn-outline-primary btn-sm" onclick="billingManager.showRechargeDetails(${recharge.id ?? recharge.Id})"><i class="fas fa-eye"></i></button></td>
            `;
            tbody.appendChild(tr);
        });
    }

    async processRecharge() {
        const form = document.getElementById('rechargeForm');
        if (!form) {
            console.error('Form ricarica non trovato');
            return;
        }

        const formData = new FormData(form);
        const rechargeData = {
            importo: parseFloat(formData.get('amount')),
            metodoPagamento: formData.get('paymentMethod'),
            note: formData.get('notes')
        };

        // Validazione
        if (rechargeData.importo < 5 || rechargeData.importo > 500) {
            this.showAlert('Importo non valido. Deve essere tra €5.00 e €500.00', 'warning');
            return;
        }

        if (!rechargeData.metodoPagamento) {
            this.showAlert('Seleziona un metodo di pagamento', 'warning');
            return;
        }

        try {
            // Disabilita pulsante e mostra caricamento
            const processButton = document.getElementById('processButton');
            const originalText = processButton.innerHTML;
            processButton.disabled = true;
            processButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Elaborazione...';

            console.log('💳 Invio richiesta ricarica:', rechargeData);

            const response = await fetch('/api/billing/recharge', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(rechargeData)
            });

            const data = await response.json();

            if (response.ok) {
                this.showAlert('Ricarica completata con successo!', 'success');
                
                // Aggiorna saldo se disponibile
                if (data.nuovoCredito !== undefined || data.NuovoCredito !== undefined) {
                    const nuovoCredito = data.nuovoCredito ?? data.NuovoCredito;
                    this.currentBalance = Number(nuovoCredito);
                    const balanceElement = document.getElementById('currentBalance');
                    if (balanceElement) {
                        balanceElement.textContent = this.currentBalance.toFixed(2);
                    }
                }
                
                // Chiudi modal e ricarica dati
                const modal = bootstrap.Modal.getInstance(document.getElementById('rechargeModal'));
                if (modal) {
                    modal.hide();
                }
                
                // Ricarica dati dopo un breve delay
                setTimeout(() => this.loadData(), 1000);
                
            } else {
                const errorMessage = data.message || data.Message || 'Errore durante la ricarica';
                this.showAlert(errorMessage, 'danger');
            }
        } catch (error) {
            console.error('❌ Errore durante la ricarica:', error);
            this.showAlert('Errore di connessione. Riprova più tardi.', 'danger');
        } finally {
            // Riabilita pulsante
            const processButton = document.getElementById('processButton');
            processButton.disabled = false;
            processButton.innerHTML = originalText;
        }
    }

    showRechargeDetails(rechargeId) {
        const modal = new bootstrap.Modal(document.getElementById('rechargeDetailsModal'));
        const content = document.getElementById('rechargeDetailsContent');
        
        // Trova la ricarica
        const recharge = this.recharges.find(r => (r.id ?? r.Id) === rechargeId);
        
        if (!recharge) {
            content.innerHTML = '<div class="alert alert-warning">Dettagli non disponibili</div>';
            modal.show();
            return;
        }

        content.innerHTML = `
            <div class="recharge-details">
                <div class="row"><div class="col-6"><strong>ID Transazione:</strong></div><div class="col-6">${recharge.id ?? recharge.Id}</div></div>
                <div class="row"><div class="col-6"><strong>Data:</strong></div><div class="col-6">${new Date(recharge.dataRicarica || recharge.DataRicarica).toLocaleString()}</div></div>
                <div class="row"><div class="col-6"><strong>Importo:</strong></div><div class="col-6">€${Number(recharge.importo ?? recharge.Importo).toFixed(2)}</div></div>
                <div class="row"><div class="col-6"><strong>Metodo:</strong></div><div class="col-6">${recharge.metodoPagamento ?? recharge.MetodoPagamento}</div></div>
                <div class="row"><div class="col-6"><strong>Stato:</strong></div><div class="col-6">${recharge.stato ?? recharge.Stato}</div></div>
                <div class="row"><div class="col-6"><strong>Saldo finale:</strong></div><div class="col-6">€${Number(recharge.saldoFinale ?? recharge.SaldoFinale ?? 0).toFixed(2)}</div></div>
                ${(recharge.note || recharge.Note) ? `<div class="row"><div class="col-12"><strong>Note:</strong><p class="mt-2">${recharge.note || recharge.Note}</p></div></div>` : ''}
            </div>
        `;
        
        modal.show();
    }

    setupEventListeners() {
        // Pulsante aggiorna
        const refreshButton = document.querySelector('button[onclick="refreshData()"]');
        if (refreshButton) {
            refreshButton.onclick = () => this.loadData();
        }

        // Pulsante ricarica
        const processButton = document.getElementById('processButton');
        if (processButton) {
            processButton.onclick = () => this.processRecharge();
        }
    }

    // Utility functions
    setAmount(amount) {
        const amountInput = document.getElementById('rechargeAmount');
        if (amountInput) {
            amountInput.value = amount;
        }
    }

    showAlert(message, type = 'info') {
        // Semplice alert - puoi sostituire con un sistema di notifiche migliore
        alert(message);
    }

    showLoading() {
        const loadingElement = document.getElementById('loadingIndicator');
        const errorElement = document.getElementById('errorMessage');
        
        if (loadingElement) loadingElement.style.display = 'block';
        if (errorElement) errorElement.style.display = 'none';
    }

    hideLoading() {
        const loadingElement = document.getElementById('loadingIndicator');
        if (loadingElement) loadingElement.style.display = 'none';
    }

    showError(message) {
        const errorElement = document.getElementById('errorMessage');
        const errorText = document.getElementById('errorText');
        
        if (errorElement && errorText) {
            errorText.textContent = message;
            errorElement.style.display = 'block';
        }
        
        this.hideLoading();
    }
}

// Inizializza quando il DOM è pronto
document.addEventListener('DOMContentLoaded', function() {
    console.log('🏁 DOM pronto, inizializzazione BillingManager...');
    window.billingManager = new BillingManager();
});

// Funzioni globali per compatibilità con HTML
function setAmount(amount) {
    if (window.billingManager) {
        window.billingManager.setAmount(amount);
    }
}

function processRecharge() {
    if (window.billingManager) {
        window.billingManager.processRecharge();
    }
}

function showRechargeDetails(rechargeId) {
    if (window.billingManager) {
        window.billingManager.showRechargeDetails(rechargeId);
    }
}

function refreshData() {
    if (window.billingManager) {
        window.billingManager.loadData();
    }
}

function showEcoInfo() {
    alert('I punti eco vengono assegnati quando usi mezzi elettrici. Più usi mezzi ecologici, più punti guadagni!');
}

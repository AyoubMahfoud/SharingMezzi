
// SharingMezzi Premium App - cleaned JS: essential behavior only

class PremiumApp {
    constructor() {
        this.currentUser = null;
        this.notificationCount = 0;
        this.isAuthenticated = false;
        this.init();
    }

    async init() {
        try {
            console.log('🚀 Inizializzazione Premium App...');
            
            // Setup event listeners
            this.setupEventListeners();
            
            // Controlla autenticazione
            await this.checkAuthentication();
            this.syncPersistentAuth();
            
            // Prevenzione navigazione indietro nelle pagine protette
            this.preventBackNavigation();
            
            // Gestione cambio pagina
            this.handlePageChange();
            
            // Setup UI
            this.setupUI();
            
            console.log('✅ Premium App inizializzata con successo');
        } catch (error) {
            console.error('❌ Errore durante l\'inizializzazione:', error);
        }
    }

    // Resolve API base URL. Prefer a global auth manager if present, otherwise
    // assume API runs on same host replacing common web port 5050 with API port 5000.
    getApiBase() {
        try {
            if (window.authManager && window.authManager.apiBaseUrl) return window.authManager.apiBaseUrl.replace(/\/$/, '');
            const loc = window.location;
            // If running on the web UI port (5050) assume API is on 5000
            if (loc.port === '5050') {
                return `${loc.protocol}//${loc.hostname}:5000`;
            }
            return `${loc.protocol}//${loc.host}`;
        } catch (e) {
            return '';
        }
    }

    // Setup UI di base
    setupUI() {
        try {
            // Setup sidebar toggle
            const sidebarToggle = document.getElementById('sidebarToggle');
            const sidebar = document.getElementById('sidebar');
            
            if (sidebarToggle && sidebar) {
                sidebarToggle.addEventListener('click', () => {
                    sidebar.classList.toggle('d-none');
                });
            }
            
            // Setup sidebar close per mobile
            const sidebarClose = document.getElementById('sidebarClose');
            if (sidebarClose && sidebar) {
                sidebarClose.addEventListener('click', () => {
                    sidebar.classList.add('d-none');
                });
            }
            
            console.log('✅ UI setup completato');
        } catch (error) {
            console.warn('⚠️ Errore durante il setup UI:', error);
        }
    }

    // Setup event listeners di base
    setupEventListeners() {
        try {
            // Gestione logout dalla navbar
            const logoutButtons = document.querySelectorAll('[href="/Logout"], .logout-btn');
            logoutButtons.forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.logout();
                });
            });
            
            console.log('✅ Event listeners configurati');
        } catch (error) {
            console.warn('⚠️ Errore durante il setup event listeners:', error);
        }
    }

    // Sincronizza l'autenticazione con i cookie persistenti
    syncPersistentAuth() {
        try {
            // Controlla se ci sono cookie persistenti ma non siamo autenticati
            const hasPersistentToken = document.cookie.includes('PersistentToken');
            const hasPersistentUser = document.cookie.includes('PersistentUser');
            
            if (hasPersistentToken && hasPersistentUser && !this.isAuthenticated) {
                console.log('🔄 Rilevati cookie persistenti, sincronizzando autenticazione...');
                
                // Forza un refresh della pagina per attivare il middleware di auto-login
                // Questo è più sicuro che manipolare direttamente i cookie
                if (window.location.pathname !== '/Login' && window.location.pathname !== '/Register') {
                    console.log('🔄 Refresh per attivare auto-login...');
                    window.location.reload();
                }
            }
        } catch (error) {
            console.warn('⚠️ Errore durante la sincronizzazione auth persistente:', error);
        }
    }

    // Enhanced error handling
    setupGlobalErrorHandling() {
        window.addEventListener('error', (event) => {
            console.error('Global error:', event.error);
            // Non gestire errori di elementi DOM mancanti
            if (!event.error.message.includes('Cannot read properties of null')) {
                this.handleError(event.error);
            }
        });

        window.addEventListener('unhandledrejection', (event) => {
            console.error('Unhandled promise rejection:', event.reason);
            this.handleError(event.reason);
        });
    }

    handleError(error) {
        // Don't show error notifications for minor issues
        if (error.name === 'NetworkError' || 
            error.name === 'AbortError' || 
            error.message.includes('Cannot read properties of null')) {
            return;
        }

        this.showNotification(
            'Si è verificato un errore. Riprova tra qualche istante.',
            'error'
        );
    }

    // FIXED: Enhanced event listeners with safety checks
    setupEventListeners() {
        // Mobile menu toggle - with safety check
        const mobileMenuBtn = document.querySelector('.mobile-menu-btn');
        const sidebar = document.querySelector('.premium-sidebar');
        
        if (mobileMenuBtn && sidebar) {
            mobileMenuBtn.addEventListener('click', () => {
                sidebar.classList.toggle('show');
                document.body.classList.toggle('sidebar-open');
            });
        }

        // Close mobile menu when clicking outside - with safety check
        document.addEventListener('click', (e) => {
            if (sidebar && sidebar.classList.contains('show')) {
                if (mobileMenuBtn && !sidebar.contains(e.target) && !mobileMenuBtn.contains(e.target)) {
                    sidebar.classList.remove('show');
                    document.body.classList.remove('sidebar-open');
                }
            }
        });

        // Enhanced search functionality
        this.setupEnhancedSearch();
        
        // Keyboard shortcuts
        this.setupKeyboardShortcuts();
        
        // Scroll enhancements
        this.setupScrollEnhancements();

        // Ensure admin link clicks sync local token to server session before navigation
        // This uses a capture-phase listener to prevent inline onclick from navigating
        document.addEventListener('click', (e) => {
            try {
                // Determine the element clicked
                const target = e.target;
                const adminAnchor = target.closest && target.closest('a[href]');
                let href = null;

                if (adminAnchor) {
                    const h = adminAnchor.getAttribute('href');
                    if (h && h.startsWith('/Admin')) href = h;
                }

                // Also support buttons or elements that use data-admin-target
                if (!href) {
                    const dataTargetEl = target.closest && target.closest('[data-admin-target]');
                    if (dataTargetEl) {
                        const dt = dataTargetEl.getAttribute('data-admin-target');
                        if (dt && dt.startsWith('/Admin')) href = dt;
                    }
                }

                // Buttons that navigate via inline onclick like: onclick="window.location.href='/Admin/Users'"
                if (!href) {
                    const btn = target.closest && target.closest('button[onclick], [data-admin-target]');
                    if (btn) {
                        // data-admin-target is preferred if present
                        if (btn.dataset && btn.dataset.adminTarget) {
                            href = btn.dataset.adminTarget;
                        } else {
                            const onclick = btn.getAttribute('onclick') || '';
                            const m = onclick.match(/window\.location\.href\s*=\s*['"]([^'\"]+)['"]/);
                            if (m && m[1] && m[1].startsWith('/Admin')) href = m[1];
                        }
                    }
                }

                if (!href) return; // not an admin navigation

                console.debug('Admin click intercepted, target href=', href);

                // Intercept and sync session. Prevent default navigation but do not stop other handlers
                // (stopImmediatePropagation can suppress inline onclick handlers and prevent navigation)
                e.preventDefault();

                // show transition
                this.showPageTransition();

                const token = localStorage.getItem('token') || localStorage.getItem('auth_token') || localStorage.getItem('authToken');

                const setSession = async () => {
                    if (!token) return;
                    try {
                        // include credentials to allow server to set/receive session cookie
                        await fetch(location.origin + '/Auth/SetSession', {
                            method: 'POST',
                            credentials: 'include',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ token })
                        });
                    } catch (err) {
                        console.debug('SetSession failed on admin click', err);
                    }
                };

                // Wait for either SetSession to complete or a short timeout, then navigate
                (async () => {
                    const p = setSession();
                    const timeout = new Promise(resolve => setTimeout(resolve, 700));
                    await Promise.race([p, timeout]);
                    // final defensive check: ensure href is still valid and not an external
                    console.debug('Navigating to admin href=', href);
                    if (href && href.startsWith('/')) {
                        window.location.href = href;
                    } else if (href) {
                        window.location.assign(href);
                    }
                })();
            } catch (err) {
                console.error('Admin click handler error', err);
            }
        }, true);

        // Fallback delegated listener for elements using data-admin-target
        // Some DOM structures or event ordering can prevent the capture handler from acting; this catches those.
        document.addEventListener('click', (e) => {
            try {
                const el = e.target && e.target.closest && e.target.closest('[data-admin-target]');
                if (!el) return;
                const href = el.getAttribute('data-admin-target');
                if (!href || !href.startsWith('/Admin')) return;

                console.debug('Fallback data-admin-target click, href=', href);

                e.preventDefault();

                const token = localStorage.getItem('token') || localStorage.getItem('auth_token') || localStorage.getItem('authToken');
                fetch(location.origin + '/Auth/SetSession', {
                    method: 'POST',
                    credentials: 'include',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ token })
                }).catch(() => null).finally(() => {
                    setTimeout(() => window.location.href = href, 200);
                });
            } catch (err) {
                console.error('Fallback admin click error', err);
            }
        }, false);
    }

    setupEnhancedSearch() {
        const searchBox = document.querySelector('.search-box input');
        if (!searchBox) {
            console.log('Search box not found - skipping search setup');
            return;
        }

        let searchTimeout;
        
        searchBox.addEventListener('input', (e) => {
            clearTimeout(searchTimeout);
            const query = e.target.value.trim();
            
            if (query.length < 2) {
                this.hideSearchResults();
                return;
            }
            
            searchTimeout = setTimeout(() => {
                this.performSearch(query);
            }, 300);
        });

        // Search shortcuts
        searchBox.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                searchBox.blur();
                this.hideSearchResults();
            }
        });
    }

    async performSearch(query) {
        try {
            this.showSearchLoading();
            
            // Simulate API call - replace with actual search endpoint
            const results = await this.mockSearchAPI(query);
            this.displaySearchResults(results);
            
        } catch (error) {
            console.error('Search error:', error);
            this.hideSearchResults();
        }
    }

    // Mock search API - replace with real implementation
    async mockSearchAPI(query) {
        return new Promise((resolve) => {
            setTimeout(() => {
                resolve([
                    { type: 'vehicle', title: `Bicicletta ${query}`, url: '/vehicles' },
                    { type: 'parking', title: `Parcheggio ${query}`, url: '/parking' },
                    { type: 'page', title: `Pagina ${query}`, url: '/page' }
                ]);
            }, 500);
        });
    }

    setupKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Only when not typing in form fields
            if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
                return;
            }

            // Ctrl/Cmd + K for search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchBox = document.querySelector('.search-box input');
                if (searchBox) {
                    searchBox.focus();
                }
            }

            // Escape key handling
            if (e.key === 'Escape') {
                this.closeAllModals();
                this.hideSearchResults();
            }
        });
    }

    setupScrollEnhancements() {
        let lastScrollTop = 0;
        const header = document.querySelector('.main-header');
        
        if (!header) {
            console.log('Header not found - skipping scroll enhancements');
            return;
        }

        window.addEventListener('scroll', this.throttle(() => {
            const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
            
            if (scrollTop > lastScrollTop && scrollTop > 100) {
                // Scrolling down
                header.classList.add('header-hidden');
            } else {
                // Scrolling up
                header.classList.remove('header-hidden');
            }
            
            lastScrollTop = scrollTop;
        }, 100));
    }

    // Enhanced navigation
    setupNavigation() {
        // Active page highlighting
        this.highlightActivePage();
        
        // Breadcrumb generation
        this.generateBreadcrumbs();
        
        // Navigation transitions
        this.setupNavigationTransitions();
    }

    highlightActivePage() {
        const currentPath = window.location.pathname;
        const navLinks = document.querySelectorAll('.nav-link');
        
        navLinks.forEach(link => {
            const linkPath = new URL(link.href).pathname;
            
            if (linkPath === currentPath || 
                (currentPath.startsWith(linkPath) && linkPath !== '/')) {
                link.classList.add('active');
                
                // Expand parent menu if nested
                const parentItem = link.closest('.nav-item');
                if (parentItem) {
                    const parentCollapse = parentItem.querySelector('.collapse');
                    if (parentCollapse) {
                        parentCollapse.classList.add('show');
                    }
                }
            } else {
                link.classList.remove('active');
            }
        });
    }

    generateBreadcrumbs() {
        console.log('Generating breadcrumbs...');
    }

    setupNavigationTransitions() {
        const navLinks = document.querySelectorAll('.nav-link');
        
        navLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                // Skip if it's an external link or has special handling
                if (link.hasAttribute('target') || 
                    link.getAttribute('href').startsWith('#')) {
                    return;
                }
                
                // Add loading state
                this.showPageTransition();
            });
        });
    }

    showPageTransition() {
        const loader = document.createElement('div');
        loader.className = 'page-transition-loader';
        loader.innerHTML = `
            <div class="transition-content">
                <div class="transition-spinner"></div>
                <p>Caricamento...</p>
            </div>
        `;
        document.body.appendChild(loader);
        
        // Auto-remove if page doesn't change within 3 seconds
        setTimeout(() => {
            if (document.body.contains(loader)) {
                loader.remove();
            }
        }, 3000);
    }

    // Enhanced notifications system
    setupNotifications() {
        this.notificationContainer = this.createNotificationContainer();
        this.updateNotificationBadge();
        
        // Check for saved notifications
        this.loadSavedNotifications();
    }

    createNotificationContainer() {
        let container = document.querySelector('.notification-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'notification-container';
            container.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 10000;
                pointer-events: none;
            `;
            document.body.appendChild(container);
        }
        return container;
    }

    showNotification(message, type = 'info', duration = 5000, actions = null) {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type} animate-slide-in`;
        
        const iconMap = {
            success: 'fas fa-check-circle',
            error: 'fas fa-exclamation-circle',
            warning: 'fas fa-exclamation-triangle',
            info: 'fas fa-info-circle'
        };

        const bgColorMap = {
            success: '#28a745',
            error: '#dc3545',
            warning: '#ffc107',
            info: '#17a2b8'
        };

        notification.style.cssText = `
            background: ${bgColorMap[type]};
            color: white;
            padding: 15px 20px;
            border-radius: 8px;
            margin-bottom: 10px;
            font-size: 14px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            opacity: 0;
            transform: translateX(100%);
            transition: all 0.3s ease;
            pointer-events: auto;
            max-width: 350px;
            position: relative;
            display: flex;
            align-items: center;
            gap: 10px;
        `;

        notification.innerHTML = `
            <i class="${iconMap[type]}"></i>
            <span>${message}</span>
            <button class="notification-close" style="
                background: transparent;
                border: none;
                color: white;
                font-size: 16px;
                cursor: pointer;
                margin-left: auto;
                padding: 0;
                width: 20px;
                height: 20px;
                display: flex;
                align-items: center;
                justify-content: center;
            ">
                <i class="fas fa-times"></i>
            </button>
        `;

        // Close button functionality
        const closeBtn = notification.querySelector('.notification-close');
        closeBtn.addEventListener('click', () => {
            this.dismissNotification(notification);
        });

        // Auto-dismiss
        if (duration > 0) {
            setTimeout(() => {
                this.dismissNotification(notification);
            }, duration);
        }

        this.notificationContainer.appendChild(notification);
        
        // Trigger animation
        requestAnimationFrame(() => {
            notification.style.opacity = '1';
            notification.style.transform = 'translateX(0)';
        });

        return notification;
    }

    dismissNotification(notification) {
        notification.style.opacity = '0';
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }

    updateNotificationBadge() {
        const badge = document.querySelector('.notification-badge');
        if (badge) {
            badge.textContent = this.notificationCount;
            badge.style.display = this.notificationCount > 0 ? 'inline' : 'none';
        }
    }

    // Enhanced loading states
    setupLoadingStates() {
        // Global loading overlay
        this.createLoadingOverlay();
        
        // Form submission loading states
        this.setupFormLoadingStates();
    }

    createLoadingOverlay() {
        const overlay = document.createElement('div');
        overlay.id = 'global-loading-overlay';
        overlay.className = 'loading-overlay hidden';
        overlay.innerHTML = `
            <div class="loading-content">
                <div class="loading-spinner"></div>
                <p class="loading-text">Caricamento...</p>
            </div>
        `;
        document.body.appendChild(overlay);
        return overlay;
    }

    showLoading(text = 'Caricamento...') {
        const overlay = document.getElementById('global-loading-overlay');
        const loadingText = overlay.querySelector('.loading-text');
        
        if (loadingText) {
            loadingText.textContent = text;
        }
        
        overlay.classList.remove('hidden');
        document.body.style.overflow = 'hidden';
    }

    hideLoading() {
        const overlay = document.getElementById('global-loading-overlay');
        overlay.classList.add('hidden');
        document.body.style.overflow = '';
    }

    setupFormLoadingStates() {
        document.addEventListener('submit', (e) => {
            const form = e.target;
            if (form.tagName === 'FORM') {
                const submitBtn = form.querySelector('button[type="submit"]');
                if (submitBtn) {
                    this.setButtonLoading(submitBtn, true);
                    
                    // Reset after 10 seconds as fallback
                    setTimeout(() => {
                        this.setButtonLoading(submitBtn, false);
                    }, 10000);
                }
            }
        });
    }

    setButtonLoading(button, loading) {
        if (loading) {
            button.disabled = true;
            button.dataset.originalText = button.innerHTML;
            button.innerHTML = `
                <span class="loading-spinner-sm"></span>
                Caricamento...
            `;
        } else {
            button.disabled = false;
            button.innerHTML = button.dataset.originalText || button.innerHTML;
        }
    }

    // FIXED: Enhanced authentication con supporto per cookie persistenti
    async checkAuthentication() {
        try {
            // Cerca il token sia in localStorage che in sessionStorage usando chiavi comuni
            const token = await this.getStoredToken();

            // Se non c'è token in memoria, chiedi al server se esiste una sessione valida
            if (!token) {
                console.log('🔄 No stored token, asking server for current session...');
                try {
                        const resp = await fetch(location.origin + '/Auth/Current', { credentials: 'same-origin' });
                    if (resp.ok) {
                        const data = await resp.json();
                        if (data && data.isAuthenticated) {
                            console.log('� Server session valid, setting current user from server');
                            if (data.user) {
                                this.setCurrentUser(data.user);
                            }
                            return;
                        }
                    }
                } catch (err) {
                    console.debug('Could not verify server session', err);
                }

                console.log('❌ No token found anywhere');
                this.handleUnauthenticated();
                return;
            }

            // Decodifica il JWT localmente
            const payload = this.decodeJWT(token);
            if (!payload) {
                console.log('❌ Invalid token format');
                this.handleUnauthenticated();
                return;
            }

            // Controlla se il token è scaduto
            if (payload.exp * 1000 < Date.now()) {
                console.log('❌ Token expired, attempting refresh...');
                // Prova a rinnovare l'autenticazione
                const refreshed = await this.refreshAuthentication();
                if (refreshed) {
                    console.log('✅ Token refreshed successfully');
                    return;
                } else {
                    console.log('❌ Token refresh failed');
                    this.handleUnauthenticated();
                    return;
                }
            }

            console.log('✅ Token valid, user payload:', payload);

            // Estrai i dati utente dal token
            const userData = {
                id: parseInt(payload.sub || payload.nameid), // nameid è il ClaimType.NameIdentifier standard
                nome: payload.given_name || payload.name || 'Utente',
                cognome: payload.family_name || '',
                email: payload.email || '',
                ruolo: this.parseUserRole(payload.role) // Converte string a numero
            };

            console.log('👤 User data extracted:', userData);
            this.setCurrentUser(userData);

        } catch (error) {
            console.error('❌ Authentication check failed:', error);
            this.handleUnauthenticated();
        }
    }

    // Aggiungi questa funzione helper per decodificare il JWT
    decodeJWT(token) {
        try {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(
                atob(base64)
                    .split('')
                    .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                    .join('')
            );
            return JSON.parse(jsonPayload);
        } catch (error) {
            console.error('JWT decode failed:', error);
            return null;
        }
    }

    // Recupera il token da localStorage o sessionStorage (chiavi comuni usate in altre parti)
    async getStoredToken() {
        // Controlla le chiavi più comuni usate nel progetto
        const keys = ['token', 'auth_token', 'authToken'];
        for (const k of keys) {
            let v = localStorage.getItem(k);
            if (v) return v;
            v = sessionStorage.getItem(k);
            if (v) return v;
        }
        
        // Se non c'è token locale, prova a ottenerlo dal server
        console.log('🔄 Nessun token locale, tentativo di recupero dal server...');
        try {
            const response = await fetch('/api/auth/current-token', {
                method: 'GET',
                credentials: 'same-origin'
            });
            
            if (response.ok) {
                const data = await response.json();
                if (data.Success && data.Token) {
                    console.log('✅ Token ottenuto dal server');
                    // Salva il token per usi futuri
                    localStorage.setItem('auth_token', data.Token);
                    return data.Token;
                }
            } else {
                console.log('❌ Impossibile ottenere token dal server:', response.status);
            }
        } catch (error) {
            console.warn('Errore nel recupero del token dal server:', error);
        }
        
        console.log('❌ Nessun token disponibile');
        return null;
    }

    // Metodo pubblico per ottenere il token (compatibilità con altre parti dell'app)
    async getToken() {
        const token = await this.getStoredToken();
        if (!token) return null;
        
        // Controlla se il token è scaduto
        const payload = this.decodeJWT(token);
        if (payload && payload.exp * 1000 < Date.now()) {
            console.log('🔄 Token scaduto, tentativo di refresh...');
            // Rimuovi il token scaduto
            this.clearExpiredToken();
            // Prova a rinnovare l'autenticazione (non bloccante)
            this.refreshAuthentication().then(refreshed => {
                if (refreshed) {
                    console.log('✅ Token refreshed successfully');
                } else {
                    console.log('❌ Token refresh failed');
                }
            });
            return null;
        }
        
        return token;
    }

    // Metodo asincrono per ottenere il token con refresh automatico
    async getTokenAsync() {
        const token = await this.getStoredToken();
        if (!token) return null;
        
        // Controlla se il token è scaduto
        const payload = this.decodeJWT(token);
        if (payload && payload.exp * 1000 < Date.now()) {
            console.log('🔄 Token scaduto, tentativo di refresh sincrono...');
            // Prova a rinnovare l'autenticazione usando il refresh token
            const refreshed = await this.refreshToken();
            if (refreshed) {
                console.log('✅ Token refreshed successfully');
                return await this.getStoredToken();
            } else {
                console.log('❌ Token refresh failed');
                this.clearExpiredToken();
                return null;
            }
        }
        
        return token;
    }

    // Prova a rinnovare il token usando il refresh token
    async refreshToken() {
        try {
            // Prova prima a ottenere il refresh token dalla sessione del server
            console.log('🔄 Tentativo di refresh del token...');
            
            const response = await fetch('/api/auth/refresh-session', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'same-origin' // Importante per includere i cookie di sessione
            });

            if (response.ok) {
                const data = await response.json();
                if (data.Success && data.Token) {
                    console.log('✅ Nuovo token ottenuto dal refresh');
                    // Salva il nuovo token
                    localStorage.setItem('auth_token', data.Token);
                    return true;
                }
            } else {
                console.log('❌ Refresh token fallito:', response.status);
            }
        } catch (error) {
            console.warn('Refresh token error:', error);
        }
        return false;
    }

    // Recupera il refresh token da localStorage
    getStoredRefreshToken() {
        return localStorage.getItem('refresh_token');
    }

    // Rimuovi il token scaduto
    clearExpiredToken() {
        const keys = ['token', 'auth_token', 'authToken'];
        keys.forEach(k => {
            localStorage.removeItem(k);
            sessionStorage.removeItem(k);
        });
        // Rimuovi anche il refresh token se il refresh fallisce
        localStorage.removeItem('refresh_token');
        
        // Prova a rimuovere anche dalla sessione del server
        fetch('/Auth/Logout', { 
            method: 'POST', 
            credentials: 'same-origin' 
        }).catch(e => console.warn('Errore durante logout:', e));
    }

    // Prova a rinnovare l'autenticazione
    async refreshAuthentication() {
        try {
            console.log('🔄 Tentativo di refresh dell\'autenticazione...');
            // Prova a ottenere una nuova sessione dal server
            const resp = await fetch(location.origin + '/Auth/Current', { credentials: 'same-origin' });
            if (resp.ok) {
                const data = await resp.json();
                if (data && data.isAuthenticated && data.token) {
                    console.log('✅ Nuovo token ottenuto dal server');
                    // Salva il nuovo token
                    localStorage.setItem('auth_token', data.token);
                    this.setCurrentUser(data.user);
                    return true;
                }
            }
            
            // Se il metodo precedente fallisce, prova con il nuovo endpoint
            console.log('🔄 Tentativo con nuovo endpoint di refresh...');
            const refreshResp = await fetch('/api/auth/current-token', { 
                credentials: 'same-origin' 
            });
            if (refreshResp.ok) {
                const refreshData = await refreshResp.json();
                if (refreshData.Success && refreshData.Token) {
                    console.log('✅ Nuovo token ottenuto dal nuovo endpoint');
                    localStorage.setItem('auth_token', refreshData.Token);
                    if (refreshData.User) {
                        this.setCurrentUser(refreshData.User);
                    }
                    return true;
                }
            }
        } catch (error) {
            console.warn('Refresh authentication failed:', error);
        }
        return false;
    }

    // Proprietà per l'URL base dell'API (compatibilità con altre parti dell'app)
    get apiBaseUrl() {
        return this.getApiBase();
    }

    // Aggiungi questa funzione per parsare il ruolo
    parseUserRole(role) {
        console.log('🔍 Parsing user role:', role, typeof role);
        
        // Il ruolo potrebbe essere stringa ("Amministratore", "Utente") o numero
        if (typeof role === 'number') {
            return role;
        }
        
        if (typeof role === 'string') {
            // Converti stringa a numero
            if (role === 'Amministratore' || role === '1') {
                return 1;
            }
        }
        
        return 0; // Default a utente normale
    }

    setCurrentUser(user) {
        this.currentUser = user;
        this.isAuthenticated = true;
        this.updateUserInterface();
    }

    // Salva il refresh token quando ricevuto dal login
    saveRefreshToken(refreshToken) {
        if (refreshToken) {
            localStorage.setItem('refresh_token', refreshToken);
            console.log('✅ Refresh token salvato');
        }
    }

    handleUnauthenticated() {
        this.currentUser = null;
        this.isAuthenticated = false;
        localStorage.removeItem('auth_token');

        // Only act for protected pages
        if (!this.isProtectedPage()) return;

        // Prevent multiple concurrent checks
        if (this._authCheckInProgress) return;
        this._authCheckInProgress = true;

        // We cannot rely on document.cookie for HttpOnly cookies. Ask the server
        // for the current session before redirecting so we avoid race conditions
        // with server-side auto-login middleware.
        (async () => {
            const maxAttempts = 5;
            const delayMs = 300;
            let ok = false;

            for (let i = 0; i < maxAttempts; i++) {
                try {
                    const resp = await fetch(location.origin + '/Auth/Current', { credentials: 'same-origin' });
                    if (resp.ok) {
                        const data = await resp.json();
                        if (data && data.isAuthenticated) {
                            console.debug('Server session valid, not redirecting to Login (attempt', i + 1, ')');
                            if (data.user) this.setCurrentUser(data.user);
                            ok = true;
                            break;
                        }
                    }
                } catch (e) {
                    console.debug('Attempt', i + 1, 'failed to verify server session', e);
                }
                await new Promise(r => setTimeout(r, delayMs));
            }

            this._authCheckInProgress = false;

            if (!ok) {
                console.log('🚫 Pagina protetta senza autenticazione dopo tentativi, reindirizzamento...');
                if (window.safeRedirect) {
                    window.safeRedirect('/Login');
                } else {
                    window.location.href = '/Login';
                }
            }
        })();
    }

    updateUserInterface() {
        if (!this.currentUser) return;

        // Update user info in navigation
        this.updateUserDisplay();
        
        // Show/hide admin features
        this.toggleAdminFeatures();
        
        // Update notifications
        this.loadUserNotifications();
    }

    updateUserDisplay() {
        const userElements = document.querySelectorAll('.user-name');
        const roleElements = document.querySelectorAll('.user-role');
        const avatarElements = document.querySelectorAll('.user-avatar');

        userElements.forEach(el => {
            el.textContent = this.currentUser.nome || 'Utente';
        });

        roleElements.forEach(el => {
            el.textContent = this.currentUser.ruolo === 1 ? 'Amministratore' : 'Utente';
        });

        avatarElements.forEach(el => {
            const initial = (this.currentUser.nome || 'U').charAt(0).toUpperCase();
            el.textContent = initial;
        });
    }

    // FIXED: Migliore debug per toggleAdminFeatures
    toggleAdminFeatures() {
        const adminElements = document.querySelectorAll('.admin-only');
        const isAdmin = this.currentUser && this.currentUser.ruolo === 1;

        console.log('🔧 toggleAdminFeatures:', {
            currentUser: this.currentUser,
            userRole: this.currentUser?.ruolo,
            isAdmin: isAdmin,
            adminElementsFound: adminElements.length
        });

        adminElements.forEach((el, index) => {
            console.log(`   Admin element ${index}:`, el.className, isAdmin ? 'SHOWING' : 'HIDING');
            el.style.display = isAdmin ? 'block' : 'none';
        });

        // Anche per elementi di navigazione admin
        const adminNavItems = document.querySelectorAll('.nav-item.admin-only, .sidebar-item.admin-only');
        adminNavItems.forEach((el, index) => {
            console.log(`   Admin nav ${index}:`, el.className, isAdmin ? 'SHOWING' : 'HIDING');
            el.style.display = isAdmin ? 'flex' : 'none';
        });
    }

    // Enhanced form validation
    setupFormValidation() {
        const forms = document.querySelectorAll('form[data-validate]');
        
        if (forms.length === 0) {
            console.log('No forms to validate found');
            return;
        }
        
        forms.forEach(form => {
            this.initFormValidation(form);
        });
    }

    initFormValidation(form) {
        if (!form) return;

        const inputs = form.querySelectorAll('input, select, textarea');
        
        inputs.forEach(input => {
            input.addEventListener('blur', () => {
                this.validateField(input);
            });
            
            input.addEventListener('input', () => {
                this.clearFieldValidation(input);
            });
        });

        form.addEventListener('submit', (e) => {
            if (!this.validateForm(form)) {
                e.preventDefault();
                this.showNotification('Controlla i dati inseriti', 'error');
            }
        });
    }

    validateField(field) {
        const value = field.value.trim();
        const rules = this.getValidationRules(field);
        let isValid = true;
        let errorMessage = '';

        for (const rule of rules) {
            if (!rule.test(value)) {
                isValid = false;
                errorMessage = rule.message;
                break;
            }
        }

        this.setFieldValidationState(field, isValid, errorMessage);
        return isValid;
    }

    getValidationRules(field) {
        const rules = [];
        const type = field.type;
        const required = field.hasAttribute('required');

        if (required) {
            rules.push({
                test: (value) => value.length > 0,
                message: 'Questo campo è obbligatorio'
            });
        }

        if (type === 'email') {
            rules.push({
                test: (value) => !value || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value),
                message: 'Inserisci un indirizzo email valido'
            });
        }

        if (field.hasAttribute('minlength')) {
            const minLength = parseInt(field.getAttribute('minlength'));
            rules.push({
                test: (value) => !value || value.length >= minLength,
                message: `Deve contenere almeno ${minLength} caratteri`
            });
        }

        if (field.hasAttribute('pattern')) {
            const pattern = new RegExp(field.getAttribute('pattern'));
            rules.push({
                test: (value) => !value || pattern.test(value),
                message: field.getAttribute('data-pattern-message') || 'Formato non valido'
            });
        }

        return rules;
    }

    setFieldValidationState(field, isValid, errorMessage) {
        const fieldGroup = field.closest('.form-group') || field.parentElement;
        
        // Remove existing validation classes
        fieldGroup.classList.remove('has-error', 'has-success');
        
        // Remove existing error message
        const existingError = fieldGroup.querySelector('.field-error');
        if (existingError) {
            existingError.remove();
        }

        if (isValid) {
            fieldGroup.classList.add('has-success');
        } else {
            fieldGroup.classList.add('has-error');
            
            // Add error message
            const errorElement = document.createElement('div');
            errorElement.className = 'field-error';
            errorElement.textContent = errorMessage;
            fieldGroup.appendChild(errorElement);
        }
    }

    clearFieldValidation(field) {
        const fieldGroup = field.closest('.form-group') || field.parentElement;
        fieldGroup.classList.remove('has-error', 'has-success');
        
        const errorElement = fieldGroup.querySelector('.field-error');
        if (errorElement) {
            errorElement.remove();
        }
    }

    validateForm(form) {
        const inputs = form.querySelectorAll('input, select, textarea');
        let isValid = true;

        inputs.forEach(input => {
            if (!this.validateField(input)) {
                isValid = false;
            }
        });

        return isValid;
    }

    // Enhanced modal handling
    setupModalHandlers() {
        // Modal triggers
        document.addEventListener('click', (e) => {
            const modalTrigger = e.target.closest('[data-modal]');
            if (modalTrigger) {
                e.preventDefault();
                const modalId = modalTrigger.getAttribute('data-modal');
                this.openModal(modalId);
            }

            // Modal close buttons
            const modalClose = e.target.closest('.modal-close, [data-modal-close]');
            if (modalClose) {
                e.preventDefault();
                this.closeModal(modalClose.closest('.modal'));
            }
        });

        // Close modal on backdrop click
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('modal-backdrop')) {
                this.closeModal(e.target.querySelector('.modal'));
            }
        });

        // Close modal on Escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.closeAllModals();
            }
        });
    }

    openModal(modalId) {
        const modal = document.getElementById(modalId);
        if (!modal) return;

        modal.classList.add('active');
        document.body.classList.add('modal-open');
        
        // Focus management
        const firstFocusable = modal.querySelector('input, button, select, textarea, [tabindex]:not([tabindex="-1"])');
        if (firstFocusable) {
            firstFocusable.focus();
        }

        // Announce to screen readers
        this.announceToScreenReader('Finestra di dialogo aperta');
    }

    closeModal(modal) {
        if (!modal) return;

        modal.classList.remove('active');
        document.body.classList.remove('modal-open');
        
        // Announce to screen readers
        this.announceToScreenReader('Finestra di dialogo chiusa');
    }

    closeAllModals() {
        const openModals = document.querySelectorAll('.modal.active');
        openModals.forEach(modal => this.closeModal(modal));
    }

    // Enhanced tooltips
    setupTooltips() {
        const tooltipElements = document.querySelectorAll('[data-tooltip]');
        
        tooltipElements.forEach(element => {
            this.initTooltip(element);
        });
    }

    initTooltip(element) {
        let tooltip = null;
        let showTimeout = null;
        let hideTimeout = null;

        const showTooltip = () => {
            clearTimeout(hideTimeout);
            showTimeout = setTimeout(() => {
                tooltip = this.createTooltip(element);
                document.body.appendChild(tooltip);
                this.positionTooltip(element, tooltip);
                
                requestAnimationFrame(() => {
                    tooltip.classList.add('visible');
                });
            }, 500);
        };

        const hideTooltip = () => {
            clearTimeout(showTimeout);
            if (tooltip) {
                hideTimeout = setTimeout(() => {
                    tooltip.classList.remove('visible');
                    setTimeout(() => {
                        if (tooltip && tooltip.parentNode) {
                            tooltip.parentNode.removeChild(tooltip);
                        }
                        tooltip = null;
                    }, 200);
                }, 100);
            }
        };

        element.addEventListener('mouseenter', showTooltip);
        element.addEventListener('mouseleave', hideTooltip);
        element.addEventListener('focus', showTooltip);
        element.addEventListener('blur', hideTooltip);
    }

    createTooltip(element) {
        const tooltip = document.createElement('div');
        tooltip.className = 'premium-tooltip';
        tooltip.textContent = element.getAttribute('data-tooltip');
        
        tooltip.style.cssText = `
            position: absolute;
            background: #2c3e50;
            color: white;
            padding: 8px 12px;
            border-radius: 6px;
            font-size: 0.875rem;
            white-space: nowrap;
            z-index: 10000;
            opacity: 0;
            transition: opacity 0.2s ease;
            pointer-events: none;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        `;
        
        return tooltip;
    }

    positionTooltip(element, tooltip) {
        const rect = element.getBoundingClientRect();
        const tooltipRect = tooltip.getBoundingClientRect();
        
        // Position above element by default
        let top = rect.top - tooltipRect.height - 8;
        let left = rect.left + (rect.width - tooltipRect.width) / 2;
        
        // Adjust if tooltip would go off screen
        if (top < 8) {
            top = rect.bottom + 8;
        }
        
        if (left < 8) {
            left = 8;
        } else if (left + tooltipRect.width > window.innerWidth - 8) {
            left = window.innerWidth - tooltipRect.width - 8;
        }
        
        tooltip.style.top = `${top + window.scrollY}px`;
        tooltip.style.left = `${left + window.scrollX}px`;
    }

    // Enhanced lazy loading
    setupLazyLoading() {
        // Intersection Observer for images
        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                        img.removeAttribute('data-src');
                        imageObserver.unobserve(img);
                    }
                }
            });
        });

        // Observe all images with data-src
        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }

    // Page-specific initializations
    initPageSpecific() {
        const currentPath = window.location.pathname;
        
        console.log(`🎯 Initializing page-specific features for: ${currentPath}`);
        
        // Dashboard specific
        if (currentPath === '/Dashboard' || currentPath === '/dashboard') {
            this.initDashboard();
        }
        
        // Admin pages
        if (currentPath.startsWith('/Admin')) {
            this.initAdminPages();
        }
        
        // Maps
        if (currentPath === '/Map' || currentPath === '/map') {
            this.initMapFeatures();
        }
        
        // Vehicle pages
        if (currentPath === '/Vehicles' || currentPath === '/vehicles') {
            this.initVehicleFeatures();
        }
    }

    initDashboard() {
        console.log('🏠 Initializing dashboard features');
        
        // Initialize dashboard charts if present
        this.initDashboardCharts();
        
        // Setup dashboard real-time updates
        this.setupDashboardUpdates();
    }

    initAdminPages() {
        console.log('⚙️ Initializing admin features');
        
        // Setup admin-specific features
        this.setupAdminFeatures();
        
        // Initialize data tables if present
        this.initDataTables();
    }

    initMapFeatures() {
        console.log('🗺️ Initializing map features');
        // Map initialization would go here
    }

    initVehicleFeatures() {
        console.log('🚲 Initializing vehicle features');
        // Vehicle-specific features would go here
    }

    // Dashboard charts
    initDashboardCharts() {
        const chartElements = document.querySelectorAll('[data-chart]');
        chartElements.forEach(element => {
            this.initChart(element);
        });
    }

    initChart(element) {
        const chartType = element.getAttribute('data-chart');
        console.log(`📊 Initializing ${chartType} chart`);
        // Chart initialization logic would go here
    }

    // Admin features
    setupAdminFeatures() {
        // Setup admin data tables
        this.setupAdminTables();
        
        // Setup admin action buttons
        this.setupAdminActions();
    }

    setupAdminTables() {
        const tables = document.querySelectorAll('.admin-table');
        tables.forEach(table => {
            this.enhanceTable(table);
        });
    }

    enhanceTable(table) {
        // Add sorting, filtering, pagination
        console.log('📋 Enhancing admin table');
    }

    setupAdminActions() {
        // Admin action buttons are already handled by the click interceptor
        console.log('🔧 Admin actions ready');
    }

    // Data tables initialization
    initDataTables() {
        const dataTableElements = document.querySelectorAll('[data-table]');
        dataTableElements.forEach(element => {
            this.initDataTable(element);
        });
    }

    initDataTable(element) {
        console.log('📊 Initializing data table');
        // DataTable initialization would go here
    }

    // Dashboard updates
    setupDashboardUpdates() {
        // Setup real-time dashboard updates
        if (this.isAuthenticated) {
            this.startDashboardPolling();
        }
    }

    startDashboardPolling() {
        // Poll for dashboard updates every 30 seconds
        setInterval(() => {
            this.updateDashboardStats();
        }, 30000);
    }

    async updateDashboardStats() {
        try {
            // Update dashboard statistics
            console.log('📈 Updating dashboard stats');
        } catch (error) {
            console.error('Dashboard update failed:', error);
        }
    }

    // Search results display
    displaySearchResults(results) {
        const resultsContainer = this.getOrCreateSearchResults();
        
        if (!results || results.length === 0) {
            resultsContainer.innerHTML = '<p class="no-results">Nessun risultato trovato</p>';
            return;
        }
        
        resultsContainer.innerHTML = results.map(result => `
            <div class="search-result-item" data-url="${result.url}">
                <div class="result-type">${result.type}</div>
                <div class="result-title">${result.title}</div>
            </div>
        `).join('');
        
        // Add click handlers
        resultsContainer.querySelectorAll('.search-result-item').forEach(item => {
            item.addEventListener('click', () => {
                const url = item.getAttribute('data-url');
                if (url) {
                    this.showPageTransition();
                    window.location.href = url;
                }
            });
        });
        
        resultsContainer.classList.add('visible');
    }

    getOrCreateSearchResults() {
        let container = document.querySelector('.search-results');
        if (!container) {
            container = document.createElement('div');
            container.className = 'search-results';
            container.style.cssText = `
                position: absolute;
                top: 100%;
                left: 0;
                right: 0;
                background: white;
                border: 1px solid #ddd;
                border-radius: 0 0 8px 8px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                max-height: 300px;
                overflow-y: auto;
                z-index: 1000;
                opacity: 0;
                visibility: hidden;
                transition: all 0.2s ease;
            `;
            
            const searchBox = document.querySelector('.search-box');
            if (searchBox) {
                searchBox.style.position = 'relative';
                searchBox.appendChild(container);
            }
        }
        return container;
    }

    showSearchLoading() {
        const container = this.getOrCreateSearchResults();
        container.innerHTML = '<div class="search-loading">Ricerca in corso...</div>';
        container.classList.add('visible');
    }

    hideSearchResults() {
        const container = document.querySelector('.search-results');
        if (container) {
            container.classList.remove('visible');
        }
    }

    // User notifications
    loadSavedNotifications() {
        try {
            const saved = localStorage.getItem('notifications');
            if (saved) {
                const notifications = JSON.parse(saved);
                this.notificationCount = notifications.length;
                this.updateNotificationBadge();
            }
        } catch (error) {
            console.error('Failed to load saved notifications:', error);
        }
    }

    async loadUserNotifications() {
        if (!this.isAuthenticated) return;
        
        try {
            // Load user-specific notifications
            console.log('📬 Loading user notifications');
        } catch (error) {
            console.error('Failed to load user notifications:', error);
        }
    }

    // Utility functions
    throttle(func, limit) {
        let inThrottle;
        return function() {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }

    debounce(func, wait, immediate) {
        let timeout;
        return function executedFunction() {
            const context = this;
            const args = arguments;
            const later = function() {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    }

    // Screen reader announcements
    announceToScreenReader(message) {
        const announcement = document.createElement('div');
        announcement.setAttribute('aria-live', 'polite');
        announcement.setAttribute('aria-atomic', 'true');
        announcement.style.cssText = `
            position: absolute;
            left: -10000px;
            width: 1px;
            height: 1px;
            overflow: hidden;
        `;
        announcement.textContent = message;
        
        document.body.appendChild(announcement);
        
        setTimeout(() => {
            if (announcement.parentNode) {
                announcement.parentNode.removeChild(announcement);
            }
        }, 1000);
    }

    // Gestione logout
    logout() {
        try {
            console.log('🔓 Logout in corso...');
            
            // Pulisci tutti i dati locali
            this.currentUser = null;
            this.isAuthenticated = false;
            localStorage.removeItem('auth_token');
            localStorage.removeItem('user_data');
            
            // Rimuovi i cookie persistenti dal client
            document.cookie = 'PersistentToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
            document.cookie = 'PersistentUser=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
            
            // Reindirizza al logout del server
            window.location.href = '/Logout';
        } catch (error) {
            console.error('❌ Errore durante il logout:', error);
            // Fallback: reindirizza comunque
            window.location.href = '/Logout';
        }
    }

    // Prevenzione navigazione indietro nelle pagine protette
    preventBackNavigation() {
        if (this.isAuthenticated) {
            // Se l'utente è autenticato, previeni la navigazione indietro
            window.history.pushState(null, null, window.location.href);
            window.addEventListener('popstate', () => {
                window.history.pushState(null, null, window.location.href);
            });
        }
    }

    // Controlla se la pagina corrente richiede autenticazione
    isProtectedPage() {
    const protectedPaths = ['/dashboard', '/vehicles', '/trips', '/billing', '/profile', '/admin'];
    const currentPath = window.location.pathname.toLowerCase();
    return protectedPaths.some(path => currentPath.startsWith(path));
    }

    // Gestione cambio pagina
    handlePageChange() {
        // If there's a client token or server session, do not redirect. Ask server before redirecting.
        const storedToken = this.getStoredToken();
        if (this.isProtectedPage() && !this.isAuthenticated && !storedToken) {
            (async () => {
                const maxAttempts = 5;
                const delayMs = 300;
                let ok = false;
                for (let i = 0; i < maxAttempts; i++) {
                    try {
                            const resp = await fetch(location.origin + '/Auth/Current', { credentials: 'same-origin' });
                        if (resp.ok) {
                            const data = await resp.json();
                            if (data && data.isAuthenticated) {
                                console.debug('Server session valid, not redirecting to Login (attempt', i + 1, ')');
                                if (data.user) this.setCurrentUser(data.user);
                                ok = true;
                                break;
                            }
                        }
                    } catch (e) {
                        console.debug('Attempt', i + 1, 'failed to verify server session', e);
                    }

                    // small delay before retrying
                    await new Promise(r => setTimeout(r, delayMs));
                }

                if (!ok) {
                    console.log('🚫 Pagina protetta senza autenticazione dopo tentativi, reindirizzamento...');
                    if (window.safeRedirect) {
                        window.safeRedirect('/Login');
                    } else {
                        window.location.href = '/Login';
                    }
                }
            })();
        }
    }
}

// CSS Injection for enhanced styles
function injectEnhancedStyles() {
    if (document.querySelector('#premium-app-styles')) return;
    
    const style = document.createElement('style');
    style.id = 'premium-app-styles';
    style.textContent = `
        /* Premium App Enhanced Styles */
        .premium-tooltip.visible {
            opacity: 1 !important;
        }
        
        .search-results.visible {
            opacity: 1 !important;
            visibility: visible !important;
        }
        
        .search-result-item {
            padding: 12px 16px;
            border-bottom: 1px solid #eee;
            cursor: pointer;
            transition: background-color 0.2s ease;
        }
        
        .search-result-item:hover {
            background-color: #f8f9fa;
        }
        
        .search-result-item:last-child {
            border-bottom: none;
        }
        
        .result-type {
            font-size: 0.75rem;
            color: #6c757d;
            text-transform: uppercase;
            font-weight: 500;
            margin-bottom: 2px;
        }
        
        .result-title {
            font-weight: 500;
            color: #343a40;
        }
        
        .no-results {
            padding: 16px;
            text-align: center;
            color: #6c757d;
            font-style: italic;
        }
        
        .search-loading {
            padding: 16px;
            text-align: center;
            color: #6c757d;
        }
        
        .page-transition-loader {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(255, 255, 255, 0.9);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10001;
            backdrop-filter: blur(2px);
        }
        
        .transition-content {
            text-align: center;
            padding: 2rem;
        }
        
        .transition-spinner {
            width: 40px;
            height: 40px;
            border: 4px solid #e3f2fd;
            border-top: 4px solid #2196f3;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin: 0 auto 1rem;
        }
        
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        
        .loading-overlay {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.5);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10000;
        }
        
        .loading-overlay.hidden {
            display: none;
        }
        
        .loading-content {
            background: white;
            padding: 2rem;
            border-radius: 12px;
            text-align: center;
            box-shadow: 0 8px 32px rgba(0,0,0,0.1);
        }
        
        .loading-spinner {
            width: 32px;
            height: 32px;
            border: 3px solid #e3f2fd;
            border-top: 3px solid #2196f3;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin: 0 auto 1rem;
        }
        
        .loading-spinner-sm {
            width: 16px;
            height: 16px;
            border: 2px solid rgba(255,255,255,0.3);
            border-top: 2px solid white;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            display: inline-block;
            margin-right: 8px;
        }
        
        .loading-text {
            color: #666;
            font-weight: 500;
            margin: 0;
        }
        
        .notification-container {
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 10000;
        }
        
        .field-error {
            color: #dc3545;
            font-size: 0.875rem;
            margin-top: 0.25rem;
        }
        
        .form-group.has-error input,
        .form-group.has-error select,
        .form-group.has-error textarea {
            border-color: #dc3545;
        }
        
        .form-group.has-success input,
        .form-group.has-success select,
        .form-group.has-success textarea {
            border-color: #28a745;
        }
        
        .modal-backdrop {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.5);
            z-index: 1050;
        }
        
        .modal.active {
            display: block;
        }
        
        body.modal-open {
            overflow: hidden;
        }
        
        body.sidebar-open {
            overflow: hidden;
        }
        
        .header-hidden {
            transform: translateY(-100%);
        }
        
        /* Admin action buttons enhancement */
        .admin-action-btn {
            transition: all 0.2s ease;
        }
        
        .admin-action-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        }
        
        /* Theme transitions */
        [data-theme="dark"] {
            --bg-primary: #1a1a1a;
            --bg-secondary: #2d2d2d;
            --text-primary: #ffffff;
            --text-secondary: #b3b3b3;
        }
        
        [data-theme="light"] {
            --bg-primary: #ffffff;
            --bg-secondary: #f8f9fa;
            --text-primary: #212529;
            --text-secondary: #6c757d;
        }
    `;
    
    document.head.appendChild(style);
}

// Instantiate when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    try {
        // Inject enhanced styles
        injectEnhancedStyles();
        
        if (typeof PremiumApp !== 'undefined' && !window.premiumApp) {
            window.premiumApp = new PremiumApp();
            // Rendi disponibile globalmente per compatibilità con altre parti dell'app
            window.app = window.premiumApp;
            window.authManager = window.premiumApp;
        }
    } catch (e) {
        console.debug('Could not instantiate PremiumApp', e);
    }
});

// Debug helper: safeRedirect logs a stack trace before navigating so we can
// identify where redirects to login are coming from during debugging.
if (!window.safeRedirect) {
    // recordRedirect saves the redirect details to localStorage so the info
    // survives navigation (we can inspect it on the destination page).
    function recordRedirect(url) {
        try {
            const stack = (new Error()).stack || '';
            const payload = {
                url: url,
                timestamp: new Date().toISOString(),
                stack: stack
            };
            localStorage.setItem('lastRedirectTrace', JSON.stringify(payload));
        } catch (e) {
            // ignore storage failures
        }
    }

    window.safeRedirect = function (url) {
        try {
            console.groupCollapsed('[safeRedirect] Navigating to: ' + url);
            console.trace();
            console.groupEnd();
        } catch (e) {
            console.debug('safeRedirect trace failed', e);
        }
        // persist trace for inspection after navigation
        recordRedirect(url);
        window.location.href = url;
    };
}

// On page load show last redirect trace (if any) so we can inspect causes
try {
    const last = localStorage.getItem('lastRedirectTrace');
    if (last) {
        try {
            const obj = JSON.parse(last);
            console.groupCollapsed('[lastRedirectTrace] Previous redirect recorded -> ' + (obj.url || ''));
            console.log('timestamp:', obj.timestamp);
            console.log('url:', obj.url);
            console.log('stack:', obj.stack);
            console.groupEnd();
        } catch (e) {
            console.log('lastRedirectTrace (raw):', last);
        }
        // keep the trace for a short time to allow inspection, then clear
        setTimeout(() => localStorage.removeItem('lastRedirectTrace'), 10000);
    }
} catch (e) {
    // ignore storage access errors
}
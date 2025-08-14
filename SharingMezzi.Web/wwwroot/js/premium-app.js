// SharingMezzi Premium App - Enhanced JavaScript

class PremiumApp {
    constructor() {
        this.currentUser = null;
        this.notificationCount = 0;
        this.isAuthenticated = false;
        this.init();
    }

    async init() {
        console.log('🚀 Initializing SharingMezzi Premium App...');
        
        try {
            // Core initialization
            this.setupGlobalErrorHandling();
            this.setupEventListeners();
            this.setupNavigation();
            this.setupTheme();
            this.setupNotifications();
            this.setupLoadingStates();
            this.setupFormValidation();
            this.setupModalHandlers();
            this.setupTooltips();
            this.setupLazyLoading();
            
            // Authentication check
            await this.checkAuthentication();
            
            // Page-specific initializations
            this.initPageSpecific();
            
            console.log('✅ App initialized successfully');
        } catch (error) {
            console.error('❌ App initialization failed:', error);
            this.showNotification('Errore di inizializzazione', 'error');
        }
    }

    // Enhanced error handling
    setupGlobalErrorHandling() {
        window.addEventListener('error', (event) => {
            console.error('Global error:', event.error);
            this.handleError(event.error);
        });

        window.addEventListener('unhandledrejection', (event) => {
            console.error('Unhandled promise rejection:', event.reason);
            this.handleError(event.reason);
        });
    }

    handleError(error) {
        // Don't show error notifications for minor issues
        if (error.name === 'NetworkError' || error.name === 'AbortError') {
            return;
        }

        this.showNotification(
            'Si è verificato un errore. Riprova tra qualche istante.',
            'error'
        );
    }

    // Enhanced event listeners
    setupEventListeners() {
        // Mobile menu toggle
        const mobileMenuBtn = document.querySelector('.mobile-menu-btn');
        const sidebar = document.querySelector('.premium-sidebar');
        
        if (mobileMenuBtn && sidebar) {
            mobileMenuBtn.addEventListener('click', () => {
                sidebar.classList.toggle('show');
                document.body.classList.toggle('sidebar-open');
            });
        }

        // Close mobile menu when clicking outside
        document.addEventListener('click', (e) => {
            if (sidebar && sidebar.classList.contains('show')) {
                if (!sidebar.contains(e.target) && !mobileMenuBtn.contains(e.target)) {
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
    }

    setupEnhancedSearch() {
        const searchBox = document.querySelector('.search-box input');
        if (!searchBox) return;

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
            // Ctrl/Cmd + K for search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchBox = document.querySelector('.search-box input');
                if (searchBox) {
                    searchBox.focus();
                }
            }
            
            // Escape to close modals
            if (e.key === 'Escape') {
                this.closeAllModals();
            }
        });
    }

    setupScrollEnhancements() {
        let scrollTimeout;
        
        window.addEventListener('scroll', () => {
            // Show/hide scroll to top button
            const scrollTop = window.pageYOffset;
            const scrollTopBtn = document.querySelector('.scroll-top-btn');
            
            if (scrollTopBtn) {
                if (scrollTop > 500) {
                    scrollTopBtn.classList.add('visible');
                } else {
                    scrollTopBtn.classList.remove('visible');
                }
            }
            
            // Update navigation highlighting
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(() => {
                this.updateNavigationHighlight();
            }, 100);
        });
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

    // Enhanced theme system
    setupTheme() {
        // Check for saved theme preference
        const savedTheme = localStorage.getItem('theme');
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        
        // Set initial theme
        if (savedTheme === 'dark' || (!savedTheme && prefersDark)) {
            document.documentElement.setAttribute('data-theme', 'dark');
            this.updateThemeIcon(true);
        } else {
            document.documentElement.setAttribute('data-theme', 'light');
            this.updateThemeIcon(false);
        }
        
        // Theme toggle functionality
        const themeToggle = document.getElementById('themeToggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', () => {
                this.toggleTheme();
            });
        }

        // Listen for system theme changes
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
            if (!localStorage.getItem('theme')) {
                const newTheme = e.matches ? 'dark' : 'light';
                document.documentElement.setAttribute('data-theme', newTheme);
                this.updateThemeIcon(e.matches);
            }
        });
    }

    toggleTheme() {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        
        // Add transition effect
        document.documentElement.style.transition = 'all 0.3s ease';
        
        document.documentElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        this.updateThemeIcon(newTheme === 'dark');
        
        // Remove transition after animation
        setTimeout(() => {
            document.documentElement.style.transition = '';
        }, 300);

        // Announce theme change for accessibility
        this.announceToScreenReader(`Tema ${newTheme === 'dark' ? 'scuro' : 'chiaro'} attivato`);
    }

    updateThemeIcon(isDark) {
        const themeToggle = document.getElementById('themeToggle');
        if (themeToggle) {
            themeToggle.innerHTML = isDark 
                ? '<i class="fas fa-sun"></i>' 
                : '<i class="fas fa-moon"></i>';
            themeToggle.title = isDark ? 'Attiva tema chiaro' : 'Attiva tema scuro';
            themeToggle.setAttribute('aria-label', isDark ? 'Attiva tema chiaro' : 'Attiva tema scuro');
        }
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

        notification.innerHTML = `
            <div class="notification-content">
                <div class="notification-icon">
                    <i class="${iconMap[type]}"></i>
                </div>
                <div class="notification-message">
                    <p>${message}</p>
                    ${actions ? `<div class="notification-actions">${actions}</div>` : ''}
                </div>
                <button class="notification-close" aria-label="Chiudi notifica">
                    <i class="fas fa-times"></i>
                </button>
            </div>
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
            notification.classList.add('visible');
        });

        return notification;
    }

    dismissNotification(notification) {
        notification.classList.add('animate-slide-out');
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

    // Enhanced authentication
    async checkAuthentication() {
        try {
            const token = localStorage.getItem('auth_token');
            if (!token) {
                this.handleUnauthenticated();
                return;
            }

            // Validate token with server
            const response = await fetch('/api/auth/validate', {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const userData = await response.json();
                this.setCurrentUser(userData);
            } else {
                this.handleUnauthenticated();
            }
            
        } catch (error) {
            console.error('Authentication check failed:', error);
            this.handleUnauthenticated();
        }
    }

    setCurrentUser(user) {
        this.currentUser = user;
        this.isAuthenticated = true;
        this.updateUserInterface();
    }

    handleUnauthenticated() {
        this.currentUser = null;
        this.isAuthenticated = false;
        localStorage.removeItem('auth_token');
        
        // Redirect to login if on protected page
        const protectedPaths = ['/vehicles', '/trips', '/billing', '/profile', '/admin'];
        const currentPath = window.location.pathname;
        
        if (protectedPaths.some(path => currentPath.startsWith(path))) {
            window.location.href = '/auth/login';
        }
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

    toggleAdminFeatures() {
        const adminElements = document.querySelectorAll('.admin-only');
        const isAdmin = this.currentUser && this.currentUser.ruolo === 1;

        adminElements.forEach(el => {
            el.style.display = isAdmin ? 'block' : 'none';
        });
    }

    // Enhanced form validation
    setupFormValidation() {
        const forms = document.querySelectorAll('form[data-validate]');
        
        forms.forEach(form => {
            this.initFormValidation(form);
        });
    }

    initFormValidation(form) {
        const inputs = form.querySelectorAll('input, select, textarea');
        
        inputs.forEach(input => {
            input.addEventListener('blur', () => {
                this.validateField(input);
            });
            
            input.addEventListener('input', () => {
                // Clear validation state on input
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
        
        const arrow = document.createElement('div');
        arrow.className = 'tooltip-arrow';
        tooltip.appendChild(arrow);
        
        return tooltip;
    }

    positionTooltip(element, tooltip) {
        const elementRect = element.getBoundingClientRect();
        const tooltipRect = tooltip.getBoundingClientRect();
        
        const top = elementRect.top - tooltipRect.height - 10;
        const left = elementRect.left + (elementRect.width - tooltipRect.width) / 2;
        
        tooltip.style.top = `${top + window.scrollY}px`;
        tooltip.style.left = `${Math.max(10, Math.min(left, window.innerWidth - tooltipRect.width - 10))}px`;
    }

    // Lazy loading for images
    setupLazyLoading() {
        const lazyImages = document.querySelectorAll('img[data-src]');
        
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.src = img.dataset.src;
                        img.classList.remove('lazy');
                        imageObserver.unobserve(img);
                    }
                });
            });

            lazyImages.forEach(img => imageObserver.observe(img));
        } else {
            // Fallback for older browsers
            lazyImages.forEach(img => {
                img.src = img.dataset.src;
                img.classList.remove('lazy');
            });
        }
    }

    // Page-specific initializations
    initPageSpecific() {
        const currentPage = this.getCurrentPage();
        
        switch (currentPage) {
            case 'home':
                this.initHomePage();
                break;
            case 'vehicles':
                this.initVehiclesPage();
                break;
            case 'map':
                this.initMapPage();
                break;
            case 'dashboard':
                this.initDashboardPage();
                break;
            case 'admin':
                this.initAdminPages();
                break;
        }
    }

    getCurrentPage() {
        const path = window.location.pathname;
        
        if (path === '/' || path === '/home') return 'home';
        if (path.startsWith('/vehicles')) return 'vehicles';
        if (path.startsWith('/map')) return 'map';
        if (path.startsWith('/dashboard')) return 'dashboard';
        if (path.startsWith('/admin')) return 'admin';
        
        return 'default';
    }

    initHomePage() {
        // Homepage-specific initialization
        console.log('📄 Initializing homepage...');
    }

    initVehiclesPage() {
        // Vehicles page initialization
        console.log('🚲 Initializing vehicles page...');
        this.setupVehicleFilters();
        this.setupVehicleGrid();
    }

    initMapPage() {
        // Map page initialization
        console.log('🗺️ Initializing map page...');
    }

    initDashboardPage() {
        // Dashboard initialization
        console.log('📊 Initializing dashboard...');
        this.setupDashboardCharts();
        this.setupDashboardRefresh();
    }

    initAdminPages() {
        // Admin pages initialization
        console.log('👨‍💼 Initializing admin pages...');
    }

    // Utility functions
    setupVehicleFilters() {
        const filterForm = document.querySelector('.vehicle-filters');
        if (!filterForm) return;

        const inputs = filterForm.querySelectorAll('input, select');
        inputs.forEach(input => {
            input.addEventListener('change', this.debounce(() => {
                this.applyVehicleFilters();
            }, 300));
        });
    }

    setupVehicleGrid() {
        const vehicleGrid = document.querySelector('.vehicle-grid');
        if (!vehicleGrid) return;

        // Add interaction handlers for vehicle cards
        const vehicleCards = vehicleGrid.querySelectorAll('.vehicle-card');
        vehicleCards.forEach(card => {
            card.addEventListener('click', (e) => {
                if (!e.target.closest('button')) {
                    const vehicleId = card.dataset.vehicleId;
                    this.showVehicleDetails(vehicleId);
                }
            });
        });
    }

    async applyVehicleFilters() {
        // Implementation for vehicle filtering
        console.log('Applying vehicle filters...');
    }

    async showVehicleDetails(vehicleId) {
        // Implementation for showing vehicle details
        console.log('Showing vehicle details for:', vehicleId);
    }

    setupDashboardCharts() {
        // Implementation for dashboard charts
        console.log('Setting up dashboard charts...');
    }

    setupDashboardRefresh() {
        const refreshBtn = document.querySelector('.dashboard-refresh');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => {
                this.refreshDashboard();
            });
        }

        // Auto-refresh every 5 minutes
        setInterval(() => {
            this.refreshDashboard();
        }, 5 * 60 * 1000);
    }

    async refreshDashboard() {
        console.log('Refreshing dashboard data...');
        // Implementation for dashboard refresh
    }

    // Accessibility utilities
    announceToScreenReader(message) {
        const announcement = document.createElement('div');
        announcement.setAttribute('aria-live', 'polite');
        announcement.setAttribute('aria-atomic', 'true');
        announcement.className = 'sr-only';
        announcement.textContent = message;
        
        document.body.appendChild(announcement);
        
        setTimeout(() => {
            document.body.removeChild(announcement);
        }, 1000);
    }

    // Utility functions
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

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

    // API utilities
    async apiCall(endpoint, options = {}) {
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                ...(this.isAuthenticated && { 'Authorization': `Bearer ${localStorage.getItem('auth_token')}` })
            }
        };

        const config = { ...defaultOptions, ...options };
        
        try {
            const response = await fetch(endpoint, config);
            
            if (response.status === 401) {
                this.handleUnauthenticated();
                throw new Error('Non autorizzato');
            }
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            return await response.json();
        } catch (error) {
            console.error('API call failed:', error);
            throw error;
        }
    }

    // Cleanup
    destroy() {
        // Remove all event listeners and clean up
        console.log('🧹 Cleaning up Premium App...');
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.premiumApp = new PremiumApp();
});

// Export for external use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = PremiumApp;
}
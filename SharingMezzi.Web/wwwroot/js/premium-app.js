// Premium App JavaScript
class PremiumApp {
    constructor() {
        this.init();
    }

    init() {
        this.setupSidebar();
        this.setupNavigation();
        this.setupUserInfo();
        this.setupNotifications();
        this.setupTheme();
        this.bindEvents();
    }

    setupSidebar() {
        const sidebar = document.getElementById('sidebar');
        const content = document.getElementById('content');
        
        // Load sidebar state from localStorage
        const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        if (isCollapsed) {
            sidebar.classList.add('collapsed');
        }

        // Toggle sidebar
        document.querySelector('.sidebar-toggle').addEventListener('click', () => {
            sidebar.classList.toggle('collapsed');
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
        });

        // Active nav item
        this.setActiveNavItem();
    }

    setupNavigation() {
        // Set active navigation item based on current URL
        const navLinks = document.querySelectorAll('.nav-link');
        const currentPath = window.location.pathname;
        
        navLinks.forEach(link => {
            const href = link.getAttribute('href');
            if (href === currentPath || (currentPath.startsWith(href) && href !== '/')) {
                link.classList.add('active');
            }
        });
    }

    setActiveNavItem() {
        const navLinks = document.querySelectorAll('.sidebar-nav .nav-link');
        const currentPath = window.location.pathname;
        
        navLinks.forEach(link => {
            link.classList.remove('active');
            const href = link.getAttribute('href');
            if (href === currentPath || (currentPath.startsWith(href) && href !== '/')) {
                link.classList.add('active');
            }
        });
    }

    async setupUserInfo() {
        try {
            const response = await fetch('/api/auth/profile', {
                headers: {
                    'Authorization': `Bearer ${this.getToken()}`
                }
            });

            if (response.ok) {
                const user = await response.json();
                this.updateUserInfo(user);
                this.updateUserMenu(user);
            }
        } catch (error) {
            console.error('Error fetching user info:', error);
        }
    }

    updateUserInfo(user) {
        const userNameElement = document.querySelector('.user-name');
        const userRoleElement = document.querySelector('.user-role');
        
        if (userNameElement) {
            userNameElement.textContent = `${user.nome} ${user.cognome}`;
        }
        
        if (userRoleElement) {
            userRoleElement.textContent = user.ruolo === 1 ? 'Amministratore' : 'Utente';
        }

        // Show/hide admin menu items
        const adminItems = document.querySelectorAll('.admin-only');
        adminItems.forEach(item => {
            item.style.display = user.ruolo === 1 ? 'block' : 'none';
        });
    }

    updateUserMenu(user) {
        // Update user dropdown in navbar
        const userDropdown = document.querySelector('#userDropdown');
        if (userDropdown) {
            userDropdown.innerHTML = `
                <i class="fas fa-user-circle"></i>
                <span class="d-none d-md-inline">${user.nome}</span>
            `;
        }
    }

    setupNotifications() {
        // Setup notification system (placeholder for SignalR integration)
        this.notificationCount = 0;
        this.updateNotificationBadge();
    }

    updateNotificationBadge() {
        const badge = document.querySelector('.notification-badge');
        if (badge) {
            badge.textContent = this.notificationCount;
            badge.style.display = this.notificationCount > 0 ? 'inline' : 'none';
        }
    }

    setupTheme() {
        // Check for saved theme preference or respect OS preference
        const savedTheme = localStorage.getItem('theme');
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        
        // Set theme based on saved preference or OS preference
        if (savedTheme === 'dark' || (!savedTheme && prefersDark)) {
            document.documentElement.setAttribute('data-theme', 'dark');
            this.updateThemeIcon(true);
        } else {
            document.documentElement.setAttribute('data-theme', 'light');
            this.updateThemeIcon(false);
        }
        
        // Set up theme toggle button
        const themeToggle = document.getElementById('themeToggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', () => {
                const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
                if (isDark) {
                    document.documentElement.setAttribute('data-theme', 'light');
                    localStorage.setItem('theme', 'light');
                    this.updateThemeIcon(false);
                } else {
                    document.documentElement.setAttribute('data-theme', 'dark');
                    localStorage.setItem('theme', 'dark');
                    this.updateThemeIcon(true);
                }
            });
        }
    }
    
    updateThemeIcon(isDark) {
        const themeToggle = document.getElementById('themeToggle');
        if (themeToggle) {
            themeToggle.innerHTML = isDark 
                ? '<i class="fas fa-sun"></i>' 
                : '<i class="fas fa-moon"></i>';
            themeToggle.title = isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode';
        }
    }

    bindEvents() {
        // Confirm dialogs for destructive actions
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('confirm-action')) {
                e.preventDefault();
                const message = e.target.getAttribute('data-confirm') || 'Sei sicuro di voler procedere?';
                if (confirm(message)) {
                    // Proceed with the action
                    const href = e.target.getAttribute('href');
                    if (href) {
                        window.location.href = href;
                    }
                }
            }
        });

        // Auto-hide alerts after 5 seconds
        document.querySelectorAll('.alert').forEach(alert => {
            setTimeout(() => {
                alert.style.opacity = '0';
                setTimeout(() => alert.remove(), 300);
            }, 5000);
        });

        // Loading states for buttons
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('btn-loading')) {
                e.target.disabled = true;
                e.target.innerHTML = '<span class="spinner"></span> Caricamento...';
            }
        });
    }

    getToken() {
        // Get JWT token from session storage or local storage
        return sessionStorage.getItem('token') || localStorage.getItem('token');
    }

    showAlert(message, type = 'info') {
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        const container = document.querySelector('.main-content');
        container.insertBefore(alert, container.firstChild);
        
        // Auto-hide after 5 seconds
        setTimeout(() => {
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 300);
        }, 5000);
    }

    showLoading(element) {
        const original = element.innerHTML;
        element.innerHTML = '<span class="spinner"></span> Caricamento...';
        element.disabled = true;
        
        return () => {
            element.innerHTML = original;
            element.disabled = false;
        };
    }

    formatCurrency(amount) {
        return new Intl.NumberFormat('it-IT', {
            style: 'currency',
            currency: 'EUR'
        }).format(amount);
    }

    formatDate(dateString) {
        return new Intl.DateTimeFormat('it-IT', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        }).format(new Date(dateString));
    }

    formatRelativeTime(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diff = now - date;
        
        const minutes = Math.floor(diff / 60000);
        const hours = Math.floor(minutes / 60);
        const days = Math.floor(hours / 24);
        
        if (days > 0) return `${days} giorni fa`;
        if (hours > 0) return `${hours} ore fa`;
        if (minutes > 0) return `${minutes} minuti fa`;
        return 'Ora';
    }
}

// Utility functions
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('show');
}

function logout() {
    if (confirm('Sei sicuro di voler effettuare il logout?')) {
        fetch('/api/auth/logout', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${app.getToken()}`
            }
        }).finally(() => {
            sessionStorage.removeItem('token');
            localStorage.removeItem('token');
            window.location.href = '/Login';
        });
    }
}

function refreshPage() {
    window.location.reload();
}

// Initialize app when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.app = new PremiumApp();
});

// Export for use in other modules
window.PremiumApp = PremiumApp;

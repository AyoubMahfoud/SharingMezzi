// Authentication JavaScript
class AuthManager {
    constructor() {
        this.apiBaseUrl = 'http://localhost:5000'; // URL dell'API
        this.init();
    }

    init() {
        this.setupForms();
        this.setupPasswordStrength();
        this.checkAuthStatus();
    }

    setupForms() {
        // Register form
        const registerForm = document.getElementById('registerForm');
        if (registerForm) {
            registerForm.addEventListener('submit', this.handleRegister.bind(this));
        }

        // Forgot password form
        const forgotForm = document.getElementById('forgotForm');
        if (forgotForm) {
            forgotForm.addEventListener('submit', this.handleForgotPassword.bind(this));
        }

        // Note: Login form is handled by Razor Pages, not JavaScript
        // This ensures proper server-side session management
    }

    setupPasswordStrength() {
        const passwordInput = document.getElementById('password');
        const strengthIndicator = document.querySelector('.password-strength');
        
        if (passwordInput && strengthIndicator) {
            passwordInput.addEventListener('input', (e) => {
                const strength = this.calculatePasswordStrength(e.target.value);
                this.updatePasswordStrength(strengthIndicator, strength);
            });
        }
    }

    calculatePasswordStrength(password) {
        let score = 0;
        
        if (password.length >= 8) score++;
        if (password.length >= 12) score++;
        if (/[a-z]/.test(password)) score++;
        if (/[A-Z]/.test(password)) score++;
        if (/[0-9]/.test(password)) score++;
        if (/[^A-Za-z0-9]/.test(password)) score++;
        
        if (score < 2) return 'weak';
        if (score < 4) return 'medium';
        if (score < 6) return 'strong';
        return 'very-strong';
    }

    updatePasswordStrength(indicator, strength) {
        indicator.className = `password-strength ${strength}`;
        
        const messages = {
            weak: 'Password debole',
            medium: 'Password media',
            strong: 'Password forte',
            'very-strong': 'Password molto forte'
        };
        
        indicator.querySelector('.password-strength-text').textContent = messages[strength];
    }

    async handleLogin(e) {
        e.preventDefault();
        
        const form = e.target;
        const formData = new FormData(form);
        const submitBtn = form.querySelector('button[type="submit"]');
        const loading = form.querySelector('.loading');
        
        const loginData = {
            email: formData.get('email'),
            password: formData.get('password')
        };

        try {
            // Show loading state
            submitBtn.style.display = 'none';
            loading.classList.add('show');
            
            const response = await fetch(`${this.apiBaseUrl}/api/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(loginData)
            });

            if (!response.ok) {
                console.error('Login response not ok:', response.status, response.statusText);
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            console.log('Login result:', result);

            if (result.success) {
                // Store token in browser storage
                if (formData.get('remember')) {
                    localStorage.setItem('token', result.token);
                } else {
                    sessionStorage.setItem('token', result.token);
                }

                // Store token in session for server-side access
                await this.setServerSession(result.token);

                // Redirect to dashboard
                window.location.href = '/Dashboard';
            } else {
                this.showError(result.message || 'Credenziali non valide');
            }
        } catch (error) {
            console.error('Login error:', error);
            this.showError('Errore di connessione. Riprova più tardi.');
        } finally {
            // Hide loading state
            submitBtn.style.display = 'block';
            loading.classList.remove('show');
        }
    }

    async handleRegister(e) {
        e.preventDefault();
        
        const form = e.target;
        const formData = new FormData(form);
        const submitBtn = form.querySelector('button[type="submit"]');
        const loading = form.querySelector('.loading');
        
        const registerData = {
            email: formData.get('email'),
            nome: formData.get('nome'),
            cognome: formData.get('cognome'),
            password: formData.get('password'),
            telefono: formData.get('telefono')
        };

        // Validate password confirmation
        if (registerData.password !== formData.get('confirmPassword')) {
            this.showError('Le password non corrispondono');
            return;
        }

        try {
            // Show loading state
            submitBtn.style.display = 'none';
            loading.classList.add('show');
            
            const response = await fetch(`${this.apiBaseUrl}/api/auth/register`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(registerData)
            });

            const result = await response.json();

            if (result.success) {
                this.showSuccess('Registrazione completata con successo! Effettua il login.');
                setTimeout(() => {
                    window.location.href = '/Login';
                }, 2000);
            } else {
                this.showError(result.message || 'Errore durante la registrazione');
            }
        } catch (error) {
            console.error('Register error:', error);
            this.showError('Errore di connessione. Riprova più tardi.');
        } finally {
            // Hide loading state
            submitBtn.style.display = 'block';
            loading.classList.remove('show');
        }
    }

    async handleForgotPassword(e) {
        e.preventDefault();
        
        const form = e.target;
        const formData = new FormData(form);
        const submitBtn = form.querySelector('button[type="submit"]');
        const loading = form.querySelector('.loading');
        
        const email = formData.get('email');

        try {
            // Show loading state
            submitBtn.style.display = 'none';
            loading.classList.add('show');
            
            const response = await fetch('/api/auth/forgot-password', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email })
            });

            const result = await response.json();

            if (result.success) {
                this.showSuccess('Email di reset inviata! Controlla la tua casella di posta.');
            } else {
                this.showError(result.message || 'Errore durante l\'invio dell\'email');
            }
        } catch (error) {
            console.error('Forgot password error:', error);
            this.showError('Errore di connessione. Riprova più tardi.');
        } finally {
            // Hide loading state
            submitBtn.style.display = 'block';
            loading.classList.remove('show');
        }
    }

    checkAuthStatus() {
        const token = this.getToken();
        const isAuthPage = window.location.pathname.includes('/Login') || 
                          window.location.pathname.includes('/Register') || 
                          window.location.pathname.includes('/ForgotPassword');
        const isHomePage = window.location.pathname === '/' || window.location.pathname === '/Index';

        if (token && isAuthPage) {
            // User is logged in but on auth page, redirect to dashboard
            window.location.href = '/Dashboard';
        } else if (!token && !isAuthPage && !isHomePage) {
            // User is not logged in but not on auth page (except home), redirect to login
            window.location.href = '/Login';
        }
    }

    getToken() {
        return localStorage.getItem('token') || sessionStorage.getItem('token');
    }

    removeToken() {
        localStorage.removeItem('token');
        sessionStorage.removeItem('token');
    }

    showError(message) {
        this.showAlert(message, 'danger');
    }

    showSuccess(message) {
        this.showAlert(message, 'success');
    }

    showInfo(message) {
        this.showAlert(message, 'info');
    }

    showAlert(message, type) {
        // Remove existing alerts
        const existingAlerts = document.querySelectorAll('.alert');
        existingAlerts.forEach(alert => alert.remove());

        // Create new alert
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        // Insert alert at the top of the auth card
        const authCard = document.querySelector('.auth-card');
        if (authCard) {
            authCard.insertBefore(alert, authCard.firstChild);
        }

        // Auto-hide after 5 seconds
        setTimeout(() => {
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 300);
        }, 5000);
    }

    // Social login methods (placeholder)
    async loginWithGoogle() {
        console.log('Google login not implemented yet');
        this.showInfo('Login con Google non ancora implementato');
    }

    async loginWithFacebook() {
        console.log('Facebook login not implemented yet');
        this.showInfo('Login con Facebook non ancora implementato');
    }

    // Set server session
    async setServerSession(token) {
        try {
            await fetch('/Auth/SetSession', {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ token: token })
            });
        } catch (error) {
            console.error('Error setting server session:', error);
        }
    }

    // Utility methods
    isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    isValidPassword(password) {
        return password.length >= 6;
    }

    isValidPhone(phone) {
        const phoneRegex = /^[+]?[\d\s\-\(\)]+$/;
        return phoneRegex.test(phone);
    }
}

// Initialize auth manager when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.authManager = new AuthManager();
});

// Export for use in other modules
window.AuthManager = AuthManager;

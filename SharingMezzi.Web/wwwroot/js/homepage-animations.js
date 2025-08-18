// Homepage Animations and Interactions

class HomepageAnimations {
    constructor() {
        this.init();
    }

    init() {
        this.setupIntersectionObserver();
        this.setupCountUpAnimations();
        this.setupFloatingCards();
        this.setupParallaxEffects();
        this.setupSmoothScrolling();
        this.initParticleBackground();
    }

    // Intersection Observer for scroll animations
    setupIntersectionObserver() {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');
                    
                    // Trigger count-up animation if element has the class
                    if (entry.target.classList.contains('animate-count-up')) {
                        this.animateCountUp(entry.target);
                    }
                }
            });
        }, observerOptions);

        // Observe all elements with animation classes
        document.querySelectorAll('.animate-on-scroll, .animate-count-up').forEach(el => {
            observer.observe(el);
        });
    }

    // Count-up animation for statistics
    animateCountUp(element) {
        const numberElement = element.querySelector('.stat-number');
        if (!numberElement) return;

        const finalValue = parseInt(numberElement.getAttribute('data-count'));
        const duration = 2000; // 2 seconds
        const startTime = Date.now();
        const startValue = 0;

        const updateCounter = () => {
            const currentTime = Date.now();
            const elapsedTime = currentTime - startTime;
            const progress = Math.min(elapsedTime / duration, 1);

            // Easing function (ease-out)
            const easeOut = 1 - Math.pow(1 - progress, 3);
            const currentValue = Math.floor(startValue + (finalValue - startValue) * easeOut);

            numberElement.textContent = this.formatNumber(currentValue);

            if (progress < 1) {
                requestAnimationFrame(updateCounter);
            }
        };

        requestAnimationFrame(updateCounter);
    }

    // Format numbers with appropriate suffixes
    formatNumber(num) {
        if (num >= 1000000) {
            return (num / 1000000).toFixed(1) + 'M';
        }
        if (num >= 1000) {
            return (num / 1000).toFixed(1) + 'K';
        }
        return num.toString();
    }

    // Enhanced floating cards animation
    setupFloatingCards() {
        const cards = document.querySelectorAll('.floating-card');
        
        cards.forEach((card, index) => {
            // Add mouse interaction
            card.addEventListener('mouseenter', () => {
                card.style.transform = 'translateY(-10px) scale(1.05)';
                card.style.transition = 'all 0.3s ease';
            });

            card.addEventListener('mouseleave', () => {
                card.style.transform = '';
                card.style.transition = 'all 0.3s ease';
            });

            // Add random delay to floating animation
            const randomDelay = Math.random() * 2;
            card.style.animationDelay = `${randomDelay}s`;
        });
    }

    // Parallax effect for hero section
    setupParallaxEffects() {
        const hero = document.querySelector('.premium-hero');
        const heroCircle = document.querySelector('.hero-circle');
        const floatingCards = document.querySelectorAll('.floating-card');

        if (!hero) return;

        window.addEventListener('scroll', () => {
            const scrolled = window.pageYOffset;
            const rate = scrolled * -0.5;

            if (heroCircle) {
                heroCircle.style.transform = `translate(-50%, -50%) translateY(${rate}px)`;
            }

            floatingCards.forEach((card, index) => {
                const cardRate = rate * (0.2 + index * 0.1);
                card.style.transform = `translateY(${cardRate}px)`;
            });
        });
    }

    // Smooth scrolling for anchor links
    setupSmoothScrolling() {
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                e.preventDefault();
                const target = document.querySelector(this.getAttribute('href'));
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    // Particle background animation
    initParticleBackground() {
        const heroParticles = document.querySelector('.hero-particles');
        if (!heroParticles) return;

        // Create additional animated particles
        for (let i = 0; i < 50; i++) {
            const particle = document.createElement('div');
            particle.className = 'floating-particle';
            particle.style.cssText = `
                position: absolute;
                width: ${Math.random() * 4 + 1}px;
                height: ${Math.random() * 4 + 1}px;
                background: rgba(255, 255, 255, ${Math.random() * 0.3 + 0.1});
                border-radius: 50%;
                left: ${Math.random() * 100}%;
                top: ${Math.random() * 100}%;
                animation: floatParticle ${Math.random() * 20 + 10}s linear infinite;
                animation-delay: ${Math.random() * 5}s;
            `;
            heroParticles.appendChild(particle);
        }

        // Add particle animation keyframes
        if (!document.getElementById('particle-styles')) {
            const style = document.createElement('style');
            style.id = 'particle-styles';
            style.textContent = `
                @keyframes floatParticle {
                    0% {
                        transform: translateY(100vh) rotate(0deg);
                        opacity: 0;
                    }
                    10% {
                        opacity: 1;
                    }
                    90% {
                        opacity: 1;
                    }
                    100% {
                        transform: translateY(-100px) rotate(360deg);
                        opacity: 0;
                    }
                }
            `;
            document.head.appendChild(style);
        }
    }

    // Enhanced hover effects for cards
    setupEnhancedHoverEffects() {
        const cards = document.querySelectorAll('.premium-glass');
        
        cards.forEach(card => {
            card.addEventListener('mouseenter', (e) => {
                const rect = card.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;
                
                card.style.setProperty('--mouse-x', `${x}px`);
                card.style.setProperty('--mouse-y', `${y}px`);
                
                // Add glow effect
                card.style.boxShadow = `
                    0 20px 60px rgba(0, 0, 0, 0.15),
                    ${x}px ${y}px 30px rgba(99, 102, 241, 0.1)
                `;
            });

            card.addEventListener('mouseleave', () => {
                card.style.boxShadow = '';
            });

            card.addEventListener('mousemove', (e) => {
                const rect = card.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;
                
                const centerX = rect.width / 2;
                const centerY = rect.height / 2;
                
                const rotateX = (y - centerY) / 10;
                const rotateY = (centerX - x) / 10;
                
                card.style.transform = `
                    translateY(-10px) 
                    rotateX(${rotateX}deg) 
                    rotateY(${rotateY}deg)
                `;
            });
        });
    }

    // Loading animations
    setupLoadingAnimations() {
        // Add stagger animation to stats cards
        const statCards = document.querySelectorAll('.stat-card');
        statCards.forEach((card, index) => {
            card.style.animationDelay = `${index * 0.1}s`;
            card.classList.add('animate-fade-in-up');
        });

        // Add stagger animation to feature cards
        const featureCards = document.querySelectorAll('.feature-card');
        featureCards.forEach((card, index) => {
            card.style.animationDelay = `${index * 0.2}s`;
            card.classList.add('animate-on-scroll');
        });
    }

    // Magnetic button effect
    setupMagneticButtons() {
        const buttons = document.querySelectorAll('.btn-premium, .btn-outline-premium');
        
        buttons.forEach(button => {
            button.addEventListener('mousemove', (e) => {
                const rect = button.getBoundingClientRect();
                const x = e.clientX - rect.left - rect.width / 2;
                const y = e.clientY - rect.top - rect.height / 2;
                
                button.style.transform = `translate(${x * 0.1}px, ${y * 0.1}px)`;
            });

            button.addEventListener('mouseleave', () => {
                button.style.transform = '';
            });
        });
    }

    // Text reveal animation
    setupTextRevealAnimation() {
        const textElements = document.querySelectorAll('.hero-title, .section-title');
        
        textElements.forEach(element => {
            const text = element.textContent;
            element.innerHTML = '';
            
            const words = text.split(' ');
            words.forEach((word, index) => {
                const wordSpan = document.createElement('span');
                wordSpan.textContent = word + ' ';
                wordSpan.style.opacity = '0';
                wordSpan.style.transform = 'translateY(20px)';
                wordSpan.style.transition = `all 0.6s ease ${index * 0.1}s`;
                element.appendChild(wordSpan);
            });

            // Trigger animation when element is visible
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const spans = entry.target.querySelectorAll('span');
                        spans.forEach(span => {
                            span.style.opacity = '1';
                            span.style.transform = 'translateY(0)';
                        });
                    }
                });
            });

            observer.observe(element);
        });
    }

    // Performance optimization
    throttle(func, wait) {
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

    // Initialize performance-optimized scroll listeners
    setupOptimizedScrollListeners() {
        const optimizedScrollHandler = this.throttle(() => {
            this.updateScrollProgress();
            this.handleScrollAnimations();
        }, 16); // ~60fps

        window.addEventListener('scroll', optimizedScrollHandler, { passive: true });
    }

    updateScrollProgress() {
        const scrolled = window.pageYOffset;
        const maxScroll = document.documentElement.scrollHeight - window.innerHeight;
        const progress = scrolled / maxScroll;
        
        // Update any scroll progress indicators
        const progressBar = document.querySelector('.scroll-progress');
        if (progressBar) {
            progressBar.style.width = `${progress * 100}%`;
        }
    }

    handleScrollAnimations() {
        const scrolled = window.pageYOffset;
        
        // Parallax for hero elements
        const hero = document.querySelector('.premium-hero');
        if (hero && scrolled < window.innerHeight) {
            const heroContent = hero.querySelector('.hero-content');
            const heroVisual = hero.querySelector('.hero-visual');
            
            if (heroContent) {
                heroContent.style.transform = `translateY(${scrolled * 0.3}px)`;
            }
            
            if (heroVisual) {
                heroVisual.style.transform = `translateY(${scrolled * 0.1}px)`;
            }
        }
    }

    // Cleanup function
    destroy() {
        // Remove event listeners and clean up
        window.removeEventListener('scroll', this.optimizedScrollHandler);
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    new HomepageAnimations();
});

// Export for potential external use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = HomepageAnimations;
}
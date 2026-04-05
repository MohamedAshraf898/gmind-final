// GMIND Gaming Platform JavaScript

// DOM Content Loaded Event
document.addEventListener('DOMContentLoaded', function () {
    initializeApp();
    // Fade-in animation for hero section and content
    document.querySelector('.hero')?.classList.add('fade-in-up');
    document.querySelector('.hero-content')?.classList.add('fade-in-up');
    document.querySelector('.hero-text')?.classList.add('fade-in-up');
    document.querySelector('.hero-image')?.classList.add('fade-in-up');
    initializeProductImageSlider();
});

// Initialize Application
function initializeApp() {
    initializeNavigation();
    initializeScrollEffects();
    initializeCountdownTimer();
    initializeImageGallery();
    initializeAnimations();
    initializeProductShowcase();
    initializeFormHandling();
    initializeResponsiveHandlers();
    initializeEventSlider();
}

// Navigation Functionality
function initializeNavigation() {
    const header = document.querySelector('header');
    const navLinks = document.querySelectorAll('.nav-link');
    const mobileMenuToggle = document.getElementById('mobile-menu-toggle');
    const mobileMenu = document.getElementById('mobile-menu');
    const mobileMenuOverlay = document.getElementById('mobile-menu-overlay');
    const mobileMenuClose = document.getElementById('mobile-menu-close');

    // Mobile menu toggle
    if (mobileMenuToggle && mobileMenu) {
        function openMobileMenu() {
            mobileMenuToggle.classList.add("active");
            mobileMenu.classList.add("active");
            if (mobileMenuOverlay) mobileMenuOverlay.classList.add("active");
            document.body.classList.add('menu-open');
            document.body.style.overflow = "hidden";
        }

        function closeMobileMenu() {
            mobileMenuToggle.classList.remove("active");
            mobileMenu.classList.remove("active");
            if (mobileMenuOverlay) mobileMenuOverlay.classList.remove("active");
            document.body.classList.remove('menu-open');
            document.body.style.overflow = "";
        }

        mobileMenuToggle.addEventListener('click', openMobileMenu);
        if (mobileMenuClose) mobileMenuClose.addEventListener("click", closeMobileMenu);
        if (mobileMenuOverlay) mobileMenuOverlay.addEventListener("click", closeMobileMenu);

        // Close mobile menu when clicking on a link
        navLinks.forEach(link => {
            link.addEventListener('click', function () {
                closeMobileMenu();
            });
        });

        // Close menu on escape key
        document.addEventListener("keydown", (e) => {
            if (e.key === "Escape" && mobileMenu.classList.contains("active")) {
                closeMobileMenu();
            }
        });
    }

    // Header scroll effect
    window.addEventListener('scroll', function () {
        if (header) {
            if (window.scrollY > 100) {
                header.classList.add('scrolled');
            } else {
                header.classList.remove('scrolled');
            }
        }
    });

    // Active navigation highlighting
    function updateActiveNavLink() {
        const sections = document.querySelectorAll('section[id]');
        const scrollPos = window.scrollY + 100;

        sections.forEach(section => {
            const sectionTop = section.offsetTop;
            const sectionHeight = section.offsetHeight;
            const sectionId = section.getAttribute('id');
            const navLink = document.querySelector(`.nav-link[href="#${sectionId}"]`);

            if (scrollPos >= sectionTop && scrollPos < sectionTop + sectionHeight) {
                navLinks.forEach(link => link.classList.remove('active'));
                if (navLink) navLink.classList.add('active');
            }
        });
    }

    window.addEventListener('scroll', updateActiveNavLink);

    // Smooth scrolling for navigation links
    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            // Only handle smooth scroll for in-page anchors
            if (href && href.startsWith('#')) {
                e.preventDefault();
                const targetId = href.substring(1);
                const targetSection = document.getElementById(targetId);

                if (targetSection) {
                    const headerHeight = header.offsetHeight;
                    const targetPosition = targetSection.offsetTop - headerHeight;

                    window.scrollTo({
                        top: targetPosition,
                        behavior: 'smooth'
                    });
                }
            }
            // Otherwise, let the browser handle the navigation (for aboutus.html, etc.)
        });
    });

    // Handle window resize for mobile menu
    window.addEventListener('resize', function () {
        if (window.innerWidth > 767) {
            const mToggle = document.getElementById('mobile-menu-toggle');
            const mMenu = document.getElementById('mobile-menu');
            const mOverlay = document.getElementById('mobile-menu-overlay');
            if (mToggle) mToggle.classList.remove('active');
            if (mMenu) mMenu.classList.remove('active');
            if (mOverlay) mOverlay.classList.remove('active');
            document.body.classList.remove('menu-open');
            document.body.style.overflow = "";
        }
    });
}

// Scroll Effects and Animations
function initializeScrollEffects() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in-up');
            }
        });
    }, observerOptions);

    // Observe elements for animation
    const animateElements = document.querySelectorAll(
        '.gallery-item, .service-card, .event-card, .trend-card, .game-card, .testimonial-card'
    );

    animateElements.forEach(element => {
        observer.observe(element);
    });
}

// Countdown Timer Functionality
function initializeCountdownTimer() {
    const timerElements = {
        days: document.getElementById('days'),
        hours: document.getElementById('hours'),
        minutes: document.getElementById('minutes'),
        seconds: document.getElementById('seconds')
    };

    // Set target date (30 days from now for demo)
    const targetDate = new Date();
    targetDate.setDate(targetDate.getDate() + 30);

    function updateCountdown() {
        const now = new Date().getTime();
        const distance = targetDate.getTime() - now;

        if (distance > 0) {
            const days = Math.floor(distance / (1000 * 60 * 60 * 24));
            const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
            const seconds = Math.floor((distance % (1000 * 60)) / 1000);

            if (timerElements.days) timerElements.days.textContent = String(days).padStart(2, '0');
            if (timerElements.hours) timerElements.hours.textContent = String(hours).padStart(2, '0');
            if (timerElements.minutes) timerElements.minutes.textContent = String(minutes).padStart(2, '0');
            if (timerElements.seconds) timerElements.seconds.textContent = String(seconds).padStart(2, '0');
        } else {
            // Timer expired
            if (timerElements.days) timerElements.days.textContent = '00';
            if (timerElements.hours) timerElements.hours.textContent = '00';
            if (timerElements.minutes) timerElements.minutes.textContent = '00';
            if (timerElements.seconds) timerElements.seconds.textContent = '00';
        }
    }

    // Update countdown every second
    updateCountdown();
    setInterval(updateCountdown, 1000);
}

// Image Gallery Functionality
function initializeImageGallery() {
    const galleryItems = document.querySelectorAll('.gallery-item');

    galleryItems.forEach((item, index) => {
        item.addEventListener('click', function () {
            openLightbox(index);
        });

        // Add keyboard support
        item.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                openLightbox(index);
            }
        });

        item.setAttribute('tabindex', '0');
    });
}

// Lightbox functionality for gallery
function openLightbox(index) {
    const images = [
        'Untitled/1.png',
        'Untitled/2.png',
        'Untitled/3.png',
        'Untitled/4.png',
        'Untitled/5.png',
        'Untitled/6.png',
        'Untitled/7.png',
        'Untitled/8.png'
    ];

    const titles = [
        'Main Dashboard',
        'Game Library',
        'Progress Tracking',
        'AR Experience',
        'VR Classroom',
        'Student Portal',
        'Teacher Dashboard',
        'Analytics & Reports'
    ];

    // Create lightbox overlay
    const lightbox = document.createElement('div');
    lightbox.className = 'lightbox-overlay';
    lightbox.innerHTML = `
        <div class="lightbox-content">
            <button class="lightbox-close">&times;</button>
            <img src="${images[index]}" alt="${titles[index]}" class="lightbox-image">
            <div class="lightbox-info">
                <h3>${titles[index]}</h3>
                <p>Screenshot ${index + 1} of ${images.length}</p>
            </div>
            <button class="lightbox-prev" onclick="changeLightboxImage(${index - 1})">&lt;</button>
            <button class="lightbox-next" onclick="changeLightboxImage(${index + 1})">&gt;</button>
        </div>
    `;

    // Add lightbox styles
    lightbox.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.9);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 10000;
        opacity: 0;
        transition: opacity 0.3s ease;
    `;

    document.body.appendChild(lightbox);

    // Fade in effect
    setTimeout(() => {
        lightbox.style.opacity = '1';
    }, 10);

    // Close functionality
    const closeBtn = lightbox.querySelector('.lightbox-close');
    closeBtn.addEventListener('click', closeLightbox);

    lightbox.addEventListener('click', function (e) {
        if (e.target === lightbox) {
            closeLightbox();
        }
    });

    // Keyboard navigation
    document.addEventListener('keydown', handleLightboxKeydown);

    // Store current lightbox for navigation
    window.currentLightbox = lightbox;
    window.currentLightboxIndex = index;
}

function closeLightbox() {
    const lightbox = window.currentLightbox;
    if (lightbox) {
        lightbox.style.opacity = '0';
        setTimeout(() => {
            document.body.removeChild(lightbox);
            document.removeEventListener('keydown', handleLightboxKeydown);
            window.currentLightbox = null;
        }, 300);
    }
}

function handleLightboxKeydown(e) {
    if (e.key === 'Escape') {
        closeLightbox();
    } else if (e.key === 'ArrowLeft') {
        changeLightboxImage(window.currentLightboxIndex - 1);
    } else if (e.key === 'ArrowRight') {
        changeLightboxImage(window.currentLightboxIndex + 1);
    }
}

function changeLightboxImage(newIndex) {
    const totalImages = 8;
    if (newIndex < 0 || newIndex >= totalImages) return;

    closeLightbox();
    setTimeout(() => {
        openLightbox(newIndex);
    }, 100);
}

// Enhanced Animations
function initializeAnimations() {
    // Parallax effect for hero section
    const heroSection = document.querySelector('.hero');
    const heroBlur = document.querySelector('.hero-blur');

    if (heroSection && heroBlur) {
        window.addEventListener('scroll', function () {
            const scrolled = window.pageYOffset;
            const parallaxSpeed = 0.5;

            if (scrolled < heroSection.offsetHeight) {
                heroBlur.style.transform = `translate(-50%, -50%) translateY(${scrolled * parallaxSpeed}px)`;
            }
        });
    }

    // Hover effects for cards (only on devices with hover capability)
    if (window.matchMedia('(hover: hover)').matches) {
        const cards = document.querySelectorAll('.service-card, .event-card, .trend-card, .game-card');

        cards.forEach(card => {
            card.addEventListener('mouseenter', function () {
                this.style.transform = 'translateY(-10px) scale(1.02)';
                this.style.transition = 'all 0.3s ease';
            });

            card.addEventListener('mouseleave', function () {
                this.style.transform = 'translateY(0) scale(1)';
            });
        });
    }

    // Stagger animations for grids
    const gridContainers = document.querySelectorAll('.services-grid, .events-grid, .games-grid');

    gridContainers.forEach(container => {
        const items = container.children;
        Array.from(items).forEach((item, index) => {
            item.style.animationDelay = `${index * 0.1}s`;
        });
    });

    // Touch gesture support for mobile
    initializeTouchGestures();
}

// Touch gesture support
function initializeTouchGestures() {
    let startX = 0;
    let startY = 0;
    let endX = 0;
    let endY = 0;

    // Swipe detection for mobile menu
    document.addEventListener('touchstart', function (e) {
        startX = e.touches[0].clientX;
        startY = e.touches[0].clientY;
    });

    document.addEventListener('touchend', function (e) {
        endX = e.changedTouches[0].clientX;
        endY = e.changedTouches[0].clientY;

        const diffX = startX - endX;
        const diffY = startY - endY;

        // Swipe right to open menu (only if menu is closed)
        if (diffX < -50 && Math.abs(diffY) < 50 && window.innerWidth <= 767) {
            const mobileMenuToggle = document.getElementById('mobile-menu-toggle');
            const mobileMenu = document.getElementById('mobile-menu');
            const mobileMenuOverlay = document.getElementById('mobile-menu-overlay');

            if (mobileMenuToggle && mobileMenu && !mobileMenu.classList.contains('active')) {
                mobileMenuToggle.classList.add('active');
                mobileMenu.classList.add('active');
                if (mobileMenuOverlay) mobileMenuOverlay.classList.add('active');
                document.body.classList.add('menu-open');
                document.body.style.overflow = "hidden";
            }
        }

        // Swipe left to close menu (only if menu is open)
        if (diffX > 50 && Math.abs(diffY) < 50 && window.innerWidth <= 767) {
            const mobileMenuToggle = document.getElementById('mobile-menu-toggle');
            const mobileMenu = document.getElementById('mobile-menu');
            const mobileMenuOverlay = document.getElementById('mobile-menu-overlay');

            if (mobileMenuToggle && mobileMenu && mobileMenu.classList.contains('active')) {
                mobileMenuToggle.classList.remove('active');
                mobileMenu.classList.remove('active');
                if (mobileMenuOverlay) mobileMenuOverlay.classList.remove('active');
                document.body.classList.remove('menu-open');
                document.body.style.overflow = "";
            }
        }
    });
}

// Product Showcase Functionality
function initializeProductShowcase() {
    const indicators = document.querySelectorAll('.indicator');
    const productImages = [
        'https://via.placeholder.com/300x300/CCCCCC/666666?text=Product+View+1',
        'https://via.placeholder.com/300x300/CCCCCC/666666?text=Product+View+2',
        'https://via.placeholder.com/300x300/CCCCCC/666666?text=Product+View+3',
        'https://via.placeholder.com/300x300/CCCCCC/666666?text=Product+View+4'
    ];

    let currentImageIndex = 0;

    indicators.forEach((indicator, index) => {
        indicator.addEventListener('click', function () {
            switchProductImage(index);
        });
    });

    function switchProductImage(index) {
        const productImage = document.querySelector('.product-image img');

        if (productImage && productImages[index]) {
            // Remove active class from all indicators
            indicators.forEach(ind => ind.classList.remove('active'));

            // Add active class to clicked indicator
            indicators[index].classList.add('active');

            // Fade out effect
            productImage.style.opacity = '0';

            setTimeout(() => {
                productImage.src = productImages[index];
                productImage.style.opacity = '1';
            }, 200);

            currentImageIndex = index;
        }
    }

    // Auto-rotate product images
    setInterval(() => {
        const nextIndex = (currentImageIndex + 1) % productImages.length;
        switchProductImage(nextIndex);
    }, 5000);
}

// Form Handling
function initializeFormHandling() {
    const buttons = document.querySelectorAll('.btn');

    buttons.forEach(button => {
        button.addEventListener('click', function (e) {
            const buttonText = this.textContent.trim();

            // Handle different button actions
            switch (buttonText) {
                case 'Sign Up':
                    handleSignUp(e);
                    break;
                case 'Log in':
                    handleLogin(e);
                    break;
                case 'Join Now':
                    handleEventJoin(e);
                    break;
                case 'Add To Cart':
                    handleAddToCart(e);
                    break;
                case 'Install':
                    handleGameInstall(e);
                    break;
                default:
                    // Generic button click feedback
                    showButtonFeedback(this);
            }
        });
    });
}

function handleEventJoin(e) {
    e.preventDefault();
    const eventTitle = e.target.closest('.event-card').querySelector('.event-title').textContent;
    showNotification(`Registration for "${eventTitle}" coming soon!`, 'success');
}

function handleAddToCart(e) {
    e.preventDefault();
    showNotification('Product added to cart!', 'success');
    animateAddToCart(e.target);
}

function handleGameInstall(e) {
    e.preventDefault();
    const gameTitle = e.target.closest('.game-card').querySelector('.game-title').textContent;
    showNotification(`Installing "${gameTitle}"...`, 'info');
    simulateGameInstall(e.target);
}

function showButtonFeedback(button) {
    button.style.transform = 'scale(0.95)';
    setTimeout(() => {
        button.style.transform = 'scale(1)';
    }, 150);
}

// Notification System
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;

    notification.style.cssText = `
        position: fixed;
        top: 100px;
        right: 20px;
        background: ${type === 'success' ? '#50BB27' : type === 'error' ? '#E50B4E' : '#443D96'};
        color: white;
        padding: 16px 24px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        z-index: 10000;
        transform: translateX(100%);
        transition: transform 0.3s ease;
        max-width: 300px;
        font-size: 14px;
        font-weight: 500;
    `;

    document.body.appendChild(notification);

    // Slide in
    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
    }, 100);

    // Auto remove
    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (document.body.contains(notification)) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

function animateAddToCart(button) {
    const originalText = button.textContent;
    button.textContent = 'Added!';
    button.style.background = '#50BB27';

    setTimeout(() => {
        button.textContent = originalText;
        button.style.background = '';
    }, 2000);
}

function simulateGameInstall(button) {
    const originalText = button.textContent;
    let progress = 0;

    const installInterval = setInterval(() => {
        progress += 10;
        button.textContent = `${progress}%`;

        if (progress >= 100) {
            clearInterval(installInterval);
            button.textContent = 'Installed!';
            button.style.background = '#50BB27';
            button.style.color = 'white';

            setTimeout(() => {
                button.textContent = 'Play';
                button.style.background = '#443D96';
            }, 2000);
        }
    }, 200);
}

// Utility Functions
function debounce(func, wait) {
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

function throttle(func, limit) {
    let inThrottle;
    return function () {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

// Performance Optimization
const debouncedScrollHandler = debounce(function () {
    // Handle scroll events
}, 16);

const throttledResizeHandler = throttle(function () {
    // Handle resize events
}, 250);

window.addEventListener('scroll', debouncedScrollHandler);
window.addEventListener('resize', throttledResizeHandler);

// Error Handling
window.addEventListener('error', function (e) {
    console.error('JavaScript Error:', e.error);
    // You could send this to an error tracking service
});

// Service Worker Registration (for PWA capabilities)
if ('serviceWorker' in navigator) {
    window.addEventListener('load', function () {
        navigator.serviceWorker.register('/sw.js')
            .then(function (registration) {
                console.log('SW registered: ', registration);
            })
            .catch(function (registrationError) {
                console.log('SW registration failed: ', registrationError);
            });
    });
}

// Accessibility Enhancements
function initializeAccessibility() {
    // Skip to main content link
    const skipLink = document.createElement('a');
    skipLink.href = '#main';
    skipLink.textContent = 'Skip to main content';
    skipLink.className = 'skip-link';
    skipLink.style.cssText = `
        position: absolute;
        top: -40px;
        left: 6px;
        background: #443D96;
        color: white;
        padding: 8px;
        text-decoration: none;
        border-radius: 4px;
        z-index: 10001;
        transition: top 0.3s;
    `;

    skipLink.addEventListener('focus', function () {
        this.style.top = '6px';
    });

    skipLink.addEventListener('blur', function () {
        this.style.top = '-40px';
    });

    document.body.insertBefore(skipLink, document.body.firstChild);

    // Add main landmark
    const heroSection = document.querySelector('.hero');
    if (heroSection) {
        heroSection.setAttribute('id', 'main');
        heroSection.setAttribute('role', 'main');
    }

    // Improve focus management
    const focusableElements = document.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );

    focusableElements.forEach(element => {
        element.addEventListener('focus', function () {
            this.style.outline = '2px solid #443D96';
            this.style.outlineOffset = '2px';
        });

        element.addEventListener('blur', function () {
            this.style.outline = '';
            this.style.outlineOffset = '';
        });
    });
}

// Initialize accessibility features
document.addEventListener('DOMContentLoaded', initializeAccessibility);

// Analytics and Performance Monitoring
function trackUserInteraction(action, element) {
    // This would integrate with your analytics service
    console.log('User interaction:', action, element);
}

// Add click tracking to important elements
document.addEventListener('click', function (e) {
    if (e.target.matches('.btn, .nav-link, .gallery-item')) {
        trackUserInteraction('click', e.target.className);
    }
});

// Performance monitoring
function measurePerformance() {
    if ('performance' in window) {
        const perfData = performance.getEntriesByType('navigation')[0];
        console.log('Page Load Time:', perfData.loadEventEnd - perfData.fetchStart + 'ms');
    }
}

window.addEventListener('load', measurePerformance);

// Export functions for testing
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        initializeApp,
        showNotification,
        debounce,
        throttle
    };
}

// Responsive handlers
function initializeResponsiveHandlers() {
    // Handle orientation changes
    window.addEventListener('orientationchange', function () {
        setTimeout(function () {
            // Recalculate positions after orientation change
            updateActiveNavLink();

            // Close mobile menu on orientation change
            const mobileMenuToggle = document.getElementById('mobile-menu-toggle');
            const mobileMenu = document.getElementById('mobile-menu');
            const mobileMenuOverlay = document.getElementById('mobile-menu-overlay');

            if (mobileMenuToggle && mobileMenu) {
                mobileMenuToggle.classList.remove('active');
                mobileMenu.classList.remove('active');
                if (mobileMenuOverlay) mobileMenuOverlay.classList.remove('active');
                document.body.classList.remove('menu-open');
                document.body.style.overflow = "";
            }
        }, 100);
    });

    // Handle viewport changes
    let resizeTimeout;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(function () {
            // Recalculate layout after resize
            updateLayoutForScreenSize();
        }, 250);
    });

    // Initial layout calculation
    updateLayoutForScreenSize();
}

// Update layout based on screen size
function updateLayoutForScreenSize() {
    const isMobile = window.innerWidth <= 767;
    const isTablet = window.innerWidth > 767 && window.innerWidth <= 991;
    const isDesktop = window.innerWidth > 991;

    // Add/remove classes for different screen sizes
    document.body.classList.toggle('mobile-view', isMobile);
    document.body.classList.toggle('tablet-view', isTablet);
    document.body.classList.toggle('desktop-view', isDesktop);

    // Adjust hero section for different screen sizes
    const heroSection = document.querySelector('.hero');
    if (heroSection) {
        if (isMobile) {
            heroSection.style.minHeight = '80vh';
        } else if (isTablet) {
            heroSection.style.minHeight = '90vh';
        } else {
            heroSection.style.minHeight = '100vh';
        }
    }

    // Adjust grid layouts
    adjustGridLayouts(isMobile, isTablet, isDesktop);
}

// Adjust grid layouts for different screen sizes
function adjustGridLayouts(isMobile, isTablet, isDesktop) {
    const grids = {
        gallery: document.querySelector('.gallery-grid'),
        services: document.querySelector('.services-grid'),
        events: document.querySelector('.events-grid'),
        trends: document.querySelector('.trends-grid'),
        games: document.querySelector('.games-grid'),
        testimonials: document.querySelector('.testimonials-grid')
    };

    Object.keys(grids).forEach(gridName => {
        const grid = grids[gridName];
        if (!grid) return;

        // Remove existing responsive classes
        grid.classList.remove('mobile-layout', 'tablet-layout', 'desktop-layout');

        // Add appropriate class
        if (isMobile) {
            grid.classList.add('mobile-layout');
        } else if (isTablet) {
            grid.classList.add('tablet-layout');
        } else {
            grid.classList.add('desktop-layout');
        }
    });
}

// Event Slider Functionality
function initializeEventSlider() {
    const sliderContainer = document.getElementById('sliderContainer');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');

    if (!sliderContainer || !prevBtn || !nextBtn) return;

    const cards = Array.from(sliderContainer.querySelectorAll('.card'));
    if (cards.length === 0) return;

    // Check if mobile view
    const isMobile = window.innerWidth <= 767;

    // Desktop positions (horizontal) - 3-1-3 layout
    const desktopPositions = [
        { left: -200, z: 1, scale: 0.85, opacity: 1 },
        { left: -100, z: 2, scale: 0.85, opacity: 1 },
        { left: 0, z: 3, scale: 0.85, opacity: 1 },
        { left: 310, z: 5, scale: 1, opacity: 1 },
        { left: 620, z: 3, scale: 0.85, opacity: 1 },
        { left: 720, z: 2, scale: 0.85, opacity: 1 },
        { left: 820, z: 1, scale: 0.85, opacity: 1 },
    ];

    // Mobile positions (vertical) - showing parts of each card
    const mobilePositions = [
        { top: -80, z: 2, scale: 0.9, opacity: 1 },
        { top: -40, z: 3, scale: 0.9, opacity: 1 },
        { top: 0, z: 5, scale: 1, opacity: 1 },
        { top: 40, z: 3, scale: 0.9, opacity: 1 },
        { top: 80, z: 2, scale: 0.9, opacity: 1 },
        { top: 120, z: 1, scale: 0.9, opacity: 1 },
        { top: 160, z: 1, scale: 0.9, opacity: 1 },
    ];

    let offset = 0;

    function updateCards() {
        const positions = isMobile ? mobilePositions : desktopPositions;

        for (let i = 0; i < cards.length; i++) {
            const pos = positions[i];
            if (!pos) continue;

            const card = cards[(i + offset) % cards.length];

            if (isMobile) {
                // Mobile: vertical positioning
                card.style.top = pos.top + "px";
                card.style.left = "5%";
                card.style.zIndex = pos.z;
                card.style.opacity = pos.opacity;
                card.style.transform = `scale(${pos.scale})`;
            } else {
                // Desktop: horizontal positioning
                card.style.left = pos.left + "px";
                card.style.top = "0";
                card.style.zIndex = pos.z;
                card.style.opacity = pos.opacity;
                card.style.transform = `scale(${pos.scale})`;
            }

            // Remove all position data attributes
            card.removeAttribute('data-position');
        }

        // Add position data attributes for edge cards (desktop only)
        if (!isMobile && cards.length > 0) {
            const leftmostCard = cards[offset % cards.length];
            const rightmostCard = cards[(offset + 6) % cards.length];

            leftmostCard.setAttribute('data-position', 'left');
            rightmostCard.setAttribute('data-position', 'right');
        }
    }

    // Initialize slider
    updateCards();

    // Event listeners for navigation buttons
    prevBtn.addEventListener('click', function () {
        offset = (offset - 1 + cards.length) % cards.length;
        updateCards();
    });

    nextBtn.addEventListener('click', function () {
        offset = (offset + 1) % cards.length;
        updateCards();
    });

    // Keyboard navigation
    document.addEventListener('keydown', function (e) {
        if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') {
            e.preventDefault();
            prevBtn.click();
        } else if (e.key === 'ArrowRight' || e.key === 'ArrowDown') {
            e.preventDefault();
            nextBtn.click();
        }
    });

    // Touch/swipe support for mobile
    let startX = 0;
    let startY = 0;
    let endX = 0;
    let endY = 0;

    sliderContainer.addEventListener('touchstart', function (e) {
        startX = e.touches[0].clientX;
        startY = e.touches[0].clientY;
    });

    sliderContainer.addEventListener('touchend', function (e) {
        endX = e.changedTouches[0].clientX;
        endY = e.changedTouches[0].clientY;

        const diffX = startX - endX;
        const diffY = startY - endY;

        if (Math.abs(diffX) > 50 || Math.abs(diffY) > 50) { // Minimum swipe distance
            if (isMobile) {
                // Mobile: vertical swipes
                if (diffY > 0) {
                    nextBtn.click(); // Swipe up
                } else {
                    prevBtn.click(); // Swipe down
                }
            } else {
                // Desktop: horizontal swipes
                if (diffX > 0) {
                    nextBtn.click(); // Swipe left
                } else {
                    prevBtn.click(); // Swipe right
                }
            }
        }
    });

    // Auto-play functionality (optional)
    let autoPlayInterval;

    function startAutoPlay() {
        autoPlayInterval = setInterval(function () {
            nextBtn.click();
        }, 5000); // Change slide every 5 seconds
    }

    function stopAutoPlay() {
        if (autoPlayInterval) {
            clearInterval(autoPlayInterval);
        }
    }

    // Start auto-play and stop on user interaction
    startAutoPlay();

    sliderContainer.addEventListener('mouseenter', stopAutoPlay);
    sliderContainer.addEventListener('mouseleave', startAutoPlay);

    prevBtn.addEventListener('click', stopAutoPlay);
    nextBtn.addEventListener('click', stopAutoPlay);

    // Handle window resize
    window.addEventListener('resize', function () {
        const newIsMobile = window.innerWidth <= 767;
        if (newIsMobile !== isMobile) {
            // Reinitialize if switching between mobile and desktop
            setTimeout(updateCards, 100);
        }
    });
}

function initializeProductImageSlider() {
    const container = document.querySelector('.product-image-container');
    if (!container) return;

    const images = container.querySelectorAll('.product-image img');
    const leftBtn = container.querySelector('.product-img-arrow-left');
    const rightBtn = container.querySelector('.product-img-arrow-right');
    let current = 0;

    function showImage(idx) {
        images.forEach((img, i) => {
            img.classList.toggle('active', i === idx);
        });
    }

    leftBtn.addEventListener('click', function () {
        current = (current - 1 + images.length) % images.length;
        showImage(current);
    });

    rightBtn.addEventListener('click', function () {
        current = (current + 1) % images.length;
        showImage(current);
    });

    // Show the first image initially
    showImage(current);
}

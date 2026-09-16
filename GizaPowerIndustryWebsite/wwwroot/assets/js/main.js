// GPI Website JavaScript - Interactive Features

// ==================== Mobile Navigation ====================
const mobileToggle = document.getElementById('mobileToggle');
const navMenu = document.getElementById('navMenu');
const navLinks = document.querySelectorAll('.nav-link');

function closeMobileNav() {
    if (mobileToggle) mobileToggle.classList.remove('active');
    if (navMenu) navMenu.classList.remove('active');
    document.body.style.overflow = '';
    document.querySelectorAll('.nav-item-dropdown').forEach((el) => el.classList.remove('active'));
}

function isDesktopNav() {
    return window.matchMedia('(min-width: 1024px)').matches;
}

if (mobileToggle && navMenu) {
    mobileToggle.addEventListener('click', (e) => {
        e.stopPropagation();
        mobileToggle.classList.toggle('active');
        navMenu.classList.toggle('active');
        document.body.style.overflow = navMenu.classList.contains('active') ? 'hidden' : '';
    });
}

// Close mobile menu when clicking a top-level nav link
navLinks.forEach((link) => {
    link.addEventListener('click', () => {
        if (!isDesktopNav()) closeMobileNav();
    });
});

// Mobile / tablet: toggle dropdown sections (Business Lines)
document.querySelectorAll('.nav-item-dropdown').forEach((item) => {
    const trigger = item.querySelector(':scope > a');
    if (!trigger) return;
    trigger.addEventListener('click', (e) => {
        if (isDesktopNav()) return;
        const href = trigger.getAttribute('href');
        if (href === '#' || href === '' || href === null) {
            e.preventDefault();
        }
        const willOpen = !item.classList.contains('active');
        document.querySelectorAll('.nav-item-dropdown').forEach((el) => el.classList.remove('active'));
        if (willOpen) item.classList.add('active');
    });
});

document.querySelectorAll('.dropdown-menu a').forEach((link) => {
    link.addEventListener('click', () => {
        if (!isDesktopNav()) closeMobileNav();
    });
});

// Close mobile menu when clicking outside
document.addEventListener('click', (e) => {
    if (!navMenu || !mobileToggle) return;
    if (navMenu.classList.contains('active') &&
        !navMenu.contains(e.target) &&
        !mobileToggle.contains(e.target)) {
        closeMobileNav();
    }
});

window.addEventListener('resize', () => {
    if (isDesktopNav()) closeMobileNav();
});

// ==================== Sticky Header ====================
const header = document.getElementById('header');
let lastScrollY = window.scrollY;

window.addEventListener('scroll', () => {
    if (!header) return;
    const currentScrollY = window.scrollY;

    if (currentScrollY > 100) {
        header.classList.add('scrolled');
    } else {
        header.classList.remove('scrolled');
    }

    lastScrollY = currentScrollY;
});

// ==================== Active Navigation Link ====================
function updateActiveNav() {
    const currentPath = window.location.pathname;
    const pageName = currentPath.split('/').pop() || 'gpi-index.html';

    navLinks.forEach(link => {
        const linkHref = link.getAttribute('href');
        // Handle root path / or index.html
        if ((pageName === 'gpi-index.html' || pageName === '') && (linkHref === 'gpi-index.html' || linkHref === '/')) {
            link.classList.add('active');
        }
        else if (linkHref === pageName) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });

    // Handle 'Contact Us' button in header if it's a link
    const contactBtn = document.querySelector('.nav-actions .btn-primary');
    if (contactBtn && contactBtn.tagName === 'A') {
        const btnHref = contactBtn.getAttribute('href');
        if (btnHref === pageName) {
            contactBtn.classList.add('active');
        } else {
            contactBtn.classList.remove('active');
        }
    }
}

// Update on load
window.addEventListener('load', updateActiveNav);
// No scroll listener needed for this anymore

// ==================== Smooth Scroll ====================
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        const href = this.getAttribute('href');

        // Skip if it's just "#"
        if (href === '#') {
            e.preventDefault();
            return;
        }

        const target = document.querySelector(href);

        if (target) {
            e.preventDefault();
            const offsetTop = target.offsetTop - 80;

            window.scrollTo({
                top: offsetTop,
                behavior: 'smooth'
            });
        }
    });
});

// ==================== Scroll Animations ====================
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('animate-in');
            observer.unobserve(entry.target);
        }
    });
}, observerOptions);

// Observe elements for animation
const animateElements = document.querySelectorAll(`
    .stat-box, 
    .stat-image, 
    .mission-card, 
    .value-panel, 
    .sustainability-card,
    .partner-logo
`);

animateElements.forEach(el => {
    observer.observe(el);
});

// ==================== Counter Animation for Stats ====================
function animateCounter(element, target, duration = 2000) {
    const start = 0;
    const increment = target / (duration / 16);
    let current = start;

    const timer = setInterval(() => {
        current += increment;
        if (current >= target) {
            element.textContent = target + '+';
            clearInterval(timer);
        } else {
            element.textContent = Math.floor(current) + '+';
        }
    }, 16);
}

// Observe stat boxes and animate counters when visible
const statsObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            const numberElement = entry.target.querySelector('.stat-number');
            if (numberElement && !numberElement.classList.contains('animated')) {
                const targetValue = parseInt(numberElement.textContent);
                numberElement.classList.add('animated');
                animateCounter(numberElement, targetValue);
            }
            statsObserver.unobserve(entry.target);
        }
    });
}, { threshold: 0.5 });

document.querySelectorAll('.stat-box').forEach(stat => {
    statsObserver.observe(stat);
});

// ==================== Contact Form Handling ====================
const contactForm = document.getElementById('contactForm');

if (contactForm) {
    contactForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        // Get form data
        const formData = {
            name: document.getElementById('name').value,
            email: document.getElementById('email').value,
            subject: document.getElementById('subject').value,
            message: document.getElementById('message').value
        };

        // Basic validation
        if (!formData.name || !formData.email || !formData.subject || !formData.message) {
            showNotification('Please fill in all fields', 'error');
            return;
        }

        // Email validation
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(formData.email)) {
            showNotification('Please enter a valid email address', 'error');
            return;
        }

        // Show success message
        showNotification('Thank you for your message! We\'ll get back to you soon.', 'success');

        // Reset form
        contactForm.reset();

        // In production, you would send this data to your server
        console.log('Form data:', formData);
    });
}

// ==================== Notification System ====================
function showNotification(message, type = 'success') {
    // Remove existing notification
    const existingNotification = document.querySelector('.notification');
    if (existingNotification) {
        existingNotification.remove();
    }

    // Create notification
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.style.cssText = `
        position: fixed;
        top: 100px;
        right: 20px;
        background: ${type === 'success' ? '#10b981' : '#ef4444'};
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 8px;
        box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
        z-index: 10000;
        animation: slideInRight 0.3s ease-out;
        max-width: 400px;
        font-weight: 600;
        font-size: 0.9375rem;
    `;

    notification.textContent = message;
    document.body.appendChild(notification);

    // Auto remove after 5 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.3s ease-out';
        setTimeout(() => notification.remove(), 300);
    }, 5000);
}

// Add notification animations to head
const notificationStyle = document.createElement('style');
notificationStyle.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
`;
document.head.appendChild(notificationStyle);

// ==================== Search Functionality ====================
const searchBtn = document.querySelector('.search-btn');

if (searchBtn) {
    searchBtn.addEventListener('click', () => {
        // In production, implement search functionality
        showNotification('Search feature coming soon!', 'success');
    });
}

// ==================== Language Selector ====================
const langBtn = document.querySelector('.lang-btn');

if (langBtn) {
    langBtn.addEventListener('click', () => {
        // In production, implement language switching
        const currentLang = langBtn.textContent;
        const newLang = currentLang === 'En' ? 'Ar' : 'En';
        langBtn.textContent = newLang;

        showNotification(`Language switched to ${newLang === 'En' ? 'English' : 'Arabic'}`, 'success');
    });
}

// ==================== Button Ripple Effect ====================
document.querySelectorAll('.btn, .btn-primary, .btn-hero').forEach(button => {
    button.addEventListener('click', function (e) {
        const ripple = document.createElement('span');
        const rect = this.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = e.clientX - rect.left - size / 2;
        const y = e.clientY - rect.top - size / 2;

        ripple.style.cssText = `
            position: absolute;
            width: ${size}px;
            height: ${size}px;
            border-radius: 50%;
            background: rgba(255, 255, 255, 0.6);
            left: ${x}px;
            top: ${y}px;
            transform: scale(0);
            animation: ripple 0.6s ease-out;
            pointer-events: none;
        `;

        this.style.position = 'relative';
        this.style.overflow = 'hidden';
        this.appendChild(ripple);

        setTimeout(() => ripple.remove(), 600);
    });
});

// Add ripple animation
const rippleStyle = document.createElement('style');
rippleStyle.textContent = `
    @keyframes ripple {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
`;
document.head.appendChild(rippleStyle);

// ==================== Partner Carousel (Simple Auto-scroll) ====================
const partnerLogos = document.querySelector('.partner-logos');

if (partnerLogos) {
    let isHovering = false;

    partnerLogos.addEventListener('mouseenter', () => {
        isHovering = true;
    });

    partnerLogos.addEventListener('mouseleave', () => {
        isHovering = false;
    });

    // Add subtle hover effect to logos
    document.querySelectorAll('.partner-logo').forEach(logo => {
        logo.addEventListener('mouseenter', function () {
            this.style.background = 'linear-gradient(135deg, #f5f5f5 0%, #ffffff 100%)';
        });

        logo.addEventListener('mouseleave', function () {
            this.style.background = '#ffffff';
        });
    });
}

// ==================== Parallax Effect Removed ====================

// ==================== Initialize on Load ====================
window.addEventListener('load', () => {
    console.log('🎉 GPI Website loaded successfully!');

    // Initial active nav check
    // updateActiveNav();

    // Add loaded class to body for any load-specific animations
    document.body.classList.add('loaded');
});

// ==================== Accessibility: Keyboard Navigation ====================
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && navMenu && navMenu.classList.contains('active')) {
        closeMobileNav();
    }
});

// ==================== Performance: Debounce Scroll Events ====================
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

// Apply debounce to scroll-heavy functions
// const debouncedUpdateActiveNav = debounce(updateActiveNav, 100);
// window.removeEventListener('scroll', updateActiveNav);
// window.addEventListener('scroll', debouncedUpdateActiveNav);

// ==================== Custom cursor dot (single follower + contrast via blend) ====================
(function initCursorDot() {
    if (!window.matchMedia('(pointer: fine)').matches) return;

    const dot = document.createElement('div');
    dot.className = 'cursor-dot';
    dot.setAttribute('aria-hidden', 'true');
    document.body.appendChild(dot);

    let x = window.innerWidth / 2;
    let y = window.innerHeight / 2;
    let dotX = x;
    let dotY = y;

    function paint() {
        dotX += (x - dotX) * 0.15;
        dotY += (y - dotY) * 0.15;
        dot.style.transform = `translate3d(${dotX}px, ${dotY}px, 0) translate(-50%, -50%)`;
        requestAnimationFrame(paint);
    }

    window.addEventListener(
        'pointermove',
        (e) => {
            x = e.clientX;
            y = e.clientY;
        },
        { passive: true }
    );
    
    requestAnimationFrame(paint);
})();

// ==================== Typewriter Animation ====================
(function initTypewriter() {
    const heroTitle = document.querySelector('.hero-title');
    if (!heroTitle) return;

    function wrapTextNodes(element) {
        const nodes = Array.from(element.childNodes);
        nodes.forEach(child => {
            if (child.nodeType === Node.TEXT_NODE) {
                const text = child.textContent;
                if (text.trim() === '') return;
                
                const fragment = document.createDocumentFragment();
                for (let i = 0; i < text.length; i++) {
                    const char = text[i];
                    if (char.trim() === '') {
                        fragment.appendChild(document.createTextNode(char));
                    } else {
                        const span = document.createElement('span');
                        span.textContent = char;
                        span.className = 'typewriter-char';
                        span.style.opacity = '0';
                        span.style.display = 'inline-block';
                        fragment.appendChild(span);
                    }
                }
                element.replaceChild(fragment, child);
            } else if (child.nodeType === Node.ELEMENT_NODE) {
                wrapTextNodes(child);
            }
        });
    }

    wrapTextNodes(heroTitle);

    const chars = heroTitle.querySelectorAll('.typewriter-char');
    chars.forEach((char, index) => {
        char.style.animation = `fadeInChar 0.05s forwards`;
        char.style.animationDelay = `${index * 0.05}s`;
    });

    const style = document.createElement('style');
    style.textContent = `
        @keyframes fadeInChar {
            from { opacity: 0; transform: translateY(10px); }
            to { opacity: 1; transform: translateY(0); }
        }
    `;
    document.head.appendChild(style);
})();

// ==================== Scroll to top ====================
(function initScrollToTop() {
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'scroll-to-top';
    btn.setAttribute('aria-label', 'العودة لأعلى الصفحة');
    btn.innerHTML = '<i class="fas fa-chevron-up" aria-hidden="true"></i>';
    document.body.appendChild(btn);

    const threshold = 320;

    function toggleVisible() {
        const show = window.scrollY > threshold;
        btn.classList.toggle('scroll-to-top--visible', show);
    }

    window.addEventListener('scroll', toggleVisible, { passive: true });
    toggleVisible();

    btn.addEventListener('click', () => {
        const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        window.scrollTo({ top: 0, behavior: reduce ? 'auto' : 'smooth' });
    });
})();


/* viewport-motion.js - Progressive fade-in scroll reveals for dashboard panels */

document.addEventListener('DOMContentLoaded', () => {
    const revealElements = document.querySelectorAll('.reveal-on-scroll');

    if (revealElements.length === 0) return;

    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReducedMotion) {
        // Accessibility fallback: bypass all progressive scroll animations
        revealElements.forEach(el => el.classList.add('is-revealed'));
        return;
    }

    // IntersectionObserver to attach trigger class
    const revealObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('is-revealed');
                // Stop observing after rendering
                observer.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.05, // Render when 5% of container height enters screen
        rootMargin: '0px 0px -20px 0px' // Soft offset trigger
    });

    revealElements.forEach(el => revealObserver.observe(el));
});

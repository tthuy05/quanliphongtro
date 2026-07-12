/* counters.js - IntersectionObserver based count-up animation for numeric stats */

document.addEventListener('DOMContentLoaded', () => {
    const counterElements = document.querySelectorAll('[data-counter-target]');

    if (counterElements.length == 0) return;

    // Check for prefers-reduced-motion
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // Animation settings
    const duration = 1200; // ms
    const frameRate = 1000 / 60; // 60fps

    const animateCounter = (element) => {
        const targetString = element.getAttribute('data-counter-target');
        // Parse float value from target string, replacing commas with dots if needed
        const targetValue = parseFloat(targetString.replace(',', '.'));
        const unit = element.getAttribute('data-counter-unit') || '';

        if (isNaN(targetValue) || prefersReducedMotion) {
            // Instantly resolve
            element.textContent = targetString + unit;
            return;
        }

        const isDecimal = targetString.includes(',') || targetString.includes('.');
        const decimalPlaces = isDecimal ? (targetString.split(/[.,]/)[1] || '').length : 0;

        let startValue = 0;
        const totalFrames = Math.round(duration / frameRate);
        let currentFrame = 0;

        // Quadratic ease-out interpolation
        const easeOutQuad = (t) => t * (2 - t);

        const counterTick = () => {
            currentFrame++;
            const progress = currentFrame / totalFrames;
            const easedProgress = easeOutQuad(progress);
            const currentValue = startValue + easedProgress * (targetValue - startValue);

            // Format string output based on locale formatting (commas vs dots)
            let formattedValue = currentValue.toFixed(decimalPlaces);
            if (targetString.includes(',')) {
                formattedValue = formattedValue.replace('.', ',');
            }

            element.textContent = formattedValue + unit;

            if (currentFrame < totalFrames) {
                requestAnimationFrame(counterTick);
            } else {
                // Ensure absolute match at finish
                element.textContent = targetString + unit;
            }
        };

        counterTick();
    };

    // IntersectionObserver to trigger count animation once when scrolled in view
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateCounter(entry.target);
                // Stop observing after firing once
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    counterElements.forEach(el => observer.observe(el));
});

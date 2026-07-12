/* hero-motion.js - Cinematic animation & pointer parallax controller for hero image */

document.addEventListener('DOMContentLoaded', () => {
    const imageContainer = document.getElementById('hero-interactive-image-container');
    const landscapeImage = document.getElementById('hero-landscape-image');

    if (!imageContainer || !landscapeImage) return;

    // Check for prefers-reduced-motion
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReducedMotion) {
        // Reduced motion override: disable parallax and looping animations
        landscapeImage.classList.remove('hero-zoom-active');
        landscapeImage.style.transform = 'none';
        return;
    }

    // Check if the current device is a touch/mobile device
    const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;

    if (!isTouchDevice) {
        // Set up desktop mouse-pointer parallax
        let targetX = 0;
        let targetY = 0;
        let currentX = 0;
        let currentY = 0;
        const interpolationFactor = 0.08; // Smooth inertia

        const onMouseMove = (event) => {
            const rect = imageContainer.getBoundingClientRect();
            // Get mouse position relative to container center
            const relX = event.clientX - rect.left - rect.width / 2;
            const relY = event.clientY - rect.top - rect.height / 2;

            // Map offsets to subtle maximum movement (-10px to +10px)
            targetX = (relX / (rect.width / 2)) * 10;
            targetY = (relY / (rect.height / 2)) * 8;
        };

        // Attach mousemove with passive binding
        imageContainer.addEventListener('mousemove', onMouseMove, { passive: true });

        // Parallax update tick loop
        let animationFrameId;
        const updateTick = () => {
            // Apply linear interpolation (lerp) for smooth motion
            currentX += (targetX - currentX) * interpolationFactor;
            currentY += (targetY - currentY) * interpolationFactor;

            // Apply base scale 1.04 (to prevent image edge reveal) + coordinate offsets
            landscapeImage.style.transform = `scale(1.04) translate3d(${currentX}px, ${currentY}px, 0)`;

            animationFrameId = requestAnimationFrame(updateTick);
        };

        // Trigger loop
        updateTick();

        // Pause tracking when out of visible viewport
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    if (!animationFrameId) updateTick();
                } else {
                    cancelAnimationFrame(animationFrameId);
                    animationFrameId = null;
                }
            });
        }, { threshold: 0.1 });

        observer.observe(imageContainer);

        // Cleanup events on page unload
        window.addEventListener('unload', () => {
            imageContainer.removeEventListener('mousemove', onMouseMove);
            if (animationFrameId) cancelAnimationFrame(animationFrameId);
        });
    }
});

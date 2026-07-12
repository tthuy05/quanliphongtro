/* dropdowns.js - Dropdown focus restoration and click helpers */

document.addEventListener('DOMContentLoaded', () => {
    // Return focus to trigger button when dropdown closes via ESC key
    const dropdownContainers = document.querySelectorAll('[x-data*="open"]');
    
    dropdownContainers.forEach(container => {
        const trigger = container.querySelector('button');
        if (!trigger) return;

        // Custom listener for checking closure
        container.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' || e.key === 'Esc') {
                // If it closes, return focus to trigger button
                setTimeout(() => {
                    trigger.focus();
                }, 50);
            }
        });
    });
});

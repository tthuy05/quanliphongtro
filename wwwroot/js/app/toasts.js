/* toasts.js - Global in-app toast notification dispatch helper */

window.showToast = (message, type = 'info') => {
    // Dispatch custom window event
    const event = new CustomEvent('show-toast', {
        detail: {
            message: message,
            type: type
        }
    });
    window.dispatchEvent(event);
};

// Expose a quick confirmation notification when standard buttons are pressed
document.addEventListener('DOMContentLoaded', () => {
    // Hook details buttons
    const actionableElements = document.querySelectorAll('a[href="#"], button:not([class*="state"])');
    actionableElements.forEach(el => {
        // Exclude specific modal action controls
        if (el.getAttribute('data-counter-target') || el.closest('[x-data*="open"]')) return;
        
        el.addEventListener('click', (e) => {
            const label = el.textContent.trim() || 'Thao tác';
            // Show dynamic notice toast
            window.showToast(`Bạn đã kích hoạt hành động: "${label}" (Đang ở trạng thái Demo)`, 'info');
        });
    });
});

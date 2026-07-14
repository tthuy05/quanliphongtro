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

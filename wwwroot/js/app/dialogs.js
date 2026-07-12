/* dialogs.js - Global confirm modal helper and keyboard focus trapping */

window.openConfirmModal = (title, message, type = 'info') => {
    const event = new CustomEvent('open-confirm-modal', {
        detail: {
            title: title,
            message: message,
            type: type
        }
    });
    window.dispatchEvent(event);
};

document.addEventListener('DOMContentLoaded', () => {
    // Focus Trap helper for dialog modals
    const manageFocusTrap = (modalContainer, openFlagSelector) => {
        const focusableElements = 'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])';
        
        modalContainer.addEventListener('keydown', function(e) {
            const isTabPressed = e.key === 'Tab' || e.keyCode === 9;
            if (!isTabPressed) return;

            // Resolve list of active elements inside modal
            const focusableContent = modalContainer.querySelectorAll(focusableElements);
            const firstFocusableElement = focusableContent[0];
            const lastFocusableElement = focusableContent[focusableContent.length - 1];

            if (e.shiftKey) { // Back tab
                if (document.activeElement === firstFocusableElement) {
                    lastFocusableElement.focus();
                    e.preventDefault();
                }
            } else { // Forward tab
                if (document.activeElement === lastFocusableElement) {
                    firstFocusableElement.focus();
                    e.preventDefault();
                }
            }
        });
    };

    // Initialize traps for search and confirm modals
    const searchModal = document.querySelector('[x-trap\\.noscroll\\.inert*="searchOpen"]');
    if (searchModal) manageFocusTrap(searchModal, 'searchOpen');

    const confirmModal = document.querySelector('[x-trap\\.noscroll\\.inert*="open"]');
    if (confirmModal) manageFocusTrap(confirmModal, 'open');
});

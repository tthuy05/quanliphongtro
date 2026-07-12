/* navigation.js - Navigation shortcuts, focus, and scroll controllers */

document.addEventListener('DOMContentLoaded', () => {
    // Keyboard listener for Search Shortcut (Ctrl + K or Cmd + K)
    window.addEventListener('keydown', (event) => {
        if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
            event.preventDefault();
            // Access Alpine.js root state directly to toggle search Open flag
            const bodyData = document.body.__x;
            if (bodyData) {
                // If using alpine v3, we can toggle using expression evaluations or DOM assignments
                const alpineState = document.querySelector('[x-data]');
                if (alpineState && alpineState.__x && alpineState.__x.$data) {
                    alpineState.__x.$data.searchOpen = !alpineState.__x.$data.searchOpen;
                }
            } else {
                // Direct lookup fallback if Alpine isn't fully compiled yet
                const alpineState = document.querySelector('[x-data]');
                if (alpineState && alpineState._x_dataStack) {
                    alpineState._x_dataStack[0].searchOpen = !alpineState._x_dataStack[0].searchOpen;
                }
            }
        }
    });

    // Check for focus transitions and lock body scroll when mobile sidebar drawer or overlays are open
    const targetBody = document.body;
    
    // Add custom helper class to lock body elements
    const lockScroll = () => {
        targetBody.classList.add('overflow-hidden', 'touch-none');
    };
    
    const unlockScroll = () => {
        targetBody.classList.remove('overflow-hidden', 'touch-none');
    };

    // Watch for Alpine backdrop changes to toggles
    const alpineEl = document.querySelector('[x-data]');
    if (alpineEl && window.Alpine) {
        window.Alpine.effect(() => {
            const data = window.Alpine.store('sidebarOpen');
            // If sidebar overlay is open, lock scroll
        });
    }
});

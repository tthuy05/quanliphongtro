# Responsive, Accessibility, and Performance Spec - Trọ Sinh Viên (Internal technical name: TroHub)

This document contains instructions for layouts across different viewports, keyboard access patterns, and render optimizations.

---

## 1. Responsive Layout Strategy

We design responsively across five major breakpoints:

| Viewport | Breakpoint | Spacing | Sidebar State | Layout Adaptations |
| :--- | :--- | :--- | :--- | :--- |
| **Desktop** | `1440px`+ | Spacing `xl` (24px) | Full Expanded (260px) | 2-column hero, 5-col stats, 2-col charts, 3-col tables. |
| **Laptop Wide**| `1280px` | Spacing `lg` (16px) | Full Expanded (260px) | Balanced layout, standard grid scaling. |
| **Laptop** | `1024px` | Spacing `lg` (16px) | Collapsed Icons-only | Sidebar collapses to 72px. Hover tooltips enabled. |
| **Tablet** | `768px` | Spacing `md` (12px) | Off-canvas Drawer | Sidebar hidden. Top bar toggles drawer. Charts stack. |
| **Mobile** | `390px` | Spacing `sm` (8px) | Off-canvas Drawer | Single-column stack. Tables convert to visual cards. |

### Mobile-Specific Adaptations
*   **Touch Targets:** All buttons, input items, and menu links have a minimum clickable area of `44px x 44px`.
*   **Table Transformation:** Tables feature `overflow-x-auto` scrolling. Alternatively, on screens `<640px`, rows display as independent stackable cards containing visual name-value pairs to prevent horizontal page scrolling.
*   **Hero Section:** The landscape image shifts to the bottom of the hero text on mobile, with its height restricted to `180px` and the focal point centered on the reflection of the lake.

---

## 2. Accessibility Specification (WCAG 2.1 AA)

*   **Keyboard Navigation:** All interactive elements (dropdowns, toggle buttons, quick action grid icons) are reachable using the `Tab` key.
*   **Focus Ring Indicator:** Focusable elements display a clear, high-contrast outline (`outline-2 outline-indigo-600 outline-offset-2`) when focused via keyboard.
*   **Escape Key Behavior:** Pressing the `Escape` key closes the mobile navigation drawer, dropdown menus, property selection panel, search drawer, and modal popups.
*   **Focus Trapping:** When the Confirmation Dialog is open, focus is trapped inside the modal, preventing users from tabbing to background elements. Closing the modal restores focus to the initiating button.
*   **Color Contrast:** Text-to-background contrast maintains a minimum ratio of `4.5:1` (and `3.0:1` for large headings). Color is never used as the single indicator of status (e.g., status badges contain text labels, and chart segments use distinct patterns or labels).
*   **Screen Readers:** SVGs feature `aria-hidden="true"`, and buttons utilize descriptive `aria-label` tags (e.g., `aria-label="Đóng bảng thông báo"`).

---

## 3. Performance Optimization

*   **Cumulative Layout Shift (CLS) Prevention:**
    *   Images feature explicit `width` and `height` properties.
    *   Charts reserve their canvas aspect ratios (`aspect-ratio: 2/1` or `aspect-ratio: 1`) using placeholder containers.
*   **Asset Deferral:** All scripts are loaded using the `defer` attribute. Alpine.js initializes after the DOM is parsed.
*   **Frame Optimizations:** Scroll and mousemove listeners for parallax effects use passive binding (`{ passive: true }`) to ensure smooth scrolling.
*   **Animation Viewport Pauses:** The welcome hero's landscape image pan/zoom animation is paused using an `IntersectionObserver` when the hero is scrolled out of the visible screen.

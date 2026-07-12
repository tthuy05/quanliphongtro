# Motion and Animation Specification - Trọ Sinh Viên (Internal technical name: TroHub)

This specification defines the animation rules, triggers, durations, easings, performance requirements, and accessibility fallback states for Trọ Sinh Viên.

---

## 1. Core Guidelines
*   **Performance First:** Animations must primarily change CSS `transform` and `opacity` to avoid triggering layout paints. Do not animate `width`, `height`, `margin`, `top`, or `left` properties during loops.
*   **Restrained Playback:** Limit continuous looping animations. Looping states must be slow and atmospheric.
*   **Viewport Awareness:** Continuous loop animations must pause when they scroll out of the visible viewport.
*   **Accessibility First:** Honor the `prefers-reduced-motion` CSS media query by instantly bypassing or simplifying animations to static opacity reveals.

---

## 2. Animation Registry

| Component / Trigger | Target Element | Property | Duration | Easing | Looping Behavior / Fallback |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Hero Image Reveal** | Image wrapper | `opacity`, `scale` | `1000ms` | `cubic-bezier(0.16, 1, 0.3, 1)` | On load only. Reduced motion: instant display. |
| **Hero Image Zoom** | Landscape Image | `transform: scale` | `24s` | `linear` | Auto-reversing continuous loop. Pauses offscreen. Reduced motion: Disabled. |
| **Pointer Parallax** | Image overlays | `transform: translate3d` | Immediate | Responsive | Follows cursor movement. Disabled on mobile/touch and under reduced motion. |
| **Ambient Light** | Radial gradients | `opacity`, `transform` | `12s` | `ease-in-out` | Continuous looping pulse. Reduced motion: static glow. |
| **Water Shimmer** | Reflection overlay mask | `background-position` | `8s` | `linear` | Continuous horizontal offset looping. Reduced motion: Disabled. |
| **Section Reveal** | Container panels | `opacity`, `translateY` | `500ms` | `cubic-bezier(0.25, 0.46, 0.45, 0.94)` | Triggers when element enters viewport. Reduced motion: instant opacity reveal. |
| **Stats Counters** | Numeric text strings | Count-up JS | `1200ms` | `easeOutQuad` | Iterates integer values once. Reduced motion: static values. |
| **Card Hover** | Widget card boxes | `transform`, `box-shadow` | `200ms` | `ease-out` | Translates `y` up by `-4px`, deepens shadow. |
| **Button Press** | CTA Buttons | `transform: scale` | `100ms` | `ease-in-out` | Scales down to `0.97` on press (`active:` state). |
| **Sidebar Active Slide**| Left border/pill | `opacity`, `transform` | `250ms` | `ease-in-out` | Smooth transitions when changing menu selection. |
| **Dropdown Menus** | Dialog wrapper | `opacity`, `scale` | `150ms` | `cubic-bezier(0, 0, 0.2, 1)` | Scale from `0.95` to `1.0` during fade-in. |
| **Modal Overlays** | Centered wrapper | `opacity`, `scale` | `300ms` | `cubic-bezier(0.34, 1.56, 0.64, 1)` | Elastic zoom-in on reveal. Backdrop blurs. |
| **Toasts** | Screen edge alerts | `opacity`, `translateX` | `300ms` | `cubic-bezier(0.16, 1, 0.3, 1)` | Slides in from right margin, slides out on close. |
| **Notification Pulse**| Unread dot pill | `box-shadow` pulse | `2s` | `ease-in-out` | Looping scale pulse when notifications are unread. |
| **Skeleton Shimmer** | Grey placeholders | `background-position` | `1.5s` | `linear` | Infinite diagonal gradient shimmer loops. |

---

## 3. Implementation Guidelines

### 3.1. IntersectionObserver for Reveal Animations
Section entry animations use an `IntersectionObserver` instance to dynamically attach the class `.is-revealed`. Static configurations ensure CSS handles the actual interpolation:
```css
.reveal-on-scroll {
    opacity: 0;
    transform: translateY(12px);
    transition: opacity 600ms cubic-bezier(0.16, 1, 0.3, 1), 
                transform 600ms cubic-bezier(0.16, 1, 0.3, 1);
}
.reveal-on-scroll.is-revealed {
    opacity: 1;
    transform: translateY(0);
}
@media (prefers-reduced-motion: reduce) {
    .reveal-on-scroll {
        opacity: 1;
        transform: none;
        transition: none;
    }
}
```

### 3.2. Mouse Parallax Scripting
Parallax uses lightweight, passive event tracking. The coordinate offsets translate `transform: translate3d(x, y, 0)` on sub-layers within the hero. Values are capped at ±15px to prevent nausea.
```javascript
// Example implementation approach
requestAnimationFrame(() => {
    target.style.transform = `translate3d(${offsetX}px, ${offsetY}px, 0)`;
});
```
This is fully disabled if `window.matchMedia('(prefers-reduced-motion: reduce)').matches` is active.

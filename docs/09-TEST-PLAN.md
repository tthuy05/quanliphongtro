# Test Plan - Trọ Sinh Viên (Internal technical name: TroHub)

This plan details verification protocols, tools, manual test flows, and acceptance criteria for validating the landlord dashboard interface.

---

## 1. Automated & Build Verifications

### Test Build Success
*   **Command:** `dotnet build`
*   **Acceptance Criteria:** Compilation terminates with `0 Warning(s)` and `0 Error(s)`.

### Local Development Server Initialization
*   **Command:** `dotnet run`
*   **Acceptance Criteria:** Target application boots on `http://localhost:5000` (or configured port) without database connectivity exceptions.

---

## 2. Layout & Visual Quality QA

### Breakpoint Renders (Desktop to Mobile)
*   **Widths:** `1440px`, `1280px`, `1024px`, `768px`, `390px`.
*   **Checkpoints:**
    *   No horizontal scrollbars exist on any screen size.
    *   Padding shifts dynamically (`xl` to `sm`) to avoid cramped interfaces.
    *   No text overlaps or clips out of boundaries.

### Sidebar Collapse & Expand
*   **Desktop:** Sidebar collapses from 260px to 72px upon toggle.
*   **Acceptance Criteria:** Text labels disappear during collapse; icons remain centered; tooltips display on hover; content panels expand dynamically without layout jumps.

### Mobile Navigation Drawer
*   **Breakpoint:** `<1024px`.
*   **Verification:** Click hamburger icon to open menu.
*   **Acceptance Criteria:** Sidebar transitions smoothly from left edge; background scroll lock locks body; click outside drawer or click close button dismisses menu; focus traps inside navigation; ESC key closes drawer.

---

## 3. Dynamic UI & Interactions

### Statistic Counter Animation
*   **Check:** Scroll dashboard into view.
*   **Acceptance Criteria:** Number counters animate from 0 to target value (e.g. `24`) inside `1200ms`. Count triggers once and doesn't loop.

### Charts Rendering & Interactive Tooltips
*   **Check:** Verify Line and Donut charts.
*   **Acceptance Criteria:** Segment outlines render smoothly; hovering over items triggers custom tooltip offsets; chart resizes dynamically when resizing browser viewport.

### Top Bar Panels (Property Selector, Search, Notifications, Profile)
*   **Check:** Click each dropdown button.
*   **Acceptance Criteria:** Dropdown container translates and fades in; clicking outside the container closes the popup; keyboard navigation (`Tab` and `Enter`) opens and closes elements.

### Empty and Loading State Toggles
*   **Check:** Toggle testing filters.
*   **Acceptance Criteria:** Skeletons and Empty illustrations appear inside tables, preserving spacing, showing descriptive text, and loading normal mock collections on reset.

---

## 4. Accessibility & Performance Auditing

### Keyboard Nav & Focus Ring Visuals
*   **Check:** Navigate using `Tab` key.
*   **Acceptance Criteria:** Every link, input, and button is reachable; a visible outline ring appears on focus; focus order is logical (top-to-bottom, left-to-right).

### Reduced Motion Query Matching
*   **Check:** Turn on "Reduce Motion" inside OS accessibility panel or simulate via DevTools.
*   **Acceptance Criteria:** Landscape image slow zooms disable instantly; counts render static numbers; card slide reveals load immediately without transitions.

### Console Diagnostics
*   **Check:** Open Browser Developer Console (F12).
*   **Acceptance Criteria:** No red errors, warning logs, or failed asset loads (404s).

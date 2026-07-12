# UI/UX Design System - Trọ Sinh Viên (Internal technical name: TroHub)

This design system defines colors, typography, layout systems, UI controls, and visual states.

---

## 1. Brand Personality & Principles
*   **Premium SaaS Aesthetics:** Refined layout ratios, spacious typography, and subtle shadows.
*   **Trustworthy & Professional:** High contrast ratios, strict color systems, and clear borders.
*   **Smooth Motion & Micro-interactions:** Fluid hover transitions, non-disruptive fade-ins, and animated loading/success states.
*   **Vietnamese Contextualization:** Optimized layout sizing for Vietnamese word lengths and localized date/currency rules.

---

## 2. Typography & Fonts

*   **Primary Font Family:** `Be Vietnam Pro`, `Inter`, system-ui, `-apple-system`, `sans-serif`.
*   **Typography Scale:**

| Token | Class | Size | Line Height | Weight | Usage |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `display-1` | `text-4xl` | `2.25rem (36px)` | `2.75rem` | Bold | Large Dashboard Hero Header |
| `h1` | `text-2xl` | `1.5rem (24px)` | `2.0rem` | SemiBold | Main Sections |
| `h2` | `text-xl` | `1.25rem (20px)` | `1.75rem` | SemiBold | Card Headers, Panels |
| `h3` | `text-lg` | `1.125rem (18px)`| `1.5rem` | Medium | In-Card Sub-headers |
| `body` | `text-base` | `1.0rem (16px)` | `1.5rem` | Regular | Primary table entries, descriptions |
| `body-sm` | `text-sm` | `0.875rem (14px)`| `1.25rem` | Regular | Secondary descriptions, timestamps |
| `caption` | `text-xs` | `0.75rem (12px)` | `1.0rem` | Medium | Tooltips, badge headers, labels |

---

## 3. Color Tokens (Semantic & HSL System)

We define our color tokens based on a tailwind-compatible palette:

```
Slate (Neutral Scale)
├─ 50  : #f8fafc (App background)
├─ 100 : #f1f5f9 (Borders, divider lines)
├─ 200 : #e2e8f0 (Grid borders, inactive icons)
├─ 500 : #64748b (Secondary text, inactive sidebar state)
├─ 800 : #1e293b (Primary headings, text colors)
└─ 900 : #0f172a (Sidebar background default)

Theme Semantic Accents
├─ Primary: Deep Indigo (indigo-600 #4f46e5 / indigo-700 #4338ca)
├─ Success: Emerald Green (emerald-500 #10b981 / emerald-600 #059669)
├─ Warning: Amber Orange (amber-500 #f59e0b / amber-600 #d97706)
├─ Danger: Rose Red (rose-500 #f43f5e / rose-600 #e11d48)
└─ Info: Sky Blue (sky-500 #0ea5e9 / sky-600 #0284c7)
```

---

## 4. Spacing, Borders, and Shadows

### Spacing Scale
Uses a 4px (0.25rem) base grid:
*   `xs`: `0.25rem (4px)` | `sm`: `0.5rem (8px)` | `md`: `0.75rem (12px)` | `lg`: `1.0rem (16px)`
*   `xl`: `1.5rem (24px)` | `2xl`: `2.0rem (32px)` | `3xl`: `3.0rem (48px)`

### Border Radius Scale
*   `base-radius`: `0.5rem (8px)` (Standard buttons, inputs)
*   `card-radius`: `0.75rem (12px)` (All card wrapper containers)
*   `hero-radius`: `1.0rem (16px)` (Main banner welcome block)
*   `full-radius`: `9999px` (Status badges, circular avatars)

### Shadow Scale
*   `shadow-sm`: `0 1px 2px 0 rgba(0,0,0,0.05)` (Subtle inputs, borders)
*   `shadow-md`: `0 4px 6px -1px rgba(0,0,0,0.07), 0 2px 4px -1px rgba(0,0,0,0.04)` (Dashboard cards)
*   `shadow-lg`: `0 10px 15px -3px rgba(0,0,0,0.1), 0 4px 6px -2px rgba(0,0,0,0.05)` (Dropdowns, notifications)
*   `shadow-xl`: `0 20px 25px -5px rgba(0,0,0,0.15)` (Modals, notification drawer panels)

---

## 5. UI Components Spec

### Buttons
*   **Primary:** Solid indigo background, white text. Lift on hover, transform scale down on press.
*   **Secondary:** White background, thin slate border, dark text. Subtle highlight on hover.
*   **Danger:** Solid rose red background, white text. Used for overdue actions or cancellations.

### Inputs & Selects
*   White background, slate-200 border, rounded base. Focus displays an indigo border rings with a 3px soft blue glow shadow.

### Cards
*   White background, Slate-100 border, shadow-md. On mouse-hover, cards lift slightly (`translate-y-[-2px]`) with smooth transitions.

### Status Badges
*   **Đã thanh toán (Paid):** Soft emerald green background (`bg-emerald-50`), dark emerald text (`text-emerald-700`).
*   **Chờ thanh toán (Pending):** Soft amber background (`bg-amber-50`), dark amber text (`text-amber-700`).
*   **Thanh toán một phần (Partial):** Soft sky background (`bg-sky-50`), dark sky text (`text-sky-700`).
*   **Quá hạn (Overdue):** Soft rose background (`bg-rose-50`), dark rose text (`text-rose-700`).

---

## 6. Feedback Elements

### Skeleton Loaders
*   Pulsing grey placeholders (`animate-pulse`) simulating visual layout sizes to eliminate layout shifts.

### Toasts
*   Floating containers at bottom-right viewport. Fade-in on mount and slide out on dismiss.

### Modals
*   Centered modal overlays with backdrop blur and scaling entrance animation.

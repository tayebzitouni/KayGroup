---
name: KAY ONE Enterprise
colors:
  surface: '#f6f9ff'
  surface-dim: '#d4dbe2'
  surface-bright: '#f6f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#eef4fc'
  surface-container: '#e8eef6'
  surface-container-high: '#e3e9f1'
  surface-container-highest: '#dde3eb'
  on-surface: '#161c22'
  on-surface-variant: '#44474d'
  inverse-surface: '#2b3137'
  inverse-on-surface: '#ebf1f9'
  outline: '#75777e'
  outline-variant: '#c5c6cd'
  surface-tint: '#515f78'
  primary: '#000000'
  on-primary: '#ffffff'
  primary-container: '#0d1c32'
  on-primary-container: '#76849f'
  inverse-primary: '#b9c7e4'
  secondary: '#0051d5'
  on-secondary: '#ffffff'
  secondary-container: '#316bf3'
  on-secondary-container: '#fefcff'
  tertiary: '#000000'
  on-tertiary: '#ffffff'
  tertiary-container: '#00201d'
  on-tertiary-container: '#0c9488'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d6e3ff'
  primary-fixed-dim: '#b9c7e4'
  on-primary-fixed: '#0d1c32'
  on-primary-fixed-variant: '#39475f'
  secondary-fixed: '#dbe1ff'
  secondary-fixed-dim: '#b4c5ff'
  on-secondary-fixed: '#00174b'
  on-secondary-fixed-variant: '#003ea8'
  tertiary-fixed: '#89f5e7'
  tertiary-fixed-dim: '#6bd8cb'
  on-tertiary-fixed: '#00201d'
  on-tertiary-fixed-variant: '#005049'
  background: '#f6f9ff'
  on-background: '#161c22'
  surface-variant: '#dde3eb'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 30px
    fontWeight: '700'
    lineHeight: 38px
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Inter
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
    letterSpacing: -0.01em
  title-sm:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '600'
    lineHeight: 24px
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  body-sm:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 18px
  label-caps:
    fontFamily: Inter
    fontSize: 11px
    fontWeight: '700'
    lineHeight: 16px
    letterSpacing: 0.05em
  data-mono:
    fontFamily: JetBrains Mono
    fontSize: 13px
    fontWeight: '500'
    lineHeight: 18px
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  unit: 4px
  container-padding: 24px
  gutter: 16px
  component-gap-tight: 8px
  sidebar-width: 260px
---

## Brand & Style
The design system for this product is engineered for high-stakes financial operations, prioritizing precision, efficiency, and authoritative clarity. The aesthetic is **Modern Corporate**, blending the structural rigor of enterprise software with the refined finish of a premium financial institution. 

The UI is intentionally lean to accommodate high data density without inducing cognitive overload. It avoids all decorative elements, relying instead on mathematical alignment, disciplined spacing, and a strict color-coded hierarchy to guide the user through complex workflows such as "Ventes" (Sales) and "Achats" (Purchasing). The atmosphere is professional, stable, and focused.

## Colors
The palette is architectural, using depth and contrast to separate navigation from the workspace.
- **Primary (Dark Navy):** Reserved for the global sidebar and primary headers to provide a strong structural frame.
- **Secondary (Professional Blue):** Used for primary actions, active states, and focus indicators.
- **Tertiary (Teal):** Applied to positive financial growth indicators and success states.
- **Accents (Orange/Red):** Strictly functional for "Attention" and "Critical" financial KPIs or errors.
- **Borders & Dividers:** Use Light Gray (#E2E8F0) to define table rows and card boundaries without creating visual noise.

## Typography
The system uses **Inter** for its exceptional legibility in dense interfaces. A strict hierarchy ensures that financial figures are immediately scannable.
- **Data Display:** For tabular data and financial values, use **JetBrains Mono** at small scales to ensure numbers align perfectly in columns (tabular lining).
- **Hierarchy:** Use `label-caps` for section headers like "MODES DE PAIEMENT" or column headers in tables.
- **Scale:** Keep body text at 14px for standard interaction and 13px for dense data grids to maximize information density on desktop screens.

## Layout & Spacing
The layout follows a **Fixed-Fluid hybrid model**:
- **Sidebar:** A fixed 260px navigation bar on the left (Dark Navy).
- **Canvas:** A fluid main content area that expands to fill the viewport, utilizing a 12-column grid for dashboard widgets.
- **Density:** We utilize a "Compact" spacing model. Vertical padding in table rows is capped at 8px to allow more rows above the fold.
- **Margins:** A standard 24px margin exists around the main content canvas to ensure the UI feels premium and breathable despite the data density.

## Elevation & Depth
This system uses **Tonal Layering** rather than heavy shadows to indicate depth.
- **Level 0 (Canvas):** The white background (#FFFFFF).
- **Level 1 (Cards/Widgets):** Defined by a 1px solid border (#E2E8F0). No shadow.
- **Level 2 (Dropdowns/Modals):** A subtle 1px border with a very soft, tight shadow (0px 4px 6px rgba(0,0,0,0.05)) to lift the element off the canvas.
- **Active States:** Use a 2px left-border accent in Professional Blue for active navigation items in the sidebar.

## Shapes
In line with the serious enterprise nature of the tool, shapes are predominantly geometric and sharp. 
- **Base Radius:** 4px for buttons, input fields, and small cards. This provides a "softened-brutalist" look that feels modern yet sturdy.
- **Large Components:** Modals and large dashboard containers use a 6px radius.
- **Interactive Elements:** Use sharp 90-degree corners for table row selections to maintain the grid-like integrity of data views.

## Components
- **Buttons:** 
  - *Primary:* Solid #2563EB with white text. 4px radius. 
  - *Secondary:* Ghost style with #E2E8F0 border and #0A192F text.
- **Data Tables (Tableaux):** Header row uses a light gray background (#F8FAFC) with uppercase bold labels. Row hover state is a subtle #F1F5F9.
- **Status Chips:** Small, condensed pills with low-opacity backgrounds (e.g., Error uses 10% opacity Red fill with 100% opacity Red text).
- **Input Fields:** 1px border (#E2E8F0). On focus, the border changes to Professional Blue with a 1px inner glow.
- **KPI Cards:** Large numeric value in `display-lg`, with a small trend indicator (Teal for up, Red for down) placed in the top right corner.
- **Sidebar Nav:** Items use #64748B text color, shifting to #FFFFFF with a #2563EB left-border accent when active.
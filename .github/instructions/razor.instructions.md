---
applyTo: "**/*.cshtml"
---

# Razor Views & HTMX Rules

## Interactivity & HTMX (⚠️ CRITICAL)

- **Default to HTMX**: Use HTMX attributes (`hx-get`, `hx-post`, `hx-target`, `hx-swap`) for interactivity instead of writing custom AJAX/Vanilla JS, unless HTMX cannot solve the problem.
- **No Inline Scripts**: Avoid writing inline `<script>` tags inside Partial Views. Scope scripts appropriately or use HTMX events (`htmx:afterOnLoad`, etc.).

## Components & Reusability

- **Partial Views**: When returning from a controller for an HTMX request, return `PartialView("_MyComponent")` instead of a full `View()`.
- **ViewComponents**: For complex, reusable UI blocks that require backend logic (e.g., fetching data from a database), use ViewComponents (`@await Component.InvokeAsync(...)`) instead of standard PartialViews.

## Styling Policy

- **No Inline Styles**: Avoid inline `<style>` tags or `style="..."` attributes.
- **CSS Classes**: Always use predefined SCSS/CSS classes that follow the Modified BEM convention.

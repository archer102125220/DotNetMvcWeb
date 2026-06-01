---
description: Inline Styles Rules for Razor Views
globs: ["**/*.cshtml"]
alwaysApply: true
---

# Inline Styles Rules (Razor / MVC)

## Forbidden Inline Styles

❌ **Static values** - Use CSS classes instead
```html
<!-- ❌ WRONG -->
<div style="padding: 20px; margin-bottom: 16px;"></div>

<!-- ✅ CORRECT -->
<div class="my-element"></div>
```

❌ **Dynamic calculations** - Use CSS variable passing or calculate in ViewModel
```html
<!-- ❌ WRONG -->
<div style="height: @(Model.ContainerHeight)px;"></div>

<!-- ✅ CORRECT -->
<div style="--container_height: @(Model.ContainerHeight)px;"></div>
```

❌ **Conditional styles** - Use conditional CSS classes
```html
<!-- ❌ WRONG -->
<div style="color: @(Model.IsActive ? "red" : "gray");"></div>

<!-- ✅ CORRECT -->
<div class="@(Model.IsActive ? "is-active" : "is-inactive")"></div>
```

## Allowed Inline Styles
✅ **Third-party script requirements** (e.g., GTM, hidden iframes)
✅ **Passing dynamic backend values strictly as CSS Variables**

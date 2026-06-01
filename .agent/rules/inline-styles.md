# Inline Styles Policy for Razor

## Forbidden Inline Styles

❌ **Static values** — Use CSS classes instead:
```html
<!-- ❌ WRONG -->
<div style="padding: 20px; margin-bottom: 16px;"></div>

<!-- ✅ CORRECT -->
<div class="my-element"></div>
```

❌ **Dynamic calculations** — Pass as CSS Variables instead:
```html
<!-- ❌ WRONG -->
<div style="height: @(Model.ContainerHeight)px;"></div>

<!-- ✅ CORRECT -->
<div style="--container_height: @(Model.ContainerHeight)px;"></div>
```

❌ **Conditional styles** — Use conditional CSS classes:
```html
<!-- ❌ WRONG -->
<div style="color: @(Model.IsActive ? "red" : "gray");"></div>

<!-- ✅ CORRECT -->
<div class="my-box" data-is-active="@(Model.IsActive ? "true" : null)"></div>
```

## Allowed Inline Styles
✅ **Third-party script requirements** (e.g., GTM, hidden iframes)
✅ **Passing dynamic backend values strictly as CSS Variables**: `style="--container_height: @(Model.Height)px;"`

## Scoped CSS (Preferred Approach)
- Use Razor CSS Isolation: create `MyView.cshtml.css` next to `MyView.cshtml`.
- ASP.NET Core automatically bundles and scopes these styles so they only apply to that view.

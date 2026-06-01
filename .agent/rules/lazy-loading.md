# Lazy Loading with HTMX

To optimize performance and defer the loading of heavy components, use **HTMX's load trigger**.

## How to Lazy Load a ViewComponent or PartialView

Instead of rendering a heavy ViewComponent synchronously on page load, render an empty placeholder that fetches the content immediately after the page loads.

### Example:
```html
<!-- Placeholder container that fires a GET request on load -->
<div hx-get="/Components/HeavyChart"
     hx-trigger="load"
     hx-swap="innerHTML">
    <!-- Optional: Loading spinner -->
    <div class="spinner">Loading chart...</div>
</div>
```

### Controller Implementation:
```csharp
[HttpGet("/Components/HeavyChart")]
public IActionResult GetHeavyChart()
{
    // Return a PartialView or invoke a ViewComponent
    return ViewComponent("HeavyChart");
}
```

## When to use Lazy Loading
- Heavy database queries that block the initial page render.
- Third-party widget integrations.
- Below-the-fold content that the user doesn't see immediately.

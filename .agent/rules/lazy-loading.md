# HTMX Lazy Loading UI

## 1. Improving Perceived Performance
- For heavy components (e.g., large graphs, complex queries), do not block the initial page load.
- Use HTMX lazy loading to fetch the component asynchronously after the page renders.

## 2. Implementation
- In the initial Razor view, render a skeleton or spinner inside a `div` with `hx-get` and `hx-trigger="load"`.
  ```html
  <div hx-get="/Widgets/HeavyChart" hx-trigger="load" hx-swap="outerHTML">
      <!-- Loading Skeleton/Spinner -->
      <div class="spinner">Loading chart...</div>
  </div>
  ```
- The `HeavyChart` action method then does the expensive work and returns a `PartialView` containing the actual chart.

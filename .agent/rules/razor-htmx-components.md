# ViewComponents vs PartialViews (HTMX)

## 1. Partial Views (`@await Html.PartialAsync`)
- **When to use**: For simple fragments of HTML that do not require independent data fetching. You must pass the required Model to it from the parent view.
- **HTMX**: HTMX responses from Controllers generally return `PartialView("_MyFragment", model)` so that the client only receives the HTML snippet needed to update the DOM, not the full layout.

## 2. ViewComponents (`@await Component.InvokeAsync`)
- **When to use**: For complex UI widgets that need to run their own business logic or fetch their own data from the database (e.g., a Shopping Cart dropdown, a recent news widget).
- **Benefit**: Keeps your main Controller clean, because the parent Controller doesn't have to populate the ViewComponent's data.

## 3. HTMX + ViewComponents
- To return a ViewComponent from a Controller for an HTMX request, use:
  ```csharp
  return ViewComponent("ShoppingCart", new { userId = 123 });
  ```

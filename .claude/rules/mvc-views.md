# Razor Views & HTMX Best Practices

This project uses **ASP.NET Core MVC** with **Razor Views (`.cshtml`)** and **HTMX** for frontend interactivity.

## Core Principle: Server-Rendered UI

1. **Avoid SPAs**: Do not build Single Page Application structures. Rely on the server to render HTML.
2. **HTMX over Vanilla JS**: Use HTMX attributes (`hx-get`, `hx-post`, `hx-target`, `hx-swap`) for interactivity, lazy loading, and form submissions. Write custom JavaScript only when HTMX cannot solve the problem (e.g., third-party charting libraries, complex DOM manipulations).

## HTMX Integration

### Controller Actions for HTMX
- HTMX requests send an `HX-Request: true` HTTP header.
- You can detect this in a controller action to decide whether to return a full view or a partial view:
  ```csharp
  if (Request.Headers.ContainsKey("HX-Request"))
  {
      return PartialView("_MyComponent", model);
  }
  return View("MyPage", model);
  ```

### HTMX Attributes
- `hx-get="/Controller/Action"`: Issue GET request.
- `hx-post="/Controller/Action"`: Issue POST request.
- `hx-target="#element-id"`: Where to inject the resulting HTML.
- `hx-swap="outerHTML" | "innerHTML"`: How to swap the content.

## Razor Views (`.cshtml`)

### Structure
- Keep logic in Razor views to an absolute minimum. Business logic belongs in the Controller or a Service.
- Use `ViewModel` classes to strongly type your views (`@model MyNamespace.ViewModels.MyViewModel`).
- Use `<Nullable>enable</Nullable>` patterns. If a model property is nullable, check for nulls using `@if (Model.Item != null)` or `@Model.Item?.Name` before accessing it.

### ViewComponents vs PartialViews
- **PartialViews (`Html.PartialAsync`)**: Use for simple reusable HTML chunks that don't require complex backend data fetching.
- **ViewComponents**: Use for complex UI elements (like a dynamic navigation bar, shopping cart summary) that require database queries or complex logic independent of the parent page's Controller.
  - Usage: `@await Component.InvokeAsync("ShoppingCart")`

### Styling
- **CSS Isolation**: You can use Razor CSS isolation (e.g., `Index.cshtml.css`). These styles will be automatically scoped to the component.
- **BEM Convention**: Follow the BEM-like convention (Block, Element with `-`, Segment with `_`) if writing global CSS in `wwwroot/css/`.

### Internationalization (i18n)
- Inject `IViewLocalizer` in views:
  ```html
  @using Microsoft.AspNetCore.Mvc.Localization
  @inject IViewLocalizer Localizer

  <h1>@Localizer["WelcomeMessage"]</h1>
  ```

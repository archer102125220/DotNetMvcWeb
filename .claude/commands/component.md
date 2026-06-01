# Generate Component

Generate a new Razor ViewComponent or PartialView following ASP.NET MVC project standards.

## Usage

Use this command when you need to:
- Create new Razor UI components
- Generate full Razor Views
- Build reusable HTMX-driven partials

## Template

Please generate a component:

**Component Name**: [ComponentName]

**Component Type**:
- [ ] Partial View (`_Partial.cshtml` - for simple HTML chunks)
- [ ] ViewComponent (requires C# class + `Default.cshtml` - for complex logic)
- [ ] Full Page View (`Index.cshtml` with layout)

**Location**:
- [ ] `Views/Shared/` (reusable partial view)
- [ ] `Views/Shared/Components/[Name]/` (reusable view component)
- [ ] `Views/[Controller]/` (controller-specific view)

**Features**:
- [ ] C# ViewModel (strongly typed)
- [ ] HTMX Attributes (for interactivity)
- [ ] Razor CSS Isolation (`.cshtml.css`)
- [ ] Internationalization (i18n via `IViewLocalizer`)

**ViewModel Properties** (if applicable):
```csharp
public class MyViewModel {
  // Define properties here
}
```

**Requirements**:
- ✅ Use strongly typed models (`@model`)
- ✅ Use `IViewLocalizer` for translations
- ✅ Nullable reference checks (`@Model?.Property`)
- ✅ Follow MVC directory conventions

## Example

```
Please generate a component:

**Component Name**: UserCard

**Component Type**:
- [x] ViewComponent (complex logic required)

**Location**:
- [x] Views/Shared/Components/UserCard/

**Features**:
- [x] C# ViewModel (user data)
- [x] HTMX Attributes (onClick behavior)
- [x] Razor CSS Isolation
- [x] Internationalization (i18n)

**ViewModel Properties**:
```csharp
public class UserCardViewModel {
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
}
```
```

## Component Templates

### Partial View
```html
@model MyProject.ViewModels.UserCardViewModel
@using Microsoft.AspNetCore.Mvc.Localization
@inject IViewLocalizer Localizer

<div class="user-card" css-is-active="@Model.IsActive.ToString().ToLower()">
    <h4>@Model.Name</h4>
    <button hx-post="/Users/Activate/@Model.Id" hx-swap="outerHTML" hx-target="closest .user-card">
        @Localizer["Activate"]
    </button>
</div>
```

### ViewComponent C# Class
```csharp
using Microsoft.AspNetCore.Mvc;

namespace MyProject.ViewComponents
{
    public class UserCardViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {
            // Fetch data
            var model = new UserCardViewModel { /* ... */ };
            return View(model);
        }
    }
}
```

## Related Skills
- [File Organization](../rules/file-organization.md)
- [Razor Views & HTMX](../rules/mvc-views.md)
- [Internationalization](../rules/i18n.md)

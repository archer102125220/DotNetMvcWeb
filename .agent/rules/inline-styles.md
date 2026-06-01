# Inline Styles Policy for Razor

## 1. Avoid Inline Styles
- **Rule**: Do not use the `style="..."` attribute in HTML elements. It prevents caching, pollutes the DOM, and violates Content Security Policy (CSP) best practices.

## 2. Use Scoped CSS
- **Rule**: Use Razor CSS Isolation. Create a file named `MyView.cshtml.css` next to `MyView.cshtml`. ASP.NET Core will automatically bundle and scope these styles to only apply to `MyView.cshtml`.

## 3. Exceptions
- **Rule**: Inline styles are ONLY permitted for highly dynamic values that cannot be moved to CSS (e.g., dynamically setting a progress bar width: `style="width: @Model.Progress%"`) or dynamically calculating layout coordinates in C#.

---
name: mvc-localization
description: Guide for implementing i18n using IStringLocalizer and IViewLocalizer.
---

# mvc-localization

When implementing multi-language support (i18n), DO NOT use third-party libraries like `next-intl`. Use native .NET Localization.

## Setup
Ensure `builder.Services.AddLocalization()` and the Request Localization middleware is configured in `Program.cs`.

## Using Localization
1. **Controllers/Services**: Inject `IStringLocalizer<SharedResource> localizer`.
2. **Razor Views**:
   ```html
   @using Microsoft.AspNetCore.Mvc.Localization
   @inject IViewLocalizer Localizer
   <p>@Localizer["Text"]</p>
   ```
3. **Resource Files**: Store translations in `.resx` files (e.g., `Resources/SharedResource.zh-tw.resx`). Use Visual Studio or XML editors to modify these safely.

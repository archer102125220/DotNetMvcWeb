---
name: mvc-file-organization
description: Guidelines for where to place files in the MVC architecture.
---

# mvc-file-organization

When creating new files, follow the standard ASP.NET Core MVC structure.

- **Controllers/**: For classes handling HTTP requests. Must end in `Controller`.
- **Models/Entities/**: EF Core database models.
- **Models/ViewModels/**: Strongly typed models backing Razor Views.
- **Models/DTOs/**: Data transfer objects for APIs.
- **Views/[ControllerName]/**: Razor views (`.cshtml`).
- **Views/Shared/**: Layouts (`_Layout.cshtml`), Partial views, and ViewComponent views.
- **ViewComponents/**: C# classes for ViewComponents.
- **Services/**: Business logic and dependency injection services.
- **Data/**: The EF Core `DbContext` and configuration files.
- **wwwroot/**: Static assets like CSS, JS, HTMX library, and images.

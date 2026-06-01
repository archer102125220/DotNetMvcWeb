# MVC Architecture & Structure

## 1. Controllers
- Keep Controllers thin. They should handle HTTP request routing, binding, returning Views, and returning status codes.
- Business logic belongs in Services or Domain classes.

## 2. Models
- **ViewModels**: Group properties needed for a specific view into a ViewModel class (e.g. `LoginViewModel`). Place in `Models/ViewModels`.
- **Entities**: Represents database tables. Place in `Models/Entities`.
- **DTOs**: Used for data transfer, often for JSON API responses. Place in `Models/DTOs`.

## 3. Views
- Organized by Controller name (e.g., `Views/Home/Index.cshtml`).
- Shared views and layout go in `Views/Shared/`.

## 4. ViewComponents
- Place ViewComponent C# classes in a `ViewComponents/` directory at the root.
- Place their Razor views in `Views/Shared/Components/[ComponentName]/Default.cshtml`.

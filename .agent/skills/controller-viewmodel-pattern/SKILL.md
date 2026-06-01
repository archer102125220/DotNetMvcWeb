---
name: controller-viewmodel-pattern
description: Rules for enforcing the Controller-ViewModel pattern in MVC.
---

# controller-viewmodel-pattern

## The Problem
Passing Entity models (e.g., EF Core models like `User` or `Order`) directly to Razor Views often exposes sensitive data (like password hashes) or causes lazy-loading issues.

## The Rule
1. **Always use ViewModels**: Controllers must map Data Entities to ViewModels (e.g., `UserViewModel`) before passing them to `return View(model)`.
2. **Thin Controllers**: Controllers should only:
   - Validate ModelState.
   - Call a Service to get/save data.
   - Map Data to ViewModel.
   - Return the View.
3. **No Business Logic in Views**: Razor `.cshtml` files should only contain `if/else` and `foreach` for presentation logic. Complex calculations belong in the Service or ViewModel.

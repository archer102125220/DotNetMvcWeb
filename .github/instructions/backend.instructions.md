---
applyTo: 
  - "**/*Controller.cs"
  - "**/*Service.cs"
  - "**/*Context.cs"
  - "**/Models/**/*.cs"
---

# Backend & ORM Rules

## Entity Framework Core (EF Core) Best Practices

- **Async First**: ALWAYS use async/await methods for database operations (`ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`). Synchronous DB calls are strictly forbidden.
- **No Tracking**: For read-only queries where you do not need to update the entities, always append `.AsNoTracking()` to improve performance.
- **Dependency Injection**: Always resolve your `DbContext` via constructor dependency injection. Never instantiate it manually with `new AppDbContext()`.

## Database Schema Modifications (⚠️ CRITICAL)

**Before ANY database schema change** (e.g., running migrations, modifying EF models), you MUST:

1. **Ask the human developer**: "Is this project deployed to production?"
2. **Wait for their answer** and act accordingly:
   - **Not deployed**: You may drop the last unapplied migration, modify the existing migration, or delete and recreate the DB.
   - **Deployed**: NEVER modify existing executed migrations. You must always create NEW migration files (`dotnet ef migrations add AddNewColumn`).

## MVC Conventions

- **Controllers**: MVC View controllers must inherit from `Controller`. API controllers should inherit from `ControllerBase`. Controller class names must end with `Controller`.
- **Views mapping**: Razor views (`.cshtml`) must align with Controller names (e.g., `Views/Home/Index.cshtml`).
- **Dependencies**: Use Dependency Injection for all services. Do not tightly couple code by manually instantiating service classes.

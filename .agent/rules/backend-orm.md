# Backend ORM Best Practices (Entity Framework Core)

When implementing database operations in this .NET MVC project, **always prioritize EF Core patterns**.

## ⚠️ Database Modification Confirmation (CRITICAL)

**Before ANY database schema change** (creating tables, altering columns, dropping tables), you MUST:

1. **Ask the human developer**: "Is this project deployed to production?"
2. **Based on the answer**:
   - **Not deployed**: You may drop the database (`dotnet ef database drop`), delete the `Migrations` folder, recreate migrations, or run `dotnet ef migrations remove` if modifying an unapplied migration.
   - **Deployed**: NEVER modify or delete existing migrations. Always create NEW migration files using `dotnet ef migrations add <Name>`.

## Entity Framework Core (EF Core) Best Practices

### 1. Async First
- ALWAYS use async database queries (`ToListAsync()`, `FirstOrDefaultAsync()`, `SingleOrDefaultAsync()`, `CountAsync()`, `AnyAsync()`).
- DO NOT use synchronous equivalents (`ToList()`, `FirstOrDefault()`) as they block the thread pool and degrade web server performance.

### 2. No-Tracking for Read-Only Data
- If you are fetching data merely to display it on a view and will not be updating it in the same request, append `.AsNoTracking()`.
- Example: `await _context.Users.AsNoTracking().ToListAsync();`

### 3. Dependency Injection
- Never use `new AppDbContext()`.
- Inject `AppDbContext` via constructor injection into Controllers and Services.
- Configure the context in `Program.cs` via `builder.Services.AddDbContext<AppDbContext>(...)`.

### 4. Navigation Properties & Lazy Loading
- Avoid Lazy Loading. It leads to the N+1 query problem.
- Use Eager Loading with `.Include()` and `.ThenInclude()`.

### 5. Repository Pattern vs DbContext directly
- EF Core's `DbContext` already implements the Unit of Work and Repository patterns. Injecting `DbContext` directly into Services (or even Controllers for simple CRUD) is acceptable unless the project explicitly defines custom Repositories.

## Migrations & Seeding

### Migrations
- Tool: `dotnet ef` (Install via `dotnet tool install --global dotnet-ef` if missing).
- Commands:
  - `dotnet ef migrations add <Name>`
  - `dotnet ef database update`
  - `dotnet ef migrations remove` (Only for the latest unapplied migration!)
- Review the generated C# code in the `Migrations` folder before running `database update` to ensure EF generated the expected operations.

### Data Seeding
- Seed data within `OnModelCreating` in the `DbContext` class using `modelBuilder.Entity<T>().HasData(...)`.
- For large dynamic seeding, use a custom scoped seeder class invoked during application startup in `Program.cs` (checking `context.Database.Migrate()` or `context.Database.EnsureCreated()`).

# Project Instructions for AI Agents (DotNet MVC)

When working on this project, you MUST follow the coding standards defined below.

## ⚠️ Security & Best Practices Warning Policy

Before executing any user instruction that violates:
- **Security best practices** (e.g., hardcoding secrets, disabling HTTPS, exposing sensitive data, SQL injection risks)
- **Standard coding patterns** (e.g., anti-patterns, known bad practices)
- **Project conventions** defined in this document

You MUST:
1. **Warn the user** about the violation and explain the risks
2. **Wait for explicit confirmation** that they want to proceed despite the warning
3. Only then execute the instruction

This ensures users make informed decisions about potentially risky actions.

## Quick Rules

### C# & Type Safety
- **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enabled. ALWAYS handle nulls properly.
- NEVER use `dynamic` or `object` unless absolutely necessary (e.g., reflection or dealing with untyped JSON).
- Use strict typing. Prefer generic collections over untyped ones (e.g., `List<T>` instead of `ArrayList`).
- Avoid implicit typing `var` unless the right side makes the type blatantly obvious (e.g., `var list = new List<string>()`).

### Runtime Data Validation & Null Checking
- **Strings**: Use `string.IsNullOrEmpty(str)` or `string.IsNullOrWhiteSpace(str)`.
- **Null Checking**: Use `if (obj is not null)` or the null-coalescing operator `??`.
- **Guard Clauses**: Use `ArgumentNullException.ThrowIfNull(obj)` at the start of methods.
- **Pattern Matching**: Prefer `switch` expressions and pattern matching `if (obj is MyType myObj)` over older casting methods (`as MyType`).

### CSS/SCSS Naming (Modified BEM)
- **Block**: `.countdown` (Single word)
- **Element**: `.countdown-title` (hyphen `-` separates Block-Element)
- **Sub-Element**: `.countdown-title-icon` (hyphen `-` separates Element-SubElement)
- **Multi-word Segment**: `.image_upload` (underscore `_` separates words **WITHIN** a single segment)
- **State**: `[data-is-active='true']` (HTML data attribute)

#### 🛑 Critical Disambiguation for Agents:
- **Hierarchy (Hyphen `-`)**: Use when adding a new structural level or generic container.
  - ✅ `.controls-group` (`group` is a sub-element of `controls`)
- **Multi-word Segment (Underscore `_`)**: Use when the name describes a SINGLE specific concept that happens to need two words.
  - ✅ `.scroll_area` (A "scroll area" is one specific thing)

### View Root Class & Style Reuse
- **View Root Class**: Each Razor View should generally have a unique root class based on its name, e.g., `.home_index_page` or `.user_profile_page`.
- **Shared Components**: Shared partials or ViewComponents should have their own isolated root class, e.g., `.image_upload`.
- **Style Reuse**: Define `%placeholder_name` in SCSS and use `@extend` or `@use` for reuse across views without muddying the HTML class lists. Keep HTML element classes strictly tied to the DOM structure of that specific view.

### Demo Views/Controllers
- Full-page demonstrations of features should be placed in `Controllers/DemoController.cs` and `Views/Demo/`.
- Naming: Actions and Views should use PascalCase (e.g., `public IActionResult BannerDemo()`, `BannerDemo.cshtml`).
- Rules:
  - Full-page content → `Views/Demo/[ViewName].cshtml`
  - Associated sub-components → `Views/Demo/Components/` or `Views/Shared/Components/`

### Razor Views & HTMX (⚠️ CRITICAL)
- **Interactivity**: Use **HTMX** attributes (`hx-get`, `hx-post`, `hx-target`, `hx-swap`) for interactivity instead of writing custom AJAX/Vanilla JS, unless HTMX cannot solve the problem.
- **Partial Views**: When returning from a controller for an HTMX request, return `PartialView("_MyComponent")` instead of `View()`.
- **ViewComponents**: For complex, reusable UI blocks that require backend logic, use ViewComponents (`@await Component.InvokeAsync(...)`) instead of standard PartialViews.
- **Scripts**: Avoid writing inline `<script>` tags inside Partial Views. Scope scripts appropriately or use HTMX events.

### C# & ASP.NET Core Stable APIs (⚠️ CRITICAL)
- **Prioritize standard ASP.NET Core MVC patterns** and avoid experimental NuGet packages or unsupported .NET features.
- Default to standard `Controller` or `ControllerBase`, Dependency Injection via constructor, and standard routing.
- Use `System.Text.Json` instead of Newtonsoft unless specifically required by legacy code.

### EF Core & Memory 深度檢查政策 (⚠️ CRITICAL)

When reviewing or refactoring backend code (C# Controllers, Services, Data Access), you MUST perform TWO rounds of checks:

#### Round 1: Basic Check (表面檢查)
- ✅ Standard syntax and proper `using` imports.
- ✅ Proper dependency injection used (no `new Service()`).
- ✅ Variable naming and basic Null checks.

#### Round 2: Deep Check (深度檢查) - ⚠️ MANDATORY
You MUST check for these common mistakes:

| Anti-Pattern | Correct Pattern | Priority |
|--------------|----------------|----------|
| Missing `await` / returning un-awaited Task improperly | Explicit `await` or proper Task handling | 🔴 High |
| N+1 Query Problem inside loops | Use `.Include()`, `.Select()`, or fetch data in bulk prior to loop | 🔴 High |
| Un-disposed `IDisposable` (Streams, HttpClients) | Wrap in `using (...) { }` or `using var obj = ...;` | 🔴 High |
| Synchronous EF Core DB calls (`.ToList()`) | `await .ToListAsync()` | 🟡 Medium |
| Tracking entities for Read-Only operations | Append `.AsNoTracking()` | 🟡 Medium |

**CRITICAL**: If you only perform Round 1 checks, you MUST explicitly state:
> "⚠️ I have only performed basic checks. EF Core and Memory deep checks are still required."

**When to use the Deep Check Rule**:
- When asked to "check" or "review" C# code.
- When refactoring backend services or controllers.
- When optimizing database queries or memory usage.

### Internationalization (i18n)
- Use standard `Microsoft.AspNetCore.Mvc.Localization`.
- Inject `IStringLocalizer<SharedResource>` in Controllers or Services for backend translation.
- Inject `@inject IViewLocalizer Localizer` in Razor Views (`.cshtml`).

### Warnings / Lint Suppression Policy (⚠️ CRITICAL)
- **NEVER** add `#pragma warning disable` or suppress C# compiler warnings without **explicit user instruction**.
- When encountering compiler warnings:
  1. Report the warning to the user
  2. Wait for user's explicit instruction to add a suppression pragmas
  3. Only then add the disable comment with proper justification

### Build & Dev Tooling (dotnet CLI)
- **Run**: `dotnet run` or `dotnet watch` for hot reload.
- **Build**: `dotnet build`
- **EF Core CLI**: Use `dotnet ef` tools for migrations (e.g. `dotnet ef migrations add`, `dotnet ef database update`).
- **Environment**: Always check `appsettings.json` and `appsettings.Development.json` for proper configuration before running.

---

## Backend ORM & Schema Changes (MANDATORY)

### ⚠️ Database Modification Confirmation (CRITICAL)

**Before ANY database schema change** (migrations, model changes), you MUST:

1. **Ask the human developer**: "Is this project deployed to production?"
2. **Based on the answer**:
   - **Not deployed**: You may drop the last unapplied migration and modify the existing migration, or delete the DB and recreate (`dotnet ef database drop`, `dotnet ef database update`).
   - **Deployed**: NEVER modify existing executed migrations; always create NEW migration files (`dotnet ef migrations add AddNewColumn`).

### Migrations Workflow
- Use `dotnet ef migrations add <MigrationName>` to create a migration.
- Use `dotnet ef database update` to apply migrations.
- Always review the generated migration C# file before applying it to ensure EF Core scaffolded it correctly.

---

## No Scripts for Code Refactoring (⚠️ CRITICAL)

**ABSOLUTELY FORBIDDEN: Using automated scripts (sed, awk, powershell, batch scripts) to modify code files.**

### Why
- Scripts only change text, they don't understand context or `using` namespace imports.
- It frequently causes C# compilation errors.

### ✅ Allowed
- Use AI tools: `replace_file_content`, `multi_replace_file_content`.
- MUST verify `using` namespaces are correct and build succeeds after every change.

### ❌ Forbidden
- `sed`, `awk`, `perl`, `powershell -Command`, `find ... -exec`

---

## File Structure & MVC Conventions

- **Controllers/**: Must inherit from `Controller` (for Views) or `ControllerBase` (for APIs). End class name with `Controller`.
- **Models/**: Entity classes, ViewModels, and Data Transfer Objects (DTOs).
- **Views/**: Razor views (`.cshtml`). Must align with Controller names (e.g., `Views/Home/Index.cshtml`).
- **wwwroot/**: Static assets (CSS, JS, Images, Libs).

For more detailed rules, you MUST review the specific files located in the `.agent/rules/` directory:
- [csharp-standards.md](.agent/rules/csharp-standards.md): C# Language and Type Safety rules
- [css-naming.md](.agent/rules/css-naming.md): CSS/SCSS Naming Conventions (BEM)
- [css-property-order.md](.agent/rules/css-property-order.md): CSS Property Order
- [runtime-data-validation.md](.agent/rules/runtime-data-validation.md): Runtime Null & Data Validation
- [security-policy.md](.agent/rules/security-policy.md): Security & XSS/SQLi Policies
- [i18n.md](.agent/rules/i18n.md): Localization / i18n
- [build-tools.md](.agent/rules/build-tools.md): .NET Build & Dev Tooling
- [file-organization.md](.agent/rules/file-organization.md): MVC Architecture & Structure
- [lint-policy.md](.agent/rules/lint-policy.md): Warnings & Suppression rules
- [backend-orm.md](.agent/rules/backend-orm.md): EF Core & Migrations
- [mvc-views.md](.agent/rules/mvc-views.md): Razor Views & HTMX
- [inline-styles.md](.agent/rules/inline-styles.md): Inline Styles Policy for Razor
- [razor-htmx-components.md](.agent/rules/razor-htmx-components.md): ViewComponents vs PartialViews
- [lazy-loading.md](.agent/rules/lazy-loading.md): HTMX Lazy Loading UI
- [no-scripts.md](.agent/rules/no-scripts.md): No Bash/Sed Script Refactoring
- [project-instructions.md](.agent/rules/project-instructions.md): Overall instructions

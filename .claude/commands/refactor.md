# Refactor Code

Refactor existing C# or Razor code to improve quality, maintainability, and follow DotNet MVC project standards.

## Usage

Use this command when you need to:
- Improve code structure without changing functionality.
- Apply project C# coding standards (e.g. pattern matching, LINQ).
- Optimize performance (e.g. `AsNoTracking`, Async/Await).
- Remove code smells and extract services from fat Controllers.

## Template

Please refactor the following code:

**Target**: [Specify file, class, or method]

**Goals**:
- [ ] Improve readability
- [ ] Apply C# Best Practices (Pattern matching, proper Nullable handling)
- [ ] Optimize Entity Framework Core queries
- [ ] Extract Business Logic to Services
- [ ] Add proper guard clauses (Fail fast)

**Constraints**:
- ✅ Maintain existing functionality.
- ✅ Keep backward compatibility.
- ✅ Follow project coding standards (No empty catches, no `dynamic`).
- ❌ Do NOT use automated bash scripts (`sed`, `awk`) for C# refactoring.
- ❌ Do NOT add `#pragma warning disable` without permission.

**Context**:
[Provide any additional context about the code]

## Example

```
Please refactor the following code:

**Target**: Controllers/OrderController.cs (CreateOrder method)

**Goals**:
- [x] Extract Business Logic to OrderService
- [x] Apply proper async/await patterns
- [x] Use Guard Clauses for null checks

**Constraints**:
- Keep existing HTTP endpoints and DTOs intact.
- Ensure EF Core transaction is maintained.
```

## Related Skills
- [No Scripts for Refactoring](../rules/no-scripts.md)
- [C# Standards](../rules/csharp-standards.md)
- [Runtime Data Validation](../rules/runtime-data-validation.md)

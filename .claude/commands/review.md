# Code Review

Perform comprehensive C# and ASP.NET Core MVC code review following project standards.

## Usage

Use this command when you need to:
- Review C# pull requests
- Check code quality & performance
- Identify security vulnerabilities (XSS, SQLi)
- Ensure coding standards compliance

## Template

Please review the following code:

**Target**: [Specify file(s), PR, or commit]

**Review Focus**:
- [ ] Code quality and readability
- [ ] C# Strong Typing (avoid `dynamic`, proper `var` usage)
- [ ] Nullable Reference Types handling
- [ ] Performance (EF Core `.AsNoTracking()`, Async/Await)
- [ ] Security vulnerabilities (SQLi, XSS, CSRF)
- [ ] Coding standards compliance
- [ ] Exception handling (no empty catches)
- [ ] HTMX / Razor View integration

**Project Standards Checklist**:
- [ ] EF Core: Async queries (`ToListAsync`), `.AsNoTracking()` for read-only.
- [ ] C#: Explicit types where `var` is ambiguous.
- [ ] Security: `[ValidateAntiForgeryToken]` on POSTs.
- [ ] Views: ViewModel usage instead of domain entities.
- [ ] Lint: No `#pragma warning disable` without justification.
- [ ] HTMX: Returning `PartialView()` appropriately.

**Output Format**:
- List issues by severity (Critical, High, Medium, Low)
- Provide specific line numbers
- Suggest concrete C# improvements
- Include code examples for fixes

## Example

```
Please review the following code:

**Target**: Controllers/UserController.cs

**Review Focus**:
- [x] Nullable Reference Types handling
- [x] EF Core Performance
- [x] Security (Over-posting, CSRF)
- [x] Exception handling

**Specific Concerns**:
- Check if DB query is optimized and async.
- Ensure the user password is not accidentally returned in JSON or Views.
```

## Review Output Format

```markdown
## Code Review Summary

### Critical Issues (Must Fix)
1. **[Line X]** Issue description
   - **Problem**: Sync DB call `ToList()` blocking threads.
   - **Fix**: Use `await ...ToListAsync()`.

### High Priority
2. **[Line Y]** Issue description
   - **Problem**: Missing `[ValidateAntiForgeryToken]`.
   - **Fix**: Add attribute to POST method.

### Medium Priority
3. **[Line Z]** Issue description
   - **Suggestion**: Use `string.IsNullOrEmpty` instead of `== ""`.

### Positive Observations
- Well-structured Dependency Injection.
- Proper ViewModel separation.
```

## Common Review Points

### C# & Types
- ❌ Using `dynamic` or unnecessary `object`.
- ❌ Implicit `var` when type isn't obvious.
- ✅ Proper pattern matching (`is not null`).
- ✅ Nullable reference type handling (`?` and `!`).

### EF Core & Database
- ❌ `.ToList()` instead of `.ToListAsync()`.
- ❌ Missing `.AsNoTracking()` for read-only operations.
- ❌ N+1 queries (missing `.Include()`).

### ASP.NET MVC & Security
- ❌ Binding Domain Entities directly to Views (over-posting).
- ❌ Empty `catch` blocks.
- ❌ Manual SQL string concatenation.
- ✅ Use of ViewModels.
- ✅ Parameterized LINQ queries.

## Related Skills
- [Security Policy](../rules/security-policy.md)
- [C# Standards](../rules/csharp-standards.md)
- [Backend ORM](../rules/backend-orm.md)
- [Lint Policy](../rules/lint-policy.md)

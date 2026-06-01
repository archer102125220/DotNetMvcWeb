# Runtime Null & Data Validation

Always validate data correctly at runtime, especially when dealing with data crossing the boundary from client (HTMX/Forms) to server (Controllers).

### Strings
- **Rule**: Use `string.IsNullOrEmpty(str)` or `string.IsNullOrWhiteSpace(str)` instead of checking `str == null || str == ""`.

### Collections
- **Rule**: When checking if an `IEnumerable<T>` has elements, use `.Any()` instead of `.Count() > 0`. `.Count()` may iterate the entire collection depending on the underlying type.

### Objects and Nulls
- **Rule**: Use the null-conditional operator `?.` and null-coalescing operator `??` to provide defaults and avoid `NullReferenceException`.
  - Example: `var name = user?.Profile?.Name ?? "Unknown";`

### Model State Validation
- **Rule**: In POST/PUT Controller actions, ALWAYS check `if (!ModelState.IsValid)` before proceeding. If invalid, return the view so validation errors are displayed.
  - With HTMX, return the PartialView containing the form so the client gets the validation feedback without a full page reload.

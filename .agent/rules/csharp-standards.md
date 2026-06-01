# C# Language and Type Safety Rules

## 1. Nullable Reference Types
- The project runs with `<Nullable>enable</Nullable>`.
- **Rule**: Always handle nulls appropriately. Avoid using `!` (null-forgiving operator) unless you are absolutely certain the value cannot be null and the compiler simply cannot infer it.
- **Rule**: Use `ArgumentNullException.ThrowIfNull(param)` at the start of methods to guard against null arguments.

## 2. Strong Typing
- **Rule**: NEVER use `dynamic` or `object` when a specific type can be used.
- **Rule**: When using `var`, it should only be used when the type is blatantly obvious from the right side of the assignment (e.g. `var user = new User();` or `var list = new List<string>();`). If it's the result of a method call where the type isn't obvious, specify the explicit type.

## 3. Pattern Matching
- **Rule**: Prefer C# 8+ pattern matching (e.g., `if (obj is MyType myObj)`) instead of casting (`var myObj = obj as MyType; if (myObj != null)`).
- **Rule**: Prefer `switch` expressions over `switch` statements for returning values.

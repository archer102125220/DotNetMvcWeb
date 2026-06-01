# C# Coding Standards

## Type Safety (MANDATORY)

- **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enabled by default. ALWAYS handle nulls properly.
- **NEVER use `dynamic` or `object`** unless absolutely necessary (e.g., when dealing with untyped JSON or Reflection). Use precise typing.
- **Limit `var` usage**: Only use implicit typing `var` when the type is blatantly obvious from the right side of the assignment (e.g., `var list = new List<string>();`). If it's not obvious, use the explicit type (e.g., `User user = await _userService.GetUserAsync();`).
- **Generic Collections**: Prefer generic collections `List<T>`, `Dictionary<TKey, TValue>` over untyped arrays or `ArrayList`.

## Examples

```csharp
// ❌ FORBIDDEN
dynamic data = GetData();
var result = ProcessSomething(); // Type is unclear

// ✅ REQUIRED
MyDataClass data = GetData();
var userList = new List<User>(); // Obvious type
```

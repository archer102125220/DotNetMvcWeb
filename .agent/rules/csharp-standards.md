# C# Language and Type Safety Rules

## 1. Nullable Reference Types
- The project runs with `<Nullable>enable</Nullable>`.
- **Rule**: Always handle nulls appropriately. Avoid using `!` (null-forgiving operator) unless you are absolutely certain the value cannot be null and the compiler simply cannot infer it.
- **Rule**: Use `ArgumentNullException.ThrowIfNull(param)` at the start of methods to guard against null arguments.

## 2. Strong Typing & Variable Declarations (`var`)
- **Rule**: NEVER use `dynamic` or `object` when a specific type can be used (e.g., reflection or untyped JSON are the rare exceptions).
- **Rule**: **Built-in types**: Always explicitly declare built-in primitive types (e.g., `int count = 0;`, `string name = "text";`, `bool isActive = true;`).
- **Rule**: **Non-built-in types (`var`)**: Prefer `var` for non-primitive types, method return values, object instantiations, and LINQ queries (e.g., `var user = await _userService.GetUserAsync(id);`, `var list = new List<string>();`), or use target-typed `new()` where appropriate (`List<string> list = new();`).
- **Rule**: Prefer generic collections `List<T>`, `Dictionary<TKey, TValue>` over untyped arrays or `ArrayList`.

## 3. Pattern Matching
- **Rule**: Prefer C# 8+ pattern matching (e.g., `if (obj is MyType myObj)`) instead of casting (`var myObj = obj as MyType; if (myObj != null)`).
- **Rule**: Prefer `switch` expressions over `switch` statements for returning values.

## Examples

```csharp
// ❌ FORBIDDEN / ANTI-PATTERNS
dynamic data = GetData();
var count = 0; // Built-in types should be explicitly declared: int count = 0;
var name = "text"; // Built-in types should be explicitly declared: string name = "text";

// ✅ RECOMMENDED (.NET Idiomatic Standard)
int count = 0;
string name = "text";
bool isReady = true;

var user = await _userService.GetUserAsync(id);
var userList = new List<User>(); // or List<User> userList = new();
var orderDetails = ProcessOrder(order);
```

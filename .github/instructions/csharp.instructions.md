---
applyTo: "**/*.cs"
---

# C# Rules

## Core Rules & Type Safety

- **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enabled. ALWAYS handle nulls properly.
- **Strict Typing**: NEVER use `dynamic` or `object` unless absolutely necessary (e.g., reflection or untyped JSON).
- **Collections**: Prefer generic collections (e.g., `List<T>`) over untyped ones (e.g., `ArrayList`).
- **Implicit Typing**: Avoid `var` unless the right side makes the type blatantly obvious (e.g., `var list = new List<string>()`).

## Runtime Data Validation & Null Checking

- **Strings**: Use `string.IsNullOrEmpty(str)` or `string.IsNullOrWhiteSpace(str)`.
- **Null Checking**: Use `if (obj is not null)` or the null-coalescing operator `??`.
- **Guard Clauses**: Use `ArgumentNullException.ThrowIfNull(obj)` at the start of methods to fail fast.
- **Pattern Matching**: Prefer `switch` expressions and pattern matching `if (obj is MyType myObj)` over older casting methods (`as MyType`).

## Lint Disable Comments (⚠️ CRITICAL)

- **NEVER** add `#pragma warning disable` or suppress C# compiler warnings without **explicit user instruction**.
- When encountering compiler warnings or errors:
  1. Report the warning to the user.
  2. Wait for the user's explicit instruction to add a suppression pragma.
  3. Only then add the disable comment with proper justification.
- This applies to ALL code analyzers and warning suppression mechanisms.

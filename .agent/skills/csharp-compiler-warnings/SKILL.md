---
name: csharp-compiler-warnings
description: Strict policy against ignoring or suppressing C# compiler warnings.
---

# csharp-compiler-warnings

When compiling the project (`dotnet build`), you may encounter warnings (e.g., CS8600 for possible null reference assignments).

## The Golden Rule
NEVER suppress warnings using `#pragma warning disable` or suppress exceptions using empty `catch` blocks.

## The Workflow
1. Read the warning output from `dotnet build`.
2. Find the offending line.
3. FIX the code (e.g., add a null check `if (x != null)`, change the type to nullable `string?`, or use `??`).
4. Re-run `dotnet build` to ensure the warning is gone.
5. If the fix is extremely complex and requires a massive refactor, ASK the user for permission to add a `#pragma warning disable` with a clear explanation of why.

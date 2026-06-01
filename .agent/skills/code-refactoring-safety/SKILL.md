---
name: code-refactoring-safety
description: Safety rules prohibiting the use of bash scripts for code refactoring in C#.
---

# code-refactoring-safety

When refactoring C# code, NEVER use automated bash scripts like `sed`, `awk`, or `perl` to find and replace text.

## Why?
C# relies heavily on `using` namespace imports and strict syntax. Text-based replacements often break the build by changing class names but failing to update the namespaces, or breaking string literals.

## What to use instead
1. Use AI code editing tools like `replace_file_content` and `multi_replace_file_content`.
2. After making changes, ALWAYS run `dotnet build` to ensure the project still compiles. If it fails, fix the compilation errors immediately.

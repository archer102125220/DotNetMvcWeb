# No Scripts for Code Refactoring (⚠️ CRITICAL)

**ABSOLUTELY FORBIDDEN: Using automated scripts (sed, awk, powershell, batch scripts) to modify code files.**

### Why
- Scripts only change text, they don't understand context or C# `using` namespace imports.
- It frequently causes C# compilation errors when class names change but usages or imports do not.
- .NET projects have strong typing and complex file structures that script-based find/replace often corrupts.

### ✅ Allowed
- Use AI tools: `replace_file_content`, `multi_replace_file_content`
- MUST verify `using` namespaces are correct and that `dotnet build` succeeds after every change.

### ❌ Forbidden
- `sed`, `awk`, `perl`, `powershell -Command`, `find ... -exec`
- Any batch text processing

### Exception
If absolutely necessary:
1. Get explicit human approval FIRST
2. Show complete script for review
3. Explain why manual tools or proper C# refactoring tools cannot do it

### Remember
**Scripts are blind. AI should be intelligent.**

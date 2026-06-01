# Warnings & Suppression rules (⚠️ CRITICAL)

## 1. Compiler Warnings
- **Rule**: NEVER suppress warnings (e.g. `#pragma warning disable CS8600`) without explicit authorization from the developer.
- **Rule**: When encountering a warning, fix the root cause (e.g., handle the null correctly) instead of hiding the warning.
- **Exception**: If fixing it requires massive architectural changes that are out of scope, explain the situation to the user and request permission to add the pragma.

## 2. Empty Catch Blocks
- **Rule**: NEVER use empty catch blocks (`catch (Exception) { }`). At minimum, log the exception using `ILogger`. Swallowing exceptions makes debugging impossible.

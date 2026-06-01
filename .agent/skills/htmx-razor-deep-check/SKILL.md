---
name: htmx-razor-deep-check
description: A deep checklist for ensuring HTMX interacts properly with Razor Views and Controllers.
---

# htmx-razor-deep-check

When adding interactivity to Razor Views using HTMX, you MUST perform these deep checks to prevent common bugs (like full page reloads inside a modal).

## Round 1: Basic Checks
- ✅ Is the HTMX attribute correct? (e.g. `hx-get`, `hx-post`)
- ✅ Does it specify a target? (`hx-target="#my-div"`)
- ✅ Is the target ID unique in the DOM?

## Round 2: Deep Checks (⚠️ MANDATORY)
You MUST check for these common mistakes:

| Anti-Pattern | Correct Pattern | Priority |
|--------------|----------------|----------|
| Controller returns `View()` for HTMX request | Return `PartialView()` or `ViewComponent()` | 🔴 High |
| Missing CSRF token in `hx-post` | Ensure `ValidateAntiForgeryToken` passes via headers or hidden inputs | 🔴 High |
| Script tags inside returned `PartialView` | Dispatch HTMX events (`HX-Trigger`) and listen externally | 🟡 Medium |
| Changing URL without `hx-push-url` | Add `hx-push-url="true"` to update browser history | 🟡 Medium |

**When to use this check:**
- When reviewing or refactoring HTMX features.
- When fixing issues where the layout duplicates (header inside a header) due to returning a full View instead of a PartialView.

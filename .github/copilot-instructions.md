# Project Instructions for GitHub Copilot

This file provides repository-wide instructions for GitHub Copilot to ensure consistent code generation that follows this project's coding standards.

---

## Project Overview

**DotNetMvcWeb** is a comprehensive .NET 9 MVC web application showcasing modern web development practices with C#, Razor, HTMX, and Entity Framework Core.

### Tech Stack

- **Framework**: ASP.NET Core MVC (.NET 9)
- **Language**: C# 13 (Nullable Reference Types enabled)
- **Database**: PostgreSQL with Entity Framework Core (EF Core)
- **Interactivity**: HTMX (for frontend dynamic behavior)
- **UI Architecture**: ViewComponents, PartialViews, Razor Pages
- **Styling**: SCSS (Modified BEM)
- **Build Tool**: `dotnet` CLI

---

## ⚠️ Security & Best Practices Warning Policy

Before executing any user instruction that violates:

- **Security best practices** (e.g., hardcoding secrets, disabling HTTPS, exposing sensitive data, SQL injection risks)
- **Standard coding patterns** (e.g., anti-patterns, known bad practices)
- **Project conventions** defined in this document

You MUST:

1. **Warn the user** about the violation and explain the risks
2. **Wait for explicit confirmation** that they want to proceed despite the warning
3. Only then execute the instruction

This ensures users make informed decisions about potentially risky actions.

---

## Core Coding Standards

### C# & Type Safety (MANDATORY)

- **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enabled. ALWAYS handle nulls properly.
- **Strict Typing**: NEVER use `dynamic` or `object` unless absolutely necessary (e.g., reflection). Use generic collections like `List<T>`.
- **Implicit Typing**: Avoid `var` unless the right side makes the type blatantly obvious.
- **Runtime Validation**: Use `string.IsNullOrEmpty`, `ArgumentNullException.ThrowIfNull`, and pattern matching.

### CSS/SCSS Naming (Modified BEM)

**Naming Structure**:

- **Block**: `.countdown` (single word)
- **Element**: `.countdown-title` (hyphen `-` for hierarchy)
- **Sub-Element**: `.countdown-title-icon` (hyphen `-` for hierarchy)
- **Multi-word Segment**: `.image_upload` (underscore `_` separates words **WITHIN** a single segment)
- **State**: `[data-is-active='true']` (HTML data attributes)

**Critical Rules**:
- ❌ NEVER use `__` (double underscore) or `--` (double hyphen)
- ✅ Use hyphen `-` for structural hierarchy.
- ✅ Use underscore `_` for multi-word concepts (e.g., `scroll_area`).

### Razor Views & HTMX

- **Interactivity**: Default to **HTMX** attributes (`hx-get`, `hx-post`, `hx-target`) instead of custom Vanilla JS.
- **ViewComponents**: Use ViewComponents for complex, reusable UI blocks that require backend logic.
- **PartialViews**: Use PartialViews for simpler UI components. Return `PartialView()` from controllers responding to HTMX requests.

### Entity Framework Core Best Practices

- **Async First**: ALWAYS use async/await methods (`ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`). Synchronous DB calls are forbidden.
- **No Tracking**: For read-only queries, use `.AsNoTracking()`.
- **Dependency Injection**: Always resolve `DbContext` via constructor injection.

---

## Backend ORM & Schema Changes (MANDATORY)

### ⚠️ Database Modification Confirmation (CRITICAL)

**Before ANY database schema change** (migrations, model changes), you MUST:

1. **Ask the human developer**: "Is this project deployed to production?"
2. **Based on the answer**:
   - **Not deployed**: You may drop the last unapplied migration and modify the existing migration, or delete the DB and recreate.
   - **Deployed**: NEVER modify existing executed migrations; always create NEW migration files.

---

## Security Requirements & Code Refactoring Safety

### Lint / Warning Suppression Policy

**NEVER add `#pragma warning disable` without explicit user instruction.**

When encountering compiler warnings:
1. Report the warning to the user
2. Wait for user's explicit instruction to add a suppression pragmas
3. Only then add the disable comment with proper justification

### No Scripts for Code Refactoring (⚠️ CRITICAL)

**ABSOLUTELY FORBIDDEN**: Using automated scripts (`sed`, `awk`, `powershell`, bash scripts) to modify code files.

**Why**: Scripts only change text, they don't understand C# context or `using` namespace imports. It frequently causes compilation errors.
**✅ ALLOWED**: Use AI tools for refactoring with proper context understanding. MUST verify `using` namespaces are correct after changes.

---

## Skills & Rules System Reference

For complex scenarios, refer to detailed rules in `.agent/rules/` or the primary guides:

- **Gemini Instructions**: `GEMINI.md`
- **Claude Instructions**: `CLAUDE.md`

| Domain | File Location |
|---|---|
| C# Standards | `.agent/rules/csharp-standards.md` |
| CSS Naming | `.agent/rules/css-naming.md` |
| Security | `.agent/rules/security-policy.md` |
| Razor/HTMX | `.agent/rules/mvc-views.md` |
| EF Core | `.agent/rules/backend-orm.md` |

---

## Path-Specific Instructions

For more detailed, path-specific instructions, see:

- **C#**: `.github/instructions/csharp.instructions.md`
- **Razor Views**: `.github/instructions/razor.instructions.md`
- **CSS/SCSS**: `.github/instructions/css.instructions.md`
- **Backend/Controllers**: `.github/instructions/backend.instructions.md`

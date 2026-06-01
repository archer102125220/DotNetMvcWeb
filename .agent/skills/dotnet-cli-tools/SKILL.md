---
name: dotnet-cli-tools
description: Best practices for using the .NET CLI for running, building, and managing packages.
---

# dotnet-cli-tools

When asked to run, build, or manage dependencies for this project, you MUST use the .NET CLI `dotnet`.

## Commands
1. **Run Application**: `dotnet run` or `dotnet watch run` (for hot reload).
2. **Build Application**: `dotnet build`
3. **Clean Application**: `dotnet clean`
4. **Manage Packages**: `dotnet add package <PackageName>` or `dotnet remove package <PackageName>`

## Environment Variables
If you need to run the application in a specific environment, set `ASPNETCORE_ENVIRONMENT`.
Example: `ASPNETCORE_ENVIRONMENT=Development dotnet run`

## Tooling
Never use `npm`, `yarn`, or `pnpm` to run backend build commands. Only use them if working specifically inside a frontend build pipeline (e.g., building Tailwind CSS in `wwwroot`), which is separate from the .NET backend.

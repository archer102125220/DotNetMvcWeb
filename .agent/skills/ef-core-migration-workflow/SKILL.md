---
name: ef-core-migration-workflow
description: Workflow and safety checks for EF Core database migrations.
---

# ef-core-migration-workflow

When asked to modify the database schema (add columns, create tables), you MUST follow this safety workflow.

## 1. Safety Check
Always ask the user: "Is this project deployed to production?"
- **If Yes (Deployed)**: You MUST create a new migration (`dotnet ef migrations add <Name>`). NEVER modify an existing migration file or delete the database.
- **If No (Not Deployed)**: You may modify the last unapplied migration directly or run `dotnet ef database drop` followed by `dotnet ef database update`.

## 2. Generating Migrations
Use the EF Core CLI tools:
```bash
dotnet ef migrations add AddNewColumn
dotnet ef database update
```
If the tool is missing, install it locally or globally: `dotnet tool install --global dotnet-ef`.

## 3. Review Generated Code
After running `dotnet ef migrations add`, ALWAYS use the `view_file` tool to inspect the generated `[Timestamp]_<Name>.cs` file in the `Migrations` folder. Ensure EF Core scaffolded the changes correctly (e.g., didn't drop a table by accident) before running `dotnet ef database update`.

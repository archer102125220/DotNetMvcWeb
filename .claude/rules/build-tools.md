# Build & Dev Tooling

This project uses the standard .NET CLI (`dotnet`) for all build, development, and execution tasks.

## Commands

### Development
- **Run Application**: `dotnet run`
- **Hot Reload (Watch)**: `dotnet watch` (Monitors source code and automatically reloads the browser when files change.)

### Build & Clean
- **Build**: `dotnet build`
- **Clean**: `dotnet clean`

### Package Management
- **Add Package**: `dotnet add package <PackageName>`
- Always verify compatibility with `<TargetFramework>net10.0</TargetFramework>` before adding a package.

### Database (Entity Framework Core CLI)
- **Add Migration**: `dotnet ef migrations add <MigrationName>`
- **Update Database**: `dotnet ef database update`
- **Remove Last Migration**: `dotnet ef migrations remove` (only for unapplied migrations)
- **Drop Database**: `dotnet ef database drop` (⚠️ USE WITH CAUTION)

## Environment Check

When starting the application, always check:
1. `appsettings.json` and environment-specific `appsettings.Development.json`.
2. Ensure connection strings map to the correct local or remote database.
3. Check `Properties/launchSettings.json` if configuring different launch profiles or ports (e.g., IIS Express vs Kestrel).

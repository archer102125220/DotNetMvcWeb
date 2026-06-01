# .NET Build & Dev Tooling

## 1. CLI Commands
- **Run**: `dotnet run` (Runs the application).
- **Hot Reload**: `dotnet watch run` (Watches for file changes and re-compiles/re-loads the app).
- **Build**: `dotnet build` (Compiles the application without running it).

## 2. Environments
- Use `ASPNETCORE_ENVIRONMENT` to control the environment (`Development`, `Staging`, `Production`).
- Configuration is loaded from `appsettings.json`, then overridden by `appsettings.{Environment}.json`.

## 3. Package Management
- Use `dotnet add package <PackageName>` to install NuGet packages.
- Always check compatibility with `<TargetFramework>net10.0</TargetFramework>` before adding a package.

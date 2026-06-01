# Security & XSS/SQLi Policies

## 1. Cross-Site Scripting (XSS)
- **Rule**: ASP.NET Core Razor automatically HTML-encodes output when using `@`. Do NOT use `@Html.Raw()` unless you explicitly trust the input and understand the risks.
- **Rule**: If returning JSON for HTMX to consume (rare, usually return HTML), ensure it is properly serialized.

## 2. SQL Injection
- **Rule**: EF Core using LINQ is automatically parameterized and safe from SQL Injection.
- **Rule**: NEVER use string interpolation to build raw SQL queries (`.FromSqlRaw($"SELECT * FROM Users WHERE Name = '{name}'")`). Instead, use `.FromSqlInterpolated($"SELECT * FROM Users WHERE Name = {name}")` which properly parameterizes the input.

## 3. Cross-Site Request Forgery (CSRF / XSRF)
- **Rule**: Always include `[ValidateAntiForgeryToken]` on POST/PUT/DELETE controller actions.
- **Rule**: Razor `form` tag helpers automatically inject the anti-forgery token. If using HTMX, ensure the token is included in the headers using `hx-headers` or dynamically added via a script on `htmx:configRequest`.

## 4. Secrets
- **Rule**: NEVER hardcode API keys or connection strings in code or `appsettings.json`. Use User Secrets during development (`dotnet user-secrets`) and environment variables in production.

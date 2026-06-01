# Entity Framework Core (EF Core) 安裝與使用教學

本文件提供在 .NET MVC 專案中安裝、設定及使用 Entity Framework Core (EF Core) 的標準流程。此外，也會強調本專案對於 EF Core 的**深度檢查政策 (Deep Check Policy)** 以及**資料庫遷移 (Migrations) 安全規範**。

## 1. 安裝 EF Core

透過 .NET CLI 安裝所需的套件。預設以 SQL Server 為例：

```bash
# 核心套件 (通常在安裝 Provider 時會自動相依)
dotnet add package Microsoft.EntityFrameworkCore

# 資料庫 Provider (依據實際使用的 DB 選擇)
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
# 若使用 PostgreSQL 則為: dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
# 若使用 SQLite 則為: dotnet add package Microsoft.EntityFrameworkCore.Sqlite
# 若使用 Oracle 則為: dotnet add package Oracle.EntityFrameworkCore

# EF Core 工具 (用於執行 Migrations 指令)
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

## 2. 設定資料庫連線

### 2.1 設定 `appsettings.json`

在 `appsettings.json` (或 `appsettings.Development.json`) 中加入連線字串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyDatabase;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### 2.2 建立 DbContext

在 `Models` 或是 `Data` 資料夾中建立繼承自 `DbContext` 的類別：

```csharp
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // 這裡定義 DbQuery 或是 DbSet
    // public DbSet<User> Users { get; set; } = null!;
}
```

### 2.3 在 `Program.cs` 註冊 DbContext

在 `Program.cs` 檔案中，將 `AppDbContext` 註冊至依賴注入容器中：

```csharp
using Microsoft.EntityFrameworkCore;
using DotNetMvcWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// 加入 DbContext 註冊
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ... 其他服務註冊

var app = builder.Build();
```

## 3. 資料庫遷移 (Migrations) 標準流程

⚠️ **資料庫修改確認規範 (CRITICAL)**：
在執行任何 Schema 變更前，**必須確認環境是否已部署至正式環境 (Production)**。
- **未部署**：可以刪除未套用的 migration 或刪除資料庫重新建立 (`dotnet ef database drop`, `dotnet ef database update`)。
- **已部署**：**絕對禁止**修改已執行過的 migrations，必須建立**新的** migration 檔案。

### 常用 Migrations 指令

```bash
# 1. 建立新的 Migration
dotnet ef migrations add <MigrationName>

# 2. 將 Migration 更新至資料庫
dotnet ef database update

# 3. 移除最後一個尚未更新到資料庫的 Migration
dotnet ef migrations remove
```

## 4. EF Core 開發深度檢查政策 (Deep Check)

開發或審查包含 C# Controller、Service 及資料存取 (EF Core) 相關程式碼時，必須遵守以下安全與效能檢查標準。

### Round 1: 表面檢查 (Basic Check)
- ✅ 標準語法及 namespace 引用是否正確。
- ✅ 透過依賴注入 (DI) 取得 `AppDbContext` (禁止使用 `new AppDbContext()`)。
- ✅ 變數命名與基本的 Null 檢查。

### Round 2: 深度檢查 (Deep Check) - ⚠️ MANDATORY
撰寫或修改 EF Core 查詢時，請務必避免以下 Anti-Patterns：

| 錯誤模式 (Anti-Pattern) | 正確做法 (Correct Pattern) | 優先級 |
|--------------|----------------|----------|
| 遺漏 `await` 或是不正確的回傳 Task | 必須明確加上 `await` 處理非同步呼叫 | 🔴 High |
| 迴圈中發生 N+1 查詢問題 | 使用 `.Include()`、`.Select()` 或在迴圈外先行批次讀取 | 🔴 High |
| 未釋放的 `IDisposable` (Streams, HttpClients) | 使用 `using (...) { }` 或 `using var obj = ...;` 包覆 | 🔴 High |
| 使用同步的 DB 呼叫 (`.ToList()`) | 必須使用非同步版本：`await .ToListAsync()` | 🟡 Medium |
| 針對唯讀操作追蹤實體 (Tracking) | 加上 `.AsNoTracking()` | 🟡 Medium |

### 範例：正確的唯讀查詢

```csharp
// 正確：非同步、NoTracking、DI 注入
public async Task<List<User>> GetActiveUsersAsync()
{
    return await _context.Users
        .AsNoTracking()
        .Where(u => u.IsActive)
        .ToListAsync();
}
```

遵循以上規範可以確保專案中的 EF Core 開發維持高效、安全，並避免常見的記憶體與效能陷阱。

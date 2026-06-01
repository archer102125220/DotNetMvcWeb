# ASP.NET Core MVC: 資料模型 (Models) 與 EF Core 開發指南

本指南整合了專案中關於資料模型 (`Models` 目錄) 的結構規範，以及 Entity Framework Core (EF Core) 的安裝、設定與深度檢查政策 (Deep Check Policy)。為維持專案架構清晰、安全且高效，請所有開發者嚴格遵守。

---

## 📂 第一部分：Models 目錄結構與職責

在 `Models` 資料夾下，我們應將不同用途的模型進行分類：

### 1. `Entities/` (實體模型)
- **用途**：代表資料庫的資料表結構，通常與 Entity Framework Core (EF Core) 搭配使用。
- **規則**：
  - **絕對不可以**直接將 Entity 傳遞給 Razor Views (`.cshtml`)，這會導致敏感資料（如密碼、雜湊值）外洩或引發 Lazy-Loading 的效能問題。
  - 僅在 `Data` (DbContext) 或 `Services` 層級中進行操作。

### 2. `ViewModels/` (視圖模型)
- **用途**：專門為 Razor Views (`.cshtml`) 設計的強型別資料結構。
- **規則**：
  - 每個 View 建議有自己專屬的 ViewModel（例如 `UserLoginViewModel`）。
  - View 中需要顯示什麼欄位，ViewModel 就只提供什麼欄位。
  - 可以在屬性上加入資料驗證標籤（Data Annotations）進行表單驗證。

### 3. `DTOs/` (資料傳輸物件)
- **用途**：用於前後端 API 呼叫、微服務通訊，或是 Service 層與 Controller 層之間的資料傳遞。
- **規則**：
  - 結構應保持扁平，僅包含傳輸所需的純資料。

---

## 🛡️ 第二部分：核心開發規範 (Controller-ViewModel 模式)

### 1. Thin Controllers 模式
- **Always use ViewModels**：Controller 的職責是從 Service 取得資料 (Entity / DTO)，將其轉換為 ViewModel，然後傳遞給 View。
- ❌ **錯誤示範**：直接回傳資料庫實體
  ```csharp
  public async Task<IActionResult> Profile() {
      User user = await _dbContext.Users.FindAsync(userId);
      return View(user); // ❌ 危險！可能外洩機密資料
  }
  ```
- ✅ **正確示範**：轉換為 ViewModel
  ```csharp
  public async Task<IActionResult> Profile() {
      var user = await _userService.GetUserAsync(userId);
      var viewModel = new UserProfileViewModel {
          Username = user.Username,
          Email = user.Email
      };
      return View(viewModel); // ✅ 安全
  }
  ```

### 2. 型別安全與 Nullable Reference Types
- 專案已啟用 `<Nullable>enable</Nullable>`，請**務必**正確處理 Null。
- 如果某個字串或物件允許為空，請標記為 `?`。
- 若不允許為空但在建構時尚未賦值，可使用 `required` 關鍵字（C# 11+）或給予預設值：
  ```csharp
  public required string Title { get; set; }
  public string Content { get; set; } = string.Empty;
  ```

### 3. 執行時期資料驗證 (Runtime Data Validation)
在 `ViewModels` 或 `DTOs` 中，善用 Data Annotations 搭配 Controller 中的 `ModelState.IsValid` 進行後端防護。
- 字串檢查：優先使用 `string.IsNullOrEmpty()` 或 `string.IsNullOrWhiteSpace()`。
- 驗證標籤範例：
  ```csharp
  using System.ComponentModel.DataAnnotations;

  public class UserRegisterViewModel
  {
      [Required(ErrorMessage = "使用者名稱為必填")]
      [StringLength(50, MinimumLength = 3, ErrorMessage = "長度必須在 3 到 50 個字元之間")]
      public required string Username { get; set; }

      [Required]
      [EmailAddress(ErrorMessage = "Email 格式不正確")]
      public required string Email { get; set; }
  }
  ```

### 4. 避免動態與弱型別
- **嚴禁使用** `dynamic` 或 `object`（除非必須使用 Reflection 或處理未知結構的 JSON）。
- 避免在 View 中使用 `ViewBag` 或 `ViewData` 來傳遞複雜資料，一律使用強型別的 ViewModel。

---

## 🛢️ 第三部分：Entity Framework Core (EF Core) 實作指南

既然 `Entities/` 是與資料庫互動的核心，以下整理了 EF Core 的標準設定與安全使用原則。

### 1. 基礎安裝與設定
若專案尚未設定 EF Core，可依下列步驟完成：

```bash
# 核心套件與工具
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

**連線字串 (`appsettings.json`)：**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1434;Database=DotNetMvcDb;User Id=AppUser;Password=AppUser!123456789;TrustServerCertificate=True;"
  }
}
```

**註冊 DbContext (`Program.cs`)：**
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 2. 資料庫遷移 (Migrations) 安全規範

⚠️ **資料庫修改確認規範 (CRITICAL)**：
在執行任何 Schema 變更前，**必須確認環境是否已部署至正式環境 (Production)**。
- **未部署**：可以刪除未套用的 migration 或刪除資料庫重新建立 (`dotnet ef database drop`, `dotnet ef database update`)。
- **已部署**：**絕對禁止**修改已執行過的 migrations，必須建立**新的** migration 檔案。

**常用指令：**
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
dotnet ef migrations remove
```

### 3. EF Core 開發深度檢查政策 (Deep Check)

開發或審查包含 EF Core 相關程式碼時，必須遵守以下安全與效能檢查標準：

#### Round 1: 表面檢查 (Basic Check)
- ✅ 標準語法及 namespace 引用是否正確。
- ✅ 透過依賴注入 (DI) 取得 `AppDbContext` (禁止使用 `new AppDbContext()`)。
- ✅ 變數命名與基本的 Null 檢查。

#### Round 2: 深度檢查 (Deep Check) - ⚠️ MANDATORY
撰寫或修改 EF Core 查詢時，請務必避免以下 Anti-Patterns：

| 錯誤模式 (Anti-Pattern) | 正確做法 (Correct Pattern) | 優先級 |
|--------------|----------------|----------|
| 遺漏 `await` 或是不正確的回傳 Task | 必須明確加上 `await` 處理非同步呼叫 | 🔴 High |
| 迴圈中發生 N+1 查詢問題 | 使用 `.Include()`、`.Select()` 或在迴圈外先行批次讀取 | 🔴 High |
| 未釋放的 `IDisposable` (Streams, HttpClients) | 使用 `using (...) { }` 或 `using var obj = ...;` 包覆 | 🔴 High |
| 使用同步的 DB 呼叫 (`.ToList()`) | 必須使用非同步版本：`await .ToListAsync()` | 🟡 Medium |
| 針對唯讀操作追蹤實體 (Tracking) | 加上 `.AsNoTracking()` | 🟡 Medium |

**正確的唯讀查詢範例：**
```csharp
public async Task<List<User>> GetActiveUsersAsync()
{
    return await _context.Users
        .AsNoTracking() // 唯讀時不追蹤實體，提升效能
        .Where(u => u.IsActive)
        .ToListAsync(); // 使用非同步方法
}
```

---

## 🎯 總結檢查清單 (Checklist)
- [ ] 實體 (Entity) 是否被隔離，沒有直接傳遞給 View？
- [ ] ViewModel 的命名是否明確且針對特定的 View？
- [ ] 屬性的 Nullable (`?`) 標示是否精確？
- [ ] 是否已經為表單 ViewModel 加上了適當的 `[Required]` 等驗證標籤？
- [ ] 所有 EF Core 查詢是否都採用非同步 (`await`)？
- [ ] 唯讀的查詢是否都加上了 `.AsNoTracking()`？

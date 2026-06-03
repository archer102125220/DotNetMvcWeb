# EF Core Migrations 運作原理：Diffing 與 Seed Data

這份筆記詳細說明了 Entity Framework Core (EF Core) 在執行 `dotnet ef migrations add` 時背後究竟是如何運作的，以及如何透過宣告式的方式來新增預設資料 (Seed Data)。

## 1. 指令背後的魔法：狀態比對 (Diffing)

初學者常會有個疑問：「我只是下達 `dotnet ef migrations add <Name>`，沒有加上任何參數，EF Core 怎麼知道我要新增欄位還是刪除資料表？」

答案是：**EF Core 並不是靠「指令參數」來得知變更，而是透過「比較（Diffing）」來推斷的。** 它的運作方式非常類似 Git 等版本控制系統。

### 運作流程

當你執行建立 Migration 的指令時，EF Core 會經歷以下步驟：

1. **抓取「現在」的狀態 (Current State)**
   EF Core 會掃描你專案中目前的 `AppDbContext` 類別以及裡面所有的 `DbSet<Model>` 和 `OnModelCreating` 裡的設定。把這些收集起來，在記憶體中建構出一個「目前程式碼期望的資料庫結構模型」。

2. **抓取「過去」的狀態 (Previous Snapshot)**
   在你的 `Migrations` 資料夾內，有一個名為 `*Snapshot.cs` 的檔案（通常叫 `AppDbContextModelSnapshot.cs`）。這個檔案記錄了「上一次成功建立 Migration 時的資料庫結構長相」。這代表了「上一版」的狀態。

3. **進行比對並產生指令 (Diffing & Generation)**
   EF Core 會拿「現在的程式碼」去跟「過去的快照」進行比對：
   * **情境 A**：發現現在的 C# 類別多了一個 `Age` 屬性，但 Snapshot 裡沒有 👉 推斷你想新增欄位，產生 `AddColumn("Age", ...)` 指令。
   * **情境 B**：發現 Snapshot 裡原本有一個 `Address` 欄位，但現在的 C# 程式碼裡不見了 👉 推斷你想刪除欄位，產生 `DropColumn("Address", ...)` 指令。

**總結來說：你的「C# 程式碼」本身就是你對資料庫結構的宣告。工具會自己負責找出差異並翻譯成 SQL 升級指令。**

---

## 2. 如何宣告並產生 Seed Data (種子資料)

了解了 Diffing 機制後，產生預設資料 (Seed Data) 的原理也完全相同！**你不需要手動去修改 Migration 檔裡的 `Insert` 語法**，而是要把資料寫在你的 `AppDbContext` 裡面。

### 步驟示範

1. **在 `DbContext` 中宣告資料**
   打開 `AppDbContext.cs`，覆寫 `OnModelCreating` 方法，並使用 `.HasData()` 來宣告資料。

   ```csharp
   public class AppDbContext : DbContext
   {
       public DbSet<OracleDemoItem> OracleDemoItems { get; set; }

       protected override void OnModelCreating(ModelBuilder modelBuilder)
       {
           base.OnModelCreating(modelBuilder);

           // 在這裡宣告 Seed Data (種子資料)
           modelBuilder.Entity<OracleDemoItem>().HasData(
               new OracleDemoItem 
               { 
                   Id = 1, // ⚠️ 注意：Seed Data 通常必須明確指定主鍵 Id
                   Name = "測試項目 1", 
                   Description = "這是第一筆預設資料",
                   CreatedAt = new DateTime(2026, 6, 2, 8, 0, 0, DateTimeKind.Utc)
               },
               new OracleDemoItem 
               { 
                   Id = 2, 
                   Name = "測試項目 2", 
                   Description = "這是第二筆資料",
                   CreatedAt = new DateTime(2026, 6, 2, 8, 5, 0, DateTimeKind.Utc)
               }
           );
       }
   }
   ```

2. **執行指令產生 Migration**
   ```bash
   dotnet ef migrations add SeedOracleDemoData
   ```

3. **背後發生的事**
   EF Core 比對發現：目前的 `AppDbContext` 裡定義了 Id 為 1 和 2 的資料，但上一次的 Snapshot 裡並沒有這些資料的紀錄。因此，它推斷你想要新增這兩筆資料，便自動產生了對應的 `Migration` 檔案，裡面包含了 `InsertData` 的指令。

### 宣告式設計的優勢

* **修改資料**：如果你把 `"測試項目 1"` 修改成 `"正式項目 1"`，再跑一次 `migrations add`。EF Core 會比對出差異，自動產生一個包含 **`UpdateData`** 的 Migration。
* **刪除資料**：如果你把整段 `new OracleDemoItem { Id = 2, ... }` 刪掉，再跑一次 `migrations add`。EF Core 會發現資料消失了，自動產生一個 **`DeleteData`** 的 Migration。

透過這種方式，你的 C# 程式碼永遠是 Single Source of Truth (唯一的真相來源)，EF Core 只是負責把你的 C# 翻譯成資料庫看得懂的變化而已！

---

## 3. DbContext 的生命週期與攔截點 (Hooks)

在 `AppDbContext` 中，`OnModelCreating` 負責了「定義資料表結構」與「產生預設資料」，這是在 EF Core 初次啟動並建構模型時執行的生命週期方法。

除此之外，`DbContext` 還有幾個非常實用且強大的生命週期方法與攔截點，讓開發者可以在資料庫操作的不同階段介入處理：

### 1. `OnConfiguring` (設定 DbContext)
* **觸發時機**：在 `DbContext` 初始化，準備連線到資料庫之前。
* **功用**：用來設定資料庫 Provider（例如 Oracle, SQL Server）與連線字串。在現代 ASP.NET Core 專案中，通常已被 `Program.cs` 裡的 Dependency Injection 取代，但開發單體 Console App 時仍會用到。

**範例程式碼：**
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        optionsBuilder.UseOracle("你的連線字串");
    }
}
```

### 2. 覆寫 `SaveChanges` / `SaveChangesAsync` (寫入前攔截)
這是日常開發中最常被覆寫的生命週期方法。
* **觸發時機**：當呼叫 `SaveChanges()`，EF Core 準備將 C# 裡的變更轉換成 SQL 語法寫入資料庫的「前一刻」。
* **常見應用**：**自動記錄時間 (Auditing)** 與 **軟刪除 (Soft Delete)**。例如，利用 `ChangeTracker` 自動為所有新增或修改的資料壓上 `CreatedAt` 和 `UpdatedAt` 時間，就不需要在每個 Controller 裡手動寫入。

**範例程式碼（自動記錄時間）：**
```csharp
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    // 透過 ChangeTracker 找出所有正在被「新增」或「修改」的實體
    var entries = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

    foreach (var entry in entries)
    {
        // 假設你的 Model 有 UpdatedAt 屬性，在這裡統一壓上當前時間
        if (entry.Property("UpdatedAt") != null)
        {
            entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
        }

        // 如果是新增的，順便壓上 CreatedAt
        if (entry.State == EntityState.Added && entry.Property("CreatedAt") != null)
        {
            entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
        }
    }

    // 處理完後，再呼叫原本的 SaveChanges 去真正執行 SQL
    return base.SaveChangesAsync(cancellationToken);
}
```

### 3. EF Core 攔截器 (Interceptors)
針對更複雜或底層的需求，EF Core 提供了 Interceptors 機制，能將攔截邏輯從 DbContext 中抽離出來。
* **`ISaveChangesInterceptor`**：功能與覆寫 `SaveChanges` 類似，但能在執行前、後、甚至失敗時攔截。
* **`DbCommandInterceptor`**：超級底層的攔截。能在 EF Core 發送任何一句 SQL 指令到資料庫之前攔截，常用於自訂效能監控、Log 記錄、或是動態修改 SQL 語法。

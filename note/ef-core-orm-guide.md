# Entity Framework Core (EF Core) 實戰教學指南

本文件記錄了在 .NET MVC 專案中使用 EF Core (搭配 Oracle 資料庫) 的核心觀念、常用指令，以及實務上常遇到的錯誤與踩雷經驗。

> 💡 **相關閱讀**：如果想了解這些資料庫操作是如何與前端 UI (MVC + HTMX) 結合的，請參考 [Oracle Database MVC & HTMX 實作導讀指南](./oracle-mvc-demo-guide.md)。
> 💡 **進階概念**：想了解為什麼 EF Core 與 Node.js/PHP 的 ORM 在核心設計上有如此巨大的差異？請參考 [ORM 設計哲學比較：Code-First vs Migration-First](./orm-architecture-comparison.md)。
> 💡 **資料庫指令**：如果需要進入 Docker 容器內直接下達 Oracle SQL 指令除錯，請參考 [Oracle Database 常用指令與操作指南](./oracle-database-commands-guide.md)。

---

## 1. 常用的 EF Core CLI 指令

在開發過程中，當修改了 `Models` 或是 `AppDbContext`，都需要透過以下指令來同步資料庫結構：

### 建立 Migration (資料庫遷移檔)
```bash
dotnet ef migrations add <MigrationName>
```
* **時機**：當新增、修改或刪除 Model 屬性，或是修改了 `OnModelCreating` 裡的設定後。
* **作用**：EF Core 會比對現有的程式碼與過去的 `ModelSnapshot`，自動產生出對應的 C# 檔案 (放在 `Migrations` 資料夾內)，裡面包含了 `Up()` (升級) 與 `Down()` (降級) 的邏輯。
* **範例**：`dotnet ef migrations add InitialCreate`

### 更新資料庫 (套用 Migration)
```bash
dotnet ef database update
```
* **時機**：在建立完 Migration 檔案，或是剛拉下同事寫好的新程式碼後。
* **作用**：將尚未套用到資料庫的 Migration，正式轉換為 SQL 語法並執行到資料庫中。

### 復原 (刪除) 最後一個尚未套用的 Migration
```bash
dotnet ef migrations remove
```
* **時機**：如果剛執行完 `migrations add`，但發現程式碼寫錯了想重來（前提是**還沒**執行 `database update`）。
* **作用**：刪除最後一次產生的 Migration 檔案，並退回前一個 Snapshot 狀態。

### 刪除整個資料庫 (開發環境限定)
```bash
dotnet ef database drop
```
* **時機**：開發初期如果資料庫搞得太亂，想要整個砍掉重練。
* **警告**：絕對不要在正式環境 (Production) 執行！

---

> 💡 **運作原理進階**：如果你好奇 `dotnet ef migrations add` 是如何推斷並產生程式碼的，以及 Seed Data 是如何運作的，請參考 [EF Core Migrations 運作原理：Diffing 與 Seed Data](./ef-core-migration-mechanism.md)。

## 2. Migrations 檔案結構解析

每當執行 `dotnet ef migrations add` 時，EF Core 其實會在 `Migrations` 資料夾內產生 **兩個** 檔案，它們扮演著不同但互補的角色：

### 1. 動作指令檔 (`[時間戳]_[名稱].cs`)
* **這是什麼**：「給資料庫的施工圖」。
* **功用**：包含了 `Up()` 與 `Down()` 兩個方法。記錄了要對資料庫執行什麼動作（例如 `CreateTable` 或 `InsertData`）。當下達 `database update` 時，執行的就是這個檔案。

### 2. 模型快照檔 (`[時間戳]_[名稱].Designer.cs`)
* **這是什麼**：「施工完成後的 3D 建模圖」。
* **功用**：這個檔案裡面沒有動作，而是記錄了當時 C# 程式碼中的 Model 結構。下次當再次建立 Migration 時，EF Core 會把「現在的 C# 程式碼」跟「這份 Designer 快照」做比對，藉此算出到底改了哪些欄位，進而產生下一張施工圖。

### 💡 為什麼檔案會越來越多？
Migrations 資料夾就像是資料庫的 **Git Commit 歷史紀錄**。每一次的異動都會產生新檔案。
* 這是正常的現象，它能確保團隊協作時的變更不會衝突，並且不影響程式執行效能。
* **最終快照 (`AppDbContextModelSnapshot.cs`)**：這個檔案永遠只有一個，它會隨時保持更新，記錄著目前資料庫「最新、最完整的全貌」。

### 💡 產生後需要手動編輯它們嗎？
**預設情況下，這兩個檔案「都不應該編輯」！**
它們是 EF Core 根據 C# 程式碼自動推算出來的產物。但如果真的遇到了 EF Core 猜錯意圖的情況（例如只是**重新命名**了欄位，但 EF Core 卻判定為「刪除舊欄位並新增空欄位」），可以這麼做：

1. **唯一能改的是 `.cs` 動作指令檔**：
   可以手動把 `DropColumn` 與 `AddColumn` 刪除，改成 `migrationBuilder.RenameColumn(...)`，以確保舊資料不會被清空。
2. **絕對禁止修改 `.Designer.cs` 模型快照檔**：
   這個檔案是 EF Core 內部的「記憶卡」。如果手動去竄改它，會導致 EF Core 的記憶跟真實的 C# 程式碼「脫節 (Desync)」。當下次下達 `migrations add` 指令時，整個資料庫的遷移時間軸就會徹底亂掉。
3. **正確的修改流程**：
   如果下完指令發現產生的 Migration 內容是錯的（例如忘記加屬性），**請不要手動改檔案**。正確的做法是先執行 `dotnet ef migrations remove`，把 C# 程式碼改對後，再重新執行一次 `dotnet ef migrations add`！

---

## 3. Seed Data (預設資料) 的兩種策略

在系統初始化時，我們通常需要寫入一些預設資料。EF Core 提供了兩種正規做法：

### 策略 A：使用 `HasData` (Model Builder Seeding)
* **寫法**：在 `AppDbContext.cs` 的 `OnModelCreating` 方法中撰寫。
* **特性**：資料會被綁定在 Migration 中，產生出明確的 `INSERT` 語法。
* **適用**：寫死不變的靜態資料 (如：身分權限、下拉選單的選項、國家代碼)。

### 策略 B：獨立的 DbInitializer (Data Seeder)
* **寫法**：獨立建立一個類別 (如 `Seeders/DbInitializer.cs`)，並在 `Program.cs` 啟動時呼叫它。
* **特性**：透過正常的 EF Core 邏輯 (`context.Add()`) 寫入資料，不會產生在 Migration 檔中。支援動態產生假資料、隨機時間等。
* **適用**：大量測試用的動態假資料、需要呼叫外部 API 取得的初始資料。

### 💡 為什麼沒有 `seed` 建立的指令？
如果學過 Laravel (`php artisan db:seed`) 或 Rails (`rails db:seed`)，可能會疑惑為什麼在 .NET 的 EF Core 中沒有看到專門用來產生或執行 Seed 的 CLI 指令。
這是因為 **.NET MVC (EF Core) 預設將 Seed 的行為自動化了**：
1. **策略 A (`HasData`) 的執行時機**：當執行 `dotnet ef database update` 時，它包含在 Migration 裡面，更新資料表的同時就會把 Seed 寫進去了。
2. **策略 B (`DbInitializer`) 的執行時機**：我們將呼叫邏輯寫在了 `Program.cs` (應用程式的進入點) 裡面。因此，只要執行 `dotnet run` 或 `dotnet watch run` 啟動網站，程式在啟動伺服器前就會自動執行這段 Seed 邏輯。
   
這代表 **不需要** 下達任何額外的 Seed 指令，一切都在資料庫更新與應用程式啟動時自動完成了！

---

## 4. 實務踩雷紀錄與錯誤排除

以下是我們在實作過程中遇到的兩個經典錯誤，特別記錄下來以防未來再次踩坑：

### ❌ 錯誤一：HasData 內使用了動態變數 (如 DateTime.UtcNow)

**錯誤訊息：**
> `The model for context 'AppDbContext' changes each time it is built. This is usually caused by dynamic values used in a 'HasData' call (e.g. new DateTime(), Guid.NewGuid()).`

**原因解析：**
當使用 **策略 A (`HasData`)** 時，EF Core 每次編譯都會檢查 Model 是否有變動。如果寫了 `DateTime.UtcNow`，每一次編譯抓到的時間都不一樣，EF Core 會誤以為「模型結構又改變了」，從而報錯要求每次都生一個新的 Migration。

**解決方法：**
在 `HasData` 裡面，**絕對不能使用動態變數**。必須改為寫死的靜態數值：
```csharp
// 錯誤寫法
CreatedAt = DateTime.UtcNow

// 正確寫法
CreatedAt = new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc)
```

---

### ❌ 錯誤二：Oracle Identity 序號與手動指定 ID 的衝突 (ORA-00001)

**錯誤訊息：**
> `OracleException (0x80004005): ORA-00001: unique constraint violated on table OracleDemoItems columns (Id)`
> `row with column values (Id:3) already exists`

**發生情境：**
我們在這次的範例中，**刻意同時混用**了 **策略 A (`HasData`)** 與 **策略 B (`DbInitializer`)** 來操作同一張資料表。
(註：這是為了在一份專案中同時展示並保留這兩種寫法的參考範例，才特別這樣設計的。)
1. 我們先用 `HasData` **強制指定**了 `Id = 1, 2, 3` 來建立三筆資料。
2. 接著我們用 `DbInitializer` 試圖新增兩筆資料，**沒有指定 ID**，想讓 Oracle 自動遞增分配。

**原因解析：**
Oracle 的自動遞增欄位 (Identity Sequence) 非常死板。當我們透過 Migration 強制塞入 ID `1, 2, 3` 的資料時，Oracle 內部的「序號發放器」**並不會跟著前進**，它依舊認為下一個要發放的 ID 是 `1`。
因此，當 `DbInitializer` 跑來要新號碼時，Oracle 發了 `1` 給它，結果發現資料庫裡早就存在 ID `1` 的資料了，就引發了主鍵重複的錯誤 (Unique Constraint Violated)。

**解決方法：**
1. **短期解法 (我們這次的做法)**：因為我們這次是為了展示兩種語法而刻意混用，所以在 `DbInitializer` 中也明確把 ID 寫死 (例如指定為 4 與 5)，就能避免跟前面的 1, 2, 3 撞號。
2. **長期解法 (實務建議)**：在實際開發上，盡量**不要混用**這兩種方式來操作同一張資料表。如果一張表要自動遞增，就全都交給 `DbInitializer` 處理，不要在 `HasData` 中強制寫死 ID。

---

## 5. 最佳實踐守則

1. 每次寫完 Model 的修改，先 `dotnet build` 確認沒有編譯錯誤，再執行 `migrations add`。
2. 開發階段如果 Migration 檔案很亂，善用 `migrations remove`，不要急著 `database update`。
3. `Program.cs` 中執行 `DbInitializer` 時，記得加上 **Guard Clause (防護機制)** (例如 `if (context.Items.Any()) return;`)，避免每次重啟程式都重複寫入相同的測試資料。

# EF Core 多資料庫 (Multi-DbContext) 開發與 Migration 管理指南

在一般的專案中，通常只會連線到一個資料庫（單一 `DbContext`）。但是當系統龐大，或者需要同時操作不同來源的資料庫（例如同時連接 Oracle 與 MySQL），我們就會在專案中建立多個 `DbContext`。

這時， Entity Framework Core 的 Migration (資料庫遷移) 管理就會變得稍微複雜，因為我們必須將不同資料庫的「藍圖（Migrations）」分開存放，避免互相干擾。

## 為什麼需要分開存放 Migrations？

如果不把 Migration 檔案分開，EF Core 預設會把它們全部擠在專案根目錄的 `Migrations` 資料夾裡，這會造成災難：
1. **Model Snapshot 衝突**：每個 DbContext 都有一份記錄當前資料庫最終狀態的 `ModelSnapshot.cs`，放在一起會互相覆蓋。
2. **語法不相容**：不同的資料庫（如 Oracle 與 MySQL）底層語法差異巨大，產生的 C# Migration 程式碼完全不同，混在一起不僅難以閱讀，執行時也一定會報錯。

## 正規的多 DbContext 建立 Migration 流程

假設我們專案裡有兩個 DbContext：
- `AppDbContext` (用來連線 Oracle)
- `MysqlDbContext` (用來連線 MySQL)

當我們要為 MySQL 建立第一份 Migration 時，**絕對不能只打 `dotnet ef migrations add`**，必須加入兩個關鍵參數：

```bash
dotnet ef migrations add InitialMysqlDemo -c MysqlDbContext -o Migrations/MysqlMigrations
```

### 參數解析：
- **`-c MysqlDbContext` (Context)**
  明確指定這次的 Migration 是針對 `MysqlDbContext` 的實體模型產生的。
  （如果不指定，EF Core 發現專案有多個 DbContext 會直接報錯拒絕執行）。

- **`-o Migrations/MysqlMigrations` (Output Directory)**
  指定產生的檔案要放在哪個獨立的子資料夾裡。
  這樣一來，MySQL 專屬的設計圖就會乾乾淨淨地躺在 `MysqlMigrations` 中，不會去污染到 Oracle 之前的歷史檔案。

## 同類型資料庫的多連線情境

有時候，我們並不是要連線不同種類的資料庫，而是要**同時連線兩個同類型的資料庫**（例如：兩個不同的 MySQL 伺服器，一個放「會員資料」，一個放「訂單資料」）。

在這種情境下，我們依然會建立兩個不同的 DbContext（例如：`MemberDbContext` 與 `OrderDbContext`）。那麼 Migrations 該怎麼處理呢？

**答案是：作法完全一樣！還是強烈建議使用 `-o` 將它們分開存放。**

```bash
# 會員資料庫的 Migration 放在 MemberMigrations 資料夾
dotnet ef migrations add InitialMember -c MemberDbContext -o Migrations/MemberMigrations

# 訂單資料庫的 Migration 放在 OrderMigrations 資料夾
dotnet ef migrations add InitialOrder -c OrderDbContext -o Migrations/OrderMigrations
```

### 為什麼同類型的資料庫也要分開資料夾？
1. **Model Snapshot 不會互相干擾**：雖然 EF Core 在產生 Snapshot 時會加上 DbContext 名稱（如 `MemberDbContextModelSnapshot.cs`），所以技術上不會覆蓋，但把不同資料庫的歷史紀錄混在同一個資料夾裡會變得極度難以追蹤。
2. **避免 Migration 類別名稱衝突**：如果兩個 DbContext 都剛好建立了一個叫做 `AddCreatedAtColumn` 的 Migration，放在同一個資料夾就會產生同名類別的編譯錯誤。
3. **專案結構清晰**：以不同資料夾管理不同資料庫，在團隊開發時一眼就能看出每個 DbContext 目前的架構演進進度。

## 更新資料庫 (Database Update)

當您建立好特定資料庫的 Migration，要正式推送到資料庫建立資料表時，一樣必須指定 `-c` 參數，告訴 EF Core 您現在要更新的是哪一個資料庫：

```bash
# 將 MysqlMigrations 裡的設計圖，更新進 MySQL 資料庫
dotnet ef database update -c MysqlDbContext
```

> **提示**：更新資料庫時不需要加 `-o` 參數，因為 EF Core 只要知道是哪個 DbContext，就會自動去尋找它所屬的 Migration 檔案（它會參考上次產生的歷史紀錄，或依照 DbContext 組態去尋找）。

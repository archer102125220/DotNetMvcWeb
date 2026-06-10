# 原生 ADO.NET (Npgsql) 操作指南

這份筆記整理了如何在 .NET 專案中，不透過 Entity Framework Core (ORM)，而是直接使用 **原生 ADO.NET (Npgsql)** 與 PostgreSQL 資料庫進行連線與操作。

---

## 1. 什麼是 ADO.NET？為什麼要用它？

ADO.NET 是 .NET 平台最基礎的資料存取技術。我們平常用的 Entity Framework Core (EF Core) 其實底層也是基於 ADO.NET 實作的。

### 何時應該選擇原生 ADO.NET？
雖然 EF Core 提供了強大的 LINQ 查詢與物件關聯對應 (ORM)，但在以下情境，直接使用原生 ADO.NET 會是更好的選擇：
- **極致的效能要求**：不需要經過 ORM 的物件轉換與追蹤 (Tracking)，查詢速度最快、記憶體佔用最低。
- **極度複雜的 SQL**：遇到多層 Subquery、複雜的 Window Functions (分析函數)、特定的 PostgreSQL 獨有語法 (如 JSONB 操作)，EF Core LINQ 很難或無法轉換時。
- **大批量資料處理 (Bulk Operations)**：一次要寫入或更新數十萬筆資料 (`Npgsql` 提供了 `COPY` 功能做極速批次匯入)。

---

## 2. 套件安裝

要連線到 PostgreSQL 資料庫，我們需要官方推薦的 Driver。
- **套件名稱**：`Npgsql`

> [!NOTE]
> **本專案現況：**
> 因為專案中已經安裝了 `Npgsql.EntityFrameworkCore.PostgreSQL`，而它已經將 `Npgsql` 作為底層相依套件一併安裝了。因此，您**不需要**額外手動安裝，直接在程式碼中加入 `using Npgsql;` 即可使用。

---

## 3. ADO.NET 核心三大物件

在操作原生資料庫時，通常圍繞著以下三個核心物件：
1. **`NpgsqlConnection`**：負責建立與資料庫實體的連線。
2. **`NpgsqlCommand`**：負責攜帶並執行 SQL 指令。
3. **`NpgsqlDataReader`** (或 `DbDataReader`)：負責將查詢結果，以「流式 (Streaming)」的方式逐筆從資料庫讀回記憶體。

---

## 4. 基礎查詢範例程式碼

以下是在 Controller 或 Service 中呼叫原生 ADO.NET 撈取資料的標準寫法：

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public async Task<List<PostgresDemoItem>> GetItemsViaAdoNet()
{
    // 1. 取得連線字串 (可直接從現有 DbContext 取，或透過 IConfiguration 讀取 appsettings.json)
    string connectionString = _context.Database.GetConnectionString();
    var resultList = new List<PostgresDemoItem>();

    // 2. 建立連線 (使用 await using 確保安全釋放)
    await using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
    {
        // 3. 開啟連線 (必須使用 Async 版本)
        await connection.OpenAsync();

        // 4. 建立 Command
        await using (NpgsqlCommand command = connection.CreateCommand())
        {
            // 5. 撰寫 SQL 語法
            // 注意：Postgres 如果透過 EF Core 建表，欄位可能會有雙引號保護大小寫。
            command.CommandText = "SELECT \"Id\", \"Name\", \"CreatedAt\", \"Description\" FROM \"PostgresDemoItems\"";

            // 6. 執行查詢並取得 Reader
            await using (var reader = await command.ExecuteReaderAsync())
            {
                // 7. 逐筆讀取資料
                while (await reader.ReadAsync())
                {
                    resultList.Add(new PostgresDemoItem
                    {
                        // 透過索引值取資料效能最好
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        CreatedAt = reader.GetDateTime(2),
                        
                        // 處理可能為 NULL 的欄位，必須先呼叫 IsDBNull 檢查
                        Description = reader.IsDBNull(3) ? null : reader.GetString(3)
                    });
                }
            }
        }
    }

    return resultList;
}
```

---

## 5. ⚠️ 撰寫時的必備規範與陷阱 (Deep Check Rules)

在專案中撰寫原生 ADO.NET 時，**必須嚴格遵守以下規範**，否則極易造成系統崩潰或記憶體洩漏：

### 🚨 規則一：強制使用 `await using` 包覆 `IDisposable`
`NpgsqlConnection`、`NpgsqlCommand` 與 `DataReader` 都實作了 `IDisposable`。
資料庫連線是非常昂貴的資源，如果沒有確實釋放，Connection Pool 會迅速耗盡 (Timeout)。
- **❌ 錯誤寫法**：`var conn = new NpgsqlConnection(...);`
- **✅ 正確寫法**：`await using (var conn = new NpgsqlConnection(...)) { ... }`

### 🚨 規則二：非同步優先 (Async First)
為了避免在資料存取時阻塞伺服器的執行緒 (Thread Starvation)，所有 I/O 操作必須是非同步的。
- **❌ 錯誤寫法**：`conn.Open()`, `command.ExecuteReader()`, `reader.Read()`
- **✅ 正確寫法**：`await conn.OpenAsync()`, `await command.ExecuteReaderAsync()`, `await reader.ReadAsync()`

### 🚨 規則三：小心 PostgreSQL 的大小寫區分
預設情況下，PostgreSQL 會將所有沒有引號的 Table 與 Column 強制轉成**小寫**。
但在 .NET 中，透過 EF Core Code-First 產生的資料表，通常會被加上雙引號以保留 CamelCase (例如 `"PostgresDemoItems"`)。
這意味著手寫 SQL 時，如果不加雙引號會被當成全小寫，進而找不到欄位或資料表 (`relation does not exist`)。
- **✅ 正確寫法**：必須加上雙引號 `SELECT \"Id\" FROM \"PostgresDemoItems\"`。

### 🚨 規則四：處理 NULL 值 (`IsDBNull`)
在使用 `DataReader` 讀取資料時，如果資料庫裡的該欄位是 `NULL`，直接呼叫 `reader.GetString()` 會引發錯誤或強轉失敗。
- **❌ 錯誤寫法**：`Description = reader.GetString(3)`
- **✅ 正確寫法**：`Description = reader.IsDBNull(3) ? null : reader.GetString(3)`

### 🚨 規則五：防止 SQL Injection (必須使用參數化查詢)
當 SQL 語句中需要動態帶入使用者的輸入條件時，**絕對禁止使用字串拼接**。必須使用 `NpgsqlParameter` 來進行參數化查詢，否則會面臨嚴重的 SQL Injection (隱碼攻擊) 風險。
> PostgreSQL 的參數化變數慣例使用 `@` 開頭 (與 Oracle 的 `:` 不同)。
- **❌ 錯誤寫法**：`command.CommandText = $"SELECT * FROM \"Users\" WHERE \"Name\" = '{userName}'";`
- **✅ 正確寫法**：
  ```csharp
  command.CommandText = "SELECT * FROM \"Users\" WHERE \"Name\" = @name";
  command.Parameters.Add(new NpgsqlParameter("name", userName));
  ```

### 🚨 規則六：重複使用 Command 時必須清空或更新 Parameters
在同一個方法中，如果您重用同一個 `NpgsqlCommand` 物件來執行多個不同的 SQL 語句，**必須注意清除舊的參數**，或者使用更新參數值的方式，否則執行第二個 SQL 時可能會因為帶入前一個指令的參數而發生錯誤。
- **❌ 錯誤寫法** (參數累積導致錯誤)：
  ```csharp
  command.CommandText = "SELECT * FROM \"Users\" WHERE \"Id\" = @id";
  command.Parameters.Add(new NpgsqlParameter("id", 1));
  await command.ExecuteReaderAsync();

  // 忘記清空 Parameters，第二個查詢會連同 id 參數一起送出！
  command.CommandText = "SELECT * FROM \"Orders\" WHERE \"UserId\" = @userId";
  command.Parameters.Add(new NpgsqlParameter("userId", 1));
  await command.ExecuteReaderAsync();
  ```
- **✅ 正確寫法 1** (使用 `Parameters.Clear()`)：
  ```csharp
  command.Parameters.Clear(); // 執行新查詢前先清空參數
  command.CommandText = "SELECT * FROM ...";
  command.Parameters.Add(...);
  ```
- **✅ 正確寫法 2** (保留參數定義，僅更新值)：
  ```csharp
  command.Parameters["id"].Value = 2; // 若 SQL 相同，只需更新值即可重用
  await command.ExecuteReaderAsync();
  ```

---

## 6. 其他常用操作

除了 `ExecuteReaderAsync` (用於 SELECT 查詢回傳多筆)，還有兩種常見的 Command 執行方式：

1. **`ExecuteScalarAsync`**：
   用於只回傳「單一值」的查詢，例如 COUNT、SUM 等聚合函數。
   ```csharp
   command.CommandText = "SELECT COUNT(*) FROM \"PostgresDemoItems\"";
   var count = Convert.ToInt32(await command.ExecuteScalarAsync());
   ```

2. **`ExecuteNonQueryAsync`**：
   用於執行 INSERT、UPDATE、DELETE 等不需要回傳資料表，只需回傳「受影響行數」的指令。
   ```csharp
   command.CommandText = "UPDATE \"PostgresDemoItems\" SET \"Name\" = 'Test' WHERE \"Id\" = 1";
   int affectedRows = await command.ExecuteNonQueryAsync();
   ```

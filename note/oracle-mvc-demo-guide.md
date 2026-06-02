# Oracle Database MVC & HTMX 實作導讀指南

這份文件是用來幫助您快速理解本專案中「Oracle Demo」模組的運作架構。我們成功地將 .NET Core MVC 與 Oracle 資料庫結合，並透過 HTMX 實現了現代化的無重整 (SPA-like) 體驗。

> 💡 **相關閱讀**：如果您想深入了解本模組背後的資料庫設定、EF Core 常用指令及踩坑紀錄，請參考 [Entity Framework Core (EF Core) 實戰教學指南](./ef-core-orm-guide.md)。

---

## 1. 核心架構概覽

這個示範模組完全遵循標準的 MVC (Model-View-Controller) 架構：

### 📦 Model (資料模型)
* **`Models/OracleDemoItem.cs`**：定義了資料庫中 `OracleDemoItems` 資料表的結構（包含 `Id`, `Name`, `Description`, `CreatedAt`）。
* **`Data/AppDbContext.cs`**：負責定義與 Oracle 資料庫的連線上下文，以及包含 `HasData` (Seed Data) 的靜態設定。

### 🎮 Controller (控制器)
* **`Controllers/OracleDemoController.cs`**：負責處理所有的 HTTP 請求 (CRUD)。
* 在這個控制器中，我們大量使用了 `async / await` 以及 EF Core 的 `.AsNoTracking()` 來最佳化讀取效能。

### 🖼️ View (畫面視圖)
* **`Views/OracleDemo/Index.cshtml`**：主頁面。負責載入框架與基本版面，並引用了 HTMX。
* **`Views/OracleDemo/_DemoList.cshtml`**：**Partial View (部分檢視)**，專門用來渲染資料列表。
* **`Views/OracleDemo/_CreateOrEdit.cshtml`**：**Partial View (部分檢視)**，專門用來渲染新增/編輯的表單。

---

## 2. HTMX 的互動魔法

這是本專案最特別的地方。我們沒有寫任何一行自訂的 JavaScript (Ajax / fetch)，而是完全依賴 **HTMX 屬性**來完成非同步更新：

### 核心運作邏輯
傳統的 MVC 送出表單後，伺服器會回傳「整頁 HTML」，導致畫面閃爍重整。
而加入了 HTMX 後的流程變成：
1. 使用者點擊按鈕，觸發 `hx-post` 或 `hx-get` 送出非同步請求。
2. Controller 接收到請求，完成資料庫操作。
3. Controller **不回傳整頁 `View()`**，而是回傳**小塊的 `PartialView("_DemoList")`**。
4. HTMX 接收到這塊小 HTML 後，依據 `hx-target` 找到畫面上的指定區塊，並將其替換 (`hx-swap`)。

### 常用的 HTMX 屬性範例
* `hx-get="/OracleDemo/List"`：向後端發起 GET 請求取得資料。
* `hx-target="#demo-list-container"`：告訴 HTMX，拿到回傳的 HTML 後，要把它塞進 id 為 `demo-list-container` 的元素裡面。
* `hx-swap="innerHTML"`：替換目標元素「內部」的 HTML。
* `hx-confirm="確定要刪除嗎？"`：在發送請求前，自動跳出瀏覽器的原生確認對話框。

### Controller 端的特殊處理
如果在處理表單送出時發現「驗證失敗」(例如必填欄位沒填)，我們會在 Controller 這樣寫：
```csharp
Response.Headers["HX-Retarget"] = "#form-container";
Response.Headers["HX-Reswap"] = "innerHTML";
return PartialView("_CreateOrEdit", item);
```
這能動態告訴前端的 HTMX：「嘿，這次不要去更新列表了，請把這個帶有錯誤訊息的表單，重新塞回 `#form-container` 裡面！」

---

## 3. CSS 命名規範 (BEM)

在這個模組中，我們實踐了經過修改的 BEM 命名法，所有樣式都寫在 `wwwroot/css/oracle-demo.css` 中：

* **Block (區塊)**：`.oracle-demo`
* **Element (元素)**：`.oracle-demo-header`、`.oracle-demo-list`
* **Modifier (狀態)**：目前較少使用，但若有會像 `.oracle-demo-item--active`

這種命名方式可以確保我們的 CSS 不會意外污染到專案中其他頁面的樣式。

---

## 4. 如何測試與執行

1. **確保 Oracle 資料庫運行中**：請確認您透過 Docker 起的 `dot-net-mvc-web-oracle-free-db` 容器正在運作。
2. **啟動專案**：在專案根目錄執行 `dotnet watch run`。
3. **前往頁面**：開啟瀏覽器前往 `http://localhost:<您的port>/OracleDemo`。
4. **體驗功能**：您可以試著新增一筆資料、故意漏填名稱觸發驗證錯誤、修改現有資料、或是刪除資料。您會發現所有的操作都非常流暢，畫面完全不會發生閃爍或重整！

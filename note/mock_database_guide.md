# .NET MVC 學習導讀：無真實資料庫 (Mock Database) 範例專案

歡迎來到這個 .NET MVC 學習專案！這份文件將帶領你了解如何在「**不連接真實資料庫**」的情況下，運用記憶體 (In-Memory) 來模擬資料存取，以此學習 ASP.NET Core MVC 的核心概念。

這種設計模式非常適合用來學習：
1. **依賴注入 (Dependency Injection, DI)**：如何將服務注入到 Controller 中。
2. **介面抽象 (Interface Abstraction)**：如何透過介面解耦 Controller 與具體的資料庫實作。
3. **MVC 與 Web API 雙軌架構**：同一個資料庫如何同時供應給網頁視圖 (Views) 與 API (JSON) 使用。

---

## 1. 核心觀念：資料模型與介面 (Models)

在 `Models/` 資料夾下，我們定義了資料結構與操作資料的合約（介面）。

### A. 實體模型 (`Product.cs`)
這是一個單純的 C# 類別 (POCO)，代表了我們要操作的資料結構。
- 屬性如 `Id`, `Name`, `Price`, `Description`。
- 在未來的真實專案中，這個模型會對應到資料庫中的一張資料表 (Table)。

### B. 存取介面 (`IProductRepository.cs`)
我們不希望 Controller 綁死在某一種特定的資料庫（例如 SQL Server），因此我們宣告了 `IProductRepository` 介面，裡面定義了 CRUD (新增、讀取、更新、刪除) 的方法。
- 方法皆回傳 `Task` 或 `Task<T>`，這是因為**真實的資料庫操作都應該是非同步的 (Asynchronous)**，我們在模擬階段就先遵循這個標準，未來切換成真實 DB 時 Controller 就不用改寫。

---

## 2. 模擬資料庫實作 (`ProductRepository.cs`)

這是本專案的精華所在：用記憶體來假裝我們有一個資料庫。

```csharp
public class ProductRepository : IProductRepository
{
    private readonly List<Product> _products = new() { ... };
    private readonly object _lock = new();

    // ...實作 CRUD 方法
}
```

### 為什麼要有 `lock (_lock)`？ (執行緒安全)
在 Web 應用程式中，可能會有好幾百個使用者**同時**發送 Request 來到伺服器。
因為這個 Repository 在整個應用程式生命週期中只有一個實例（Singleton，稍後說明），所有人都會存取同一個 `_products` List。如果兩個人同時對 List 進行寫入 (Add/Remove)，會導致程式崩潰或資料錯亂。
因此，我們加上 `lock`，確保同一個時間只有一個 Request 能夠修改這份名單。

### 為什麼要用 `Task.FromResult`？
由於我們的 List 是在記憶體中，讀取速度極快，本身是同步 (Synchronous) 的操作。但因為介面 `IProductRepository` 規定要回傳非同步的 `Task`，所以我們用 `Task.FromResult()` 與 `Task.CompletedTask` 將同步的結果包裝成假裝是非同步的回傳值。

---

## 3. 將模擬資料庫註冊到系統中 (`Program.cs`)

在 ASP.NET Core 中，我們統一在 `Program.cs` 註冊系統需要使用的服務：

```csharp
builder.Services.AddSingleton<IProductRepository, ProductRepository>();
```

**什麼是 `AddSingleton`？**
- 這代表**依賴注入 (DI)** 容器在啟動時，只會建立「一個」`ProductRepository` 實例。
- 整個網站從開啟到關閉，所有的 Controller 都會拿到這同一個實例，所以我們剛才說的 `_products` List 能夠跨 Request 保持資料狀態（你新增了一筆資料，重新整理網頁後還在）。

> **💡 未來連接真實資料庫 (EF Core) 時的差異：**
> 真實的資料庫連線不應該從頭到尾共用一條。屆時我們會將其改為 `AddScoped` (每個 Request 建立一次新的連線)。

---

## 4. 控制器如何使用資料 (Controllers)

我們有兩種 Controller 來示範不同的應用場景：

### A. Web API 控制器 (`Controllers/Api/ProductsController.cs`)
這用來提供給前端框架 (例如 Vue, React) 或手機 App 呼叫，回傳 JSON 格式。
- **建構子注入**：Controller 不會自己 `new ProductRepository()`，而是透過建構子要求系統給它一個 `IProductRepository`。這就是依賴注入的體現。
- **Action 方法**：如 `GetProducts()`, `CreateProduct()` 等，標註了 `[HttpGet]`, `[HttpPost]`。它們內部呼叫 `await _productRepository.GetAllAsync()` 來取得資料，然後回傳 `Ok(products)` (HTTP 200) 或是 `NotFound()` (HTTP 404)。

### B. MVC 控制器 (`Controllers/ProductsController.cs`)
這是傳統的伺服器渲染網頁模式 (Server-Side Rendering)，會搭配 `Views` 資料夾底下的 `.cshtml` 檔案。
- 運作邏輯與 API 相似，同樣透過建構子注入 `IProductRepository`。
- 差異在於它不是回傳 JSON，而是回傳 `View(products)`，把資料丟給 HTML 模板去渲染出畫面。
- 表單送出時，透過 `[ValidateAntiForgeryToken]` 防止 CSRF 攻擊，並透過 `ModelState.IsValid` 驗證資料。

---

## 5. 進階觀念：MVC 與 API Controller 的邏輯複用 (Service Layer)

你可能會發現 `ProductsController` 與 `Api/ProductsController` 裡面有重複的程式碼（例如都會呼叫 `await _productRepository.GetAllAsync()` 並檢查是否為 null）。
這帶出了一個重要的架構觀念：**Controller 適合做邏輯複用嗎？**

### A. 為什麼它們目前看起來很像？
因為現在專案只做極度簡單的 CRUD (新增/讀取/更新/刪除)，邏輯薄到只有一兩行程式碼，在這種情況下（被稱為「貧血模型」），直接讓 Controller 呼叫 Repository 是可以接受的。

### B. 當專案變大時該怎麼辦？(三層式架構)
在真實的企業級專案中，「新增產品」可能還伴隨著：檢查價格不得為負數、檢查名稱是否重複、寫入 Log、寄送通知 Email 等等。
如果我們把這些 **「商業邏輯 (Business Logic)」** 寫在 Controller 裡面（Fat Controller），MVC 和 API 兩邊就會出現大量重複且難以維護的程式碼。

> **⚠️ 絕對不要讓 MVC Controller 去繼承或呼叫 API Controller。**

標準的 .NET MVC 解決方案是抽出一個 **「服務層 (Service Layer)」**：
1. **建立 `ProductService`**：將商業邏輯與 Repository 的呼叫包裝在裡面。
2. **註冊服務**：在 `Program.cs` 加上 `builder.Services.AddScoped<ProductService>();`。
3. **注入 Controller**：MVC 與 API Controller 都改為注入 `ProductService` 而非直接注入 Repository。

這樣一來，Controller 就只負責「接收 HTTP 請求」與「決定回傳格式 (HTML 或 JSON)」，達到完美的**關注點分離 (Separation of Concerns)**。

---

## 總結與下一步

透過這套模擬架構，你可以在完全不需要安裝 SQL Server 或設定連線字串的情況下，專心學習 **Controller 的路由**、**依賴注入**以及 **MVC 的視圖渲染**。

當你熟悉了這些運作原理後，下一步就可以開始學習 **Entity Framework Core (EF Core)**。
屆時，你只需要：
1. 寫一個新的 `EfProductRepository` 實作 `IProductRepository` (透過 DbContext 去讀寫資料庫)。
2. 到 `Program.cs` 把 `AddSingleton<IProductRepository, ProductRepository>()` 換成新的註冊方式。
3. 你的 Controllers 完全不需要動任何一行程式碼，整個系統就能無縫切換到真實的資料庫了！這就是**介面抽象 (Interface Abstraction)** 最強大的威力。

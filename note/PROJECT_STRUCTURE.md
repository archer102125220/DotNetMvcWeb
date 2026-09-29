# .NET Core MVC 專案架構與設定檔說明

這份文件旨在說明本專案 (`DotNetMvcWeb`) 的資料夾結構與重要設定檔的用途，幫助開發者快速了解 .NET Core MVC 專案的基礎架構。

## 📁 核心資料夾結構

.NET Core MVC 專案採用約定優於配置 (Convention over Configuration) 的設計模式，標準資料夾結構如下：

### 1. `Controllers/` (控制器)
- **用途**：處理使用者的 HTTP 請求 (Request)，並決定要回傳哪個畫面 (View) 或資料。
- **說明**：MVC 中的 **C**。檔案名稱通常以 `Controller` 結尾（例如：`HomeController.cs`）。
- **流程**：接收路由 (Route) 導向的請求 -> 呼叫 Model 或服務處理商業邏輯 -> 將資料傳遞給 View。

### 2. `Models/` (模型)
- **用途**：代表應用程式的資料結構或商業邏輯狀態。
- **說明**：MVC 中的 **M**。通常包含用來與資料庫對應的 Entity 類別，或是在 Controller 與 View 之間傳遞資料的 ViewModel (例如：`ErrorViewModel.cs`)。

### 3. `Views/` (視圖)
- **用途**：負責呈現使用者介面 (UI)，將資料渲染成 HTML 格式回傳給瀏覽器。
- **說明**：MVC 中的 **V**。使用 Razor 語法 (`.cshtml` 檔案)，允許在 HTML 中撰寫 C# 程式碼。
- **結構**：
  - 通常會依據 Controller 名稱建立子資料夾（例如 `Views/Home/` 對應 `HomeController`）。
  - `Shared/`：存放跨頁面共用的視圖元件，如版型配置 (`_Layout.cshtml`) 或部分檢視 (Partial Views)。

### 4. `wwwroot/` (靜態資源)
- **用途**：存放所有可以直接透過 URL 存取的靜態檔案。
- **說明**：包含 CSS 樣式表 (`css/`)、JavaScript 檔案 (`js/`)、圖片 (`images/`) 及第三方前端套件 (例如 Bootstrap, jQuery 等庫通常放在 `lib/`)。
- **注意**：應用程式只能直接存取 `wwwroot` 內的靜態檔案，這是基於安全性的設計。

### 5. `Properties/` (專案屬性)
- **用途**：存放與專案執行、建置或發佈相關的設定。
- **重要檔案 (`launchSettings.json`)**：定義了在開發環境下啟動專案的設定，包括本機伺服器的 Port 號、環境變數 (`ASPNETCORE_ENVIRONMENT`) 等。

### 6. `Services/` (商業邏輯服務層)
- **用途**：負責處理系統的商業邏輯規則 (Business Logic) 與流程調度（例如：時間戳記指派、跨實體檢核）。
- **架構設計**：
  - `Interfaces/`：服務介面定義（例如 `IOracleDemoItemService.cs`），讓 Controller 只依賴抽象介面。
  - `Implements/`：具體實作類別。

### 7. `Repositories/` (資料存取倉儲層)
- **用途**：專職負責與資料庫的資料存取 (Data Access)，將 EF Core LINQ、原生 SQL、ADO.NET 連線及預存程序完整封裝，不讓底層資料庫細節滲透到商業層。
- **架構設計**：
  - `Interfaces/`：倉儲介面定義（例如 `IOracleDemoItemRepository.cs`）。
  - `Implements/`：具體實作類別（例如 `OracleDemoItemRepository.cs`）。

### 8. `Data/` (資料庫上下文)
- **用途**：存放 Entity Framework Core 的 `DbContext` 類別（如 `AppDbContext`、`MysqlDbContext` 等），管理實體與資料表的映射關聯及連線組態。

---

## 🏛️ 本專案架構設計：兩種分層模式對照

為了提供最佳的學習體驗，本專案在不同的資料庫示範模組中，同時展示了業界常見的兩種架構風格：

| 架構模式 | 適用模組 | 呼叫流程 (Data Flow) | 特點與適用情境 |
| :--- | :--- | :--- | :--- |
| **Service Layer Pattern**<br>*(簡化版 Repository)* | MSSQL<br>MySQL<br>PostgreSQL | `Controller` $\to$ `Service` $\to$ `DbContext` | 輕量直覺，由 Service 同時處理業務邏輯並操作 `DbContext`，適合中小型專案或原型開發。 |
| **3-Tier + Repository Pattern**<br>*(標準倉儲架構)* | Oracle | `Controller` $\to$ `Service` $\to$ `Repository` $\to$ `DbContext` | 職責徹底解耦。Service 只管業務規則，Repository 專責資料存取，便於使用 `Moq` 進行純粹的單元測試，適合大型企業級系統。 |


---

## ⚙️ 重要設定檔

專案根目錄下有幾個關鍵的設定檔，負責應用程式的啟動與組態設定：

### 1. `Program.cs`
- **用途**：應用程式的進入點 (Entry Point)。
- **功能**：
  - 建立 Web 伺服器主機 (WebHostBuilder)。
  - 註冊相依性注入 (Dependency Injection, DI) 容器的服務。
  - 設定 HTTP 請求的處理管線 (Middleware Pipeline)，例如：靜態檔案支援、路由設定、MVC 模式等。
- **注意**：在較新的 .NET 版本 (自 .NET 6 起)，`Startup.cs` 的功能已被整合進 `Program.cs`，採用頂層語句 (Top-level statements) 讓程式碼更加簡潔。

### 2. `appsettings.json` 與 `appsettings.Development.json`
- **用途**：應用程式的組態設定檔 (Configuration)。
- **功能**：以 JSON 格式儲存全域設定，例如資料庫連線字串 (Connection Strings)、日誌等級 (Logging)、第三方 API 金鑰等。
- **環境差異**：
  - `appsettings.json`：預設的通用設定檔。
  - `appsettings.Development.json`：專屬**開發環境**的設定檔，此檔案中的設定會覆寫 `appsettings.json` 中同名的項目。部署到正式環境時可建立對應的 `appsettings.Production.json`。

### 3. `DotNetMvcWeb.csproj`
- **用途**：C# 專案檔 (Project File)。
- **功能**：以 XML 格式定義專案的屬性，包括：
  - 目標框架 (Target Framework，例如 `net7.0` 或 `net8.0`)。
  - 專案參考 (Project References)。
  - NuGet 套件參考 (Package References)，記錄專案安裝的所有第三方套件及其版本。

### 4. `global.json` (若有)
- **用途**：指定要用於該專案 (或資料夾下所有專案) 的 .NET SDK 版本。
- **功能**：確保開發團隊或 CI/CD 流程中使用一致的 SDK 版本進行建置，避免因 SDK 版本差異造成的相容性問題。

---

## 總結

了解這套標準架構後，開發時的典型流程通常是：
1. 在 `Controllers` 中定義邏輯和端點。
2. 根據需求在 `Models` 中建立資料結構。
3. 在 `Views` 建立對應的 `.cshtml` 負責畫面呈現。
4. 將需要的樣式或腳本放到 `wwwroot`。
5. 將應用程式層級的設定加進 `appsettings.json`。

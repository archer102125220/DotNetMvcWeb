# 🤖 DotNet Mvc Web - AI 協作開發規範指南 (AI Coding Guidelines)

本文件綜合了專案中的各個 AI 設定檔 (`.agent`, `.claude`, `.cursorrules`, `.github`, `AGENTS.md`, `CLAUDE.md`, `GEMINI.md`)，為開發過程中的 AI 助手 (如 Copilot, Claude, Gemini) 提供統一、詳盡的中文規範指引。

---

## ⚠️ 1. 安全性與最佳實踐警告原則 (Security & Best Practices)

在執行任何可能違反以下情況的使用者指令前，AI 必須嚴格遵守「警告並確認」機制：
* **安全最佳實踐**：如硬編碼機密資訊 (hardcoding secrets)、停用 HTTPS、暴露敏感資料、SQL 注入風險等。
* **標準程式碼模式**：如反模式 (anti-patterns)、已知的不良實踐。
* **本文件定義之專案慣例**。

**強制處理流程**：
1. **警告使用者**：指出違規情況並解釋潛在風險。
2. **等待明確確認**：必須得到使用者的明確同意。
3. **執行指令**：確認後才可執行該指令。

---

## 🚀 2. C# 與型別安全 (C# & Type Safety)

* **可 Null 參考型別 (Nullable Reference Types)**：專案已啟用 `<Nullable>enable</Nullable>`。**必須**妥善處理所有可能的 null 情況。
* **嚴格型別 (Strict Typing)**：絕對**禁止**使用 `dynamic` 或 `object`，除非在反射 (Reflection) 或處理無型別 JSON 等絕對必要的情況下。應優先使用強型別的泛型集合 (如 `List<T>`) 而非 `ArrayList`。
* **隱式型別 (Implicit Typing)**：避免使用 `var`，除非等號右側的型別已非常明顯 (例如：`var list = new List<string>();`)。

### 🛡️ 執行期資料驗證與 Null 檢查 (Runtime Validation)
* **字串檢查**：使用 `string.IsNullOrEmpty(str)` 或 `string.IsNullOrWhiteSpace(str)`。
* **Null 檢查**：使用 `if (obj is not null)` 或 Null 聯合運算子 (`??`)。
* **防護子句 (Guard Clauses)**：在方法開頭使用 `ArgumentNullException.ThrowIfNull(obj)`。
* **模式匹配 (Pattern Matching)**：優先使用 `switch` 運算式與 `if (obj is MyType myObj)` 進行轉型與比對，取代舊式的 `as MyType` 語法。

---

## 🎨 3. 前端開發與樣式規範

### 📌 CSS/SCSS 命名慣例 (修改版 BEM)
* **區塊 (Block)**：單字，例如 `.countdown`。
* **元素 (Element)**：使用連字號 (`-`) 分隔 Block 與 Element，例如 `.countdown-title`。表示結構階層關係。
* **子元素 (Sub-Element)**：使用連字號 (`-`)，例如 `.countdown-title-icon`。
* **多字詞組合 (Multi-word Segment)**：在單一語意區塊內使用底線 (`_`) 分隔，例如 `.image_upload` 或 `.scroll_area`。
* **狀態 (State)**：使用 HTML data 屬性，例如 `[data-is-active='true']`。
* 🛑 **嚴格禁止**：絕對不使用雙底線 (`__`) 或雙連字號 (`--`)。

### 🧩 視圖根類別與樣式重用 (View Root Class)
* **唯一根類別**：每個 Razor View 都應有一個基於其名稱的獨特 root class (如 `.home_index_page`)。共用元件也需有獨立的 root class (如 `.image_upload`)。
* **樣式重用**：在 SCSS 中定義 `%placeholder_name`，並使用 `@extend` 或 `@use` 來重用樣式，避免在 HTML 標籤中堆砌過多 class，讓 HTML class 嚴格與 DOM 結構綁定。

---

## ⚡ 4. Razor 視圖與 HTMX 互動

* **HTMX 優先**：強烈建議使用 HTMX 屬性 (`hx-get`, `hx-post`, `hx-target`, `hx-swap`) 來處理前端互動。除非 HTMX 無法解決，否則不應撰寫原生的 Vanilla JS 或 AJAX。
* **Partial Views**：當 Controller 處理 HTMX 請求時，應回傳 `PartialView("_MyComponent")` 而非 `View()`。不應在 Partial Views 內撰寫行內 `<script>`，應適當限制作用域或使用 HTMX 事件。
* **ViewComponents**：對於需要後端邏輯運算的複雜、可重用 UI 區塊，請使用 ViewComponents (`@await Component.InvokeAsync(...)`)，取代一般的 Partial Views。

---

## 🗄️ 5. Entity Framework Core 與資料庫操作

### 🔄 非同步與效能最佳化
* **非同步優先 (Async First)**：所有資料庫操作**必須**使用 async/await (如 `ToListAsync()`, `FirstOrDefaultAsync()`)，嚴禁使用同步呼叫 (如 `.ToList()`)。
* **無追蹤 (No Tracking)**：唯讀查詢必須加上 `.AsNoTracking()` 以提升效能。
* **依賴注入 (DI)**：永遠透過建構子注入 `DbContext`，絕對不要使用 `new AppDbContext()` 實例化。

### ⚠️ EF Core 與記憶體深度檢查 (Deep Check Policy)
在審查或重構後端程式碼時，AI **必須**進行兩輪檢查：
1. **第一輪 (表面檢查)**：語法、`using` 匯入、DI 注入正確性、變數命名及基本 Null 檢查。
2. **第二輪 (深度檢查) [強制]**：
   * 🔴 漏掉 `await` 或未正確處理 `Task`。
   * 🔴 迴圈內的 **N+1 查詢問題** (應使用 `.Include()`, `.Select()` 或在迴圈前批次取得)。
   * 🔴 未釋放 `IDisposable` 資源 (Stream, HttpClient 等應使用 `using (...) { }` 或 `using var obj = ...;`)。
   * 🟡 EF Core 的同步呼叫。
   * 🟡 唯讀查詢未加上 `.AsNoTracking()`。
*(註：若 AI 僅執行第一輪檢查，必須明確宣告：「⚠️ I have only performed basic checks. EF Core and Memory deep checks are still required.」)*

### 🚨 資料庫 Schema 變更規範
**在進行任何資料庫 Schema 變更 (Migration, Model 修改) 之前，AI 必須：**
1. 詢問開發者：「這個專案是否已部署至 Production 環境？」
2. 根據回覆：
   * **未部署**：可刪除最後一個未套用的 Migration 並修改現有的，或刪除 DB 重建 (`dotnet ef database drop`, `dotnet ef database update`)。
   * **已部署**：**絕對不可**修改已執行的 Migration，必須建立全新的 Migration 檔案 (`dotnet ef migrations add AddNewColumn`)。

---

## 🏗️ 6. 專案架構與 MVC 慣例

* **Controllers/**：視圖控制器必須繼承自 `Controller`，API 控制器繼承自 `ControllerBase`。類別名稱必須以 `Controller` 結尾。
* **Models/**：包含 Entity 類別、ViewModels 及 DTOs。
* **Views/**：Razor 視圖 (`.cshtml`) 的資料夾結構必須與 Controller 名稱對齊 (如 `Views/Home/Index.cshtml`)。
* **wwwroot/**：存放靜態資源 (CSS, JS, 圖片, 第三方函式庫)。
* **展示區 (Demo Views)**：功能展示的全頁內容應放於 `Controllers/DemoController.cs` 與 `Views/Demo/`，命名採用 PascalCase (如 `BannerDemo.cshtml`)。相關子元件放於 `Views/Demo/Components/`。
* **ASP.NET Core 穩定 API**：優先使用標準的 MVC 模式與內建依賴注入。除非舊有程式碼必需，否則使用 `System.Text.Json` 而非 `Newtonsoft.Json`。

---

## 🛠️ 7. 開發工具、設定與其他規範

* **開發環境 (dotnet CLI)**：
  * 使用 `dotnet run` 或 `dotnet watch` 進行熱重載開發。
  * 執行前務必確認 `appsettings.json` 與 `appsettings.Development.json` 設定正確。
* **國際化 (i18n)**：
  * 採用標準的 `Microsoft.AspNetCore.Mvc.Localization`。
  * 後端：注入 `IStringLocalizer<SharedResource>` 進行翻譯。
  * 前端 (Razor)：加入 `@inject IViewLocalizer Localizer`。
* **警告與 Lint 忽略政策**：
  * **絕對不可**在沒有使用者明確指示下加入 `#pragma warning disable`。
  * 若遇到編譯器警告：先向使用者回報 ➡️ 等待明確指示 ➡️ 加上停用註解與正當理由。
* **禁止腳本重構 (No Scripts for Refactoring)**：
  * **絕對禁止**使用 `sed`, `awk`, `powershell`, bash 腳本等自動化腳本來修改程式碼，因為腳本無法理解 C# 語意與 `using` 命名空間。
  * ✅ **允許作法**：使用 AI 工具 (如 `replace_file_content`) 進行精準修改，並在修改後務必驗證 `using` 宣告與建置狀態。

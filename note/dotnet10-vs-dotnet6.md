# .NET 10 vs .NET 6 (以前稱為 .NET Core 6) 差異筆記

這份筆記旨在比較目前專案所使用的 **.NET 10** 與工作上可能會遇到的 **.NET 6** (有時被稱為 .NET Core 6，但在第 5 版之後，微軟統一命名為 .NET) 之間的主要差異。

## 1. 命名與版本生命週期
* **.NET 6**: 於 2021 年底發布，為長期支援 (LTS) 版本，但其官方支援已於 2024 年 11 月結束。
* **.NET 10**: 目前專案使用的版本。微軟採用偶數版本為 LTS (長期支援)、奇數版本為 STS (標準支援) 的策略，.NET 10 提供最新的功能與效能最佳化。

## 2. 搭配的 C# 語言版本與語法差異
這是在撰寫程式碼時最有感的差異。值得注意的是，從 .NET 6 (C# 10) 到 .NET 10 (C# 14)，C# 的核心開發體驗已經非常接近，這與退回 .NET Framework 有著天壤之別。

**在 .NET 6 (C# 10) 就已經具備的現代語法（與 .NET Framework 的主要分水嶺）：**
* **Top-level statements (最上層陳述式)**：讓 `Program.cs` 不需要寫 `class Program` 和 `Main` 方法。
* **Global Usings 與 File-scoped namespace**：大幅減少程式碼的縮排與冗餘的 using 宣告。
* **Record 型別**：用來建立不可變資料模型 (DTOs) 非常方便。
* **Switch 運算式與模式比對 (Pattern matching)**：取代傳統冗長的 switch-case。
* **Nullable Reference Types (可為 null 的參考型別)**：有效防範 `NullReferenceException`。

**從 .NET 6 到 .NET 10 (C# 14) 的「錦上添花」：**
這幾年的進化主要是提供更多「語法糖」，讓寫法更簡短：
* **Collection expressions (集合運算式)**：例如 `List<int> list = [1, 2, 3];`，取代傳統的 `new List<int> { ... }`。
* **Primary constructors (主要建構函式)**：直接在類別名稱旁宣告參數，讓依賴注入 (DI) 的寫法更簡潔。
* **Raw string literals (原始字串常值)** (`"""`)：在處理 JSON 或 SQL 等多行字串時不用再費心處理逸出字元。
* **List patterns (串列模式)**：更強大的陣列與集合模式比對。

## 3. 效能與 AOT (提前編譯)
* **.NET 6**: 雖然效能已經比早期的 .NET Core 3.1 提升很多，但 Native AOT 當時還處於實驗性階段。
* **.NET 10**: 效能有巨大飛躍（包括 JIT 編譯器、垃圾回收 GC 的優化）。此外，**Native AOT (原生提前編譯)** 技術已經非常成熟，能夠將應用程式編譯成不依賴 .NET Runtime 的原生執行檔，大幅降低啟動時間和記憶體佔用。

## 4. Minimal APIs (最小型 API)
* **.NET 6**: 首度引入 Minimal APIs，讓開發者能在 `Program.cs` 裡用短短幾行程式碼建立 HTTP 端點，不一定需要龐大的 Controllers。
* **.NET 10**: Minimal APIs 變得更強大，支援更好的路由群組 (Route Groups)、內建的過濾器 (Filters)、更佳的 OpenAPI/Swagger 整合，以及針對 AOT 的深度最佳化。

## 5. Web 開發 (ASP.NET Core)
* **.NET 6**: MVC 與 Web API 的開發模式與現在相似，但 Blazor 當時的伺服器渲染 (SSR) 和 WebAssembly 模式是分開的。
* **.NET 10**: ASP.NET Core 加入了更多現代 Web 開發功能。Blazor 迎來了 "Blazor United"（自 .NET 8 開始），允許在同一個專案中自由混合伺服器端渲染 (SSR)、SignalR 互動模式和 WebAssembly 互動模式。

## 總結：回去維護 .NET 6 專案時的注意事項
1. **語法限制**：將無法使用 `[]` 集合運算式、主要建構函式或 `"""` 多行原始字串，寫法會稍微囉嗦一點。
2. **Program.cs**：.NET 6 雖然也支援頂層語句 (Top-level statements)，但某些架構可能還是用舊版的 `Startup.cs`（如果是從 .NET Core 3.1 升級上來的）。
3. **效能**：執行速度與記憶體使用量可能不如 .NET 10 專案優秀。

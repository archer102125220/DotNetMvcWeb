# .NET 10 vs .NET Framework 差異筆記

這份筆記旨在比較目前專案所使用的 **.NET 10** 與歷史悠久的 **.NET Framework** (通常是指 4.x 版本，如 4.7.2 或 4.8) 之間的主要差異。這兩者的跨度非常大，幾乎可以視為不同的生態系統。

## 1. 跨平台能力與底層架構
* **.NET Framework**: 專為 Windows 設計，與 Windows 作業系統和 IIS 伺服器深度綁定。
* **.NET 10**: 完全跨平台，可以在 Windows、Linux、macOS 上運行。內建高效能的 Kestrel 伺服器，可以獨立執行或放在 Nginx、Apache、IIS 後面作為反向代理。

## 2. 專案結構與設定檔
* **.NET Framework**:
  * 高度依賴 `Web.config` 來處理所有的應用程式設定、連線字串和伺服器組態。
  * 應用程式生命週期通常在 `Global.asax` 中管理。
  * 套件管理常使用 `packages.config` (較舊的 NuGet 模式)。
* **.NET 10**:
  * 拋棄了 `Web.config`，改用輕量且結構化的 `appsettings.json` 與環境變數 (Environment Variables)。
  * 應用程式入口統一為 `Program.cs`，設定更為簡潔。
  * 套件依賴直接寫在 `.csproj` 檔案中 (PackageReference)。

## 3. 開發工具與命令列 (CLI)
* **.NET Framework**:
  * 幾乎沒有統一的命令列工具，高度綁定 **Visual Studio** (Windows 版)。
  * 建立專案、執行專案通常都必須依賴 Visual Studio 的 UI 介面與內建的 IIS Express。
  * 若要使用指令，極度碎片化（例如編譯用 `msbuild`、套件用 `nuget.exe`）。
* **.NET 10**:
  * 擁有一統天下的 **`dotnet` CLI** (`dotnet new`, `dotnet build`, `dotnet run`, `dotnet add package`)。
  * 終端機是一等公民，使用 VS Code 或是任何純文字編輯器就能在 Mac/Linux 上順暢開發。

## 4. Web 開發架構 (ASP.NET vs ASP.NET Core)
* **.NET Framework (ASP.NET)**:
  * 包含舊式的 Web Forms (以事件驅動的網頁開發) 以及 MVC 5。
  * 核心依賴於 `System.Web` 命名空間，極度肥大且難以單元測試。
* **.NET 10 (ASP.NET Core)**:
  * 完全重寫的 Web 框架，不再有 Web Forms (微軟建議改用 Blazor)。
  * 徹底解除了對 `System.Web` 的依賴，採用了管線 (Middleware Pipeline) 架構，非常輕量、模組化且易於測試。

## 5. 相依性注入 (Dependency Injection, DI)
* **.NET Framework**: 沒有內建強大的 DI 容器，通常需要依賴第三方套件（如 Autofac、Ninject、Unity）來實現控制反轉 (IoC)。
* **.NET 10**: 框架本身內建了原生的相依性注入機制 (DI 容器)，整個 ASP.NET Core 從底層就依賴 DI 運作，使用上非常直覺且標準化。

## 6. 資料庫存取 (EF6 vs EF Core)
* **.NET Framework**: 搭配 Entity Framework 6 (EF6)。支援 EDMX 視覺化設計工具 (Database First 產生的 .edmx 檔)。
* **.NET 10**: 搭配 Entity Framework Core (EF Core)。
  * 也是完全重寫的 ORM，效能大幅提升。
  * 移除了 EDMX 視覺化介面，全面擁抱 Code-First 或是透過 CLI 反向工程生成程式碼 (Scaffold-DbContext)。

## 7. C# 語言版本差異 (C# 7.3 vs C# 14)
這是一個極大的鴻溝，退回 .NET Framework 代表將失去過去 6 年多來 C# 新增的所有便利語法。以下列出在 .NET Framework 中**無法使用**，且感受最深的功能與寫法差異：

### 1. 命名空間與 using 宣告 (C# 10+)
* **.NET 10**: 支援 `global using` 和檔案範圍命名空間。
  ```csharp
  namespace MyProject; // 不需縮排
  ```
* **.NET Framework**: 必須每個檔案都寫一堆 using，且全部程式碼都要包在大括號內縮排。
  ```csharp
  namespace MyProject 
  {
      // 所有程式碼都要退一格
  }
  ```

### 2. Switch 運算式 (C# 8+)
* **.NET 10**: 可以寫出極簡的賦值邏輯：
  ```csharp
  var result = status switch {
      1 => "OK",
      2 => "Error",
      _ => "Unknown"
  };
  ```
* **.NET Framework**: 只能使用傳統冗長的 `switch-case`，且必須搭配 `break;` 或 `return;`。

### 3. Record 型別與 Init-only 屬性 (C# 9+)
* **.NET 10**: 宣告不可變 (Immutable) 的 DTO 只要一行：
  ```csharp
  public record UserDto(string Name, int Age);
  ```
* **.NET Framework**: 必須寫完整的 `class`，手動宣告唯讀屬性、寫建構函式，並自己實作 `Equals()` 如果需要比對值。

### 4. using 宣告式 (C# 8+)
* **.NET 10**: 不需要大括號，變數離開 scope 自動 Dispose。
  ```csharp
  using var stream = new MemoryStream(); 
  // 繼續寫邏輯...
  ```
* **.NET Framework**: 必須用大括號包住生命週期，容易造成深層巢狀（波動拳）。
  ```csharp
  using (var stream = new MemoryStream()) 
  {
      // 邏輯寫在這裡...
  }
  ```

### 5. 索引與範圍運算子 (Index and Range) (C# 8+)
* **.NET 10**: 取得陣列最後一個元素或切片極度簡單：
  ```csharp
  var last = arr[^1];      // 取最後一個
  var slice = arr[1..3];   // 取 index 1 到 2
  ```
* **.NET Framework**: 必須寫 `arr[arr.Length - 1]` 或是用 `LINQ` 的 `Skip().Take()` 等方法。

### 6. Nullable Reference Types (可為 Null 的參考型別) (C# 8+)
* **.NET 10**: 可以在 `.csproj` 開啟 `<Nullable>enable</Nullable>`，編譯器會嚴格檢查 `string?` 與 `string`，大幅減少 runtime 時的 `NullReferenceException`。
* **.NET Framework**: 所有參考型別預設都可以是 null，全憑開發者經驗手動加上 `if (obj != null)` 檢查，編譯器不會幫忙。

### 7. 其他族繁不及備載的現代語法
* **最上層陳述式 (Top-level statements)**：`Program.cs` 必須規規矩矩寫 `class Program { static void Main() { ... } }`。
* **集合運算式與字串常值**：無法使用 `[1, 2, 3]` 初始化集合，也沒有 `"""` 原始多行字串。
* **模式比對 (Pattern matching)**：無法使用 `is { Property: > 10 }` 等強大邏輯比對。
* **非同步串流**：沒有 `IAsyncEnumerable<T>` 和 `await foreach`。

## 總結：維護 .NET Framework 專案時的注意事項
1. **找不到 `appsettings.json`**：需要習慣去解析 XML 格式的 `Web.config`。
2. **語法受限**：習慣了現代 C# 的語法糖後，寫舊版 C# 7.3 會有「綁手綁腳」的感覺，很多功能需要寫得比較冗長。
3. **沒有原生的 DI**：要注意專案中是如何管理實例生命週期的，可能是手動 `new` 或是透過自訂的 Service Locator/IoC 容器。
4. **只能在 Windows 上開發與部署**：必須依賴 IIS 或 IIS Express。
5. **高度依賴 Visual Studio**：失去 `dotnet` CLI，無法單純用 VS Code 加上終端機指令就輕鬆完成所有開發工作。

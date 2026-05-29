# DotNetMvcWeb

這是一個用於學習目的的 .NET 10 MVC (Model-View-Controller) 專案。

## 專案環境
- **框架**: .NET 10
- **開發工具**: 可使用 Visual Studio, Visual Studio Code 或 JetBrains Rider 等 IDE 進行開發。

## 如何啟動專案

您可以使用終端機 (Terminal) 透過 .NET CLI 啟動本專案。請確保您已安裝相應版本的 .NET SDK。

1. **進入專案目錄**:
   ```bash
   cd DotNetMvcWeb
   ```

2. **還原 NuGet 套件**:
   ```bash
   dotnet restore
   ```

3. **啟動專案**:
   - **一般執行模式：**
   ```bash
   dotnet run
   ```

   - **開發者模式 (熱重載 Hot Reload)：**
   （推薦使用此模式，當你修改程式碼並存檔時，API 伺服器會自動重新載入，無須手動重啟）
   ```bash
   dotnet watch run
   ```

4. **瀏覽網站**:
   專案啟動後，開啟終端機中提示的網址 (通常為 `http://localhost:5xxx` 或 `https://localhost:7xxx`) 進行瀏覽。

## 跨平台 IDE 開發指南

如果您習慣使用 Visual Studio (Windows) 或其他全功能整合開發環境 (IDE) 來開啟此專案：
- 請直接使用 IDE 開啟專案資料夾，或透過開啟 `DotNetMvcWeb.csproj` 載入專案。
- 專案已內建基礎的執行設定檔 (位在 `Properties/launchSettings.json` 內)，您可以選擇透過 IIS Express（若為 Windows）或是預設的 Kestrel 伺服器來啟動應用程式。

## 如何從頭建立此專案

若您想了解本專案是如何從零開始建立的，以下是使用 .NET CLI 的建立指令紀錄：

### 1. 建立 MVC 專案
在終端機中，執行以下指令以建立一個名為 `DotNetMvcWeb` 的 MVC 專案：
```bash
dotnet new mvc -n DotNetMvcWeb
```
*(備註：`-n` 參數用來指定專案名稱)*

### 2. 建立 .gitignore 檔案
為了避免將編譯過程產生的暫存檔 (如 `bin/`, `obj/`) 或本機設定檔加入版本控制，專案建立完成並進入專案目錄後，可使用以下指令產生官方標準的 `.gitignore` 範本：
```bash
cd DotNetMvcWeb
dotnet new gitignore
```

## 架構概念：MVC 與 Razor Pages 的差異

在 .NET 網頁開發中，常見的兩種架構模式為 **MVC (Model-View-Controller)** 與 **Razor Pages**：

- **Razor Pages**:
  - **以「頁面」為中心 (Page-Focused)**：每個頁面 (Page) 都有對應的後端程式碼 (PageModel)，職責清晰且檔案結構集中。
  - **適合場景**：適合大部分標準的 Web 應用程式、簡單的表單處理、以及資料流較為單純的情境。

- **MVC (Model-View-Controller)**:
  - **職責分離 (Separation of Concerns)**：將應用程式嚴格劃分為模型 (資料與商業邏輯)、視圖 (UI 呈現) 及控制器 (處理請求並溝通 Model 與 View)。
  - **適合場景**：
    1. **大型且複雜的應用程式**：當專案規模龐大，需要嚴謹的架構來劃分各組件時。
    2. **高度客製化或複雜的路由需求**：MVC 支援非常彈性的路由設計。
    3. **已有明確分工的開發團隊**：前端與後端開發者可以各自專注於 View 與 Controller/Model 的開發，互不干擾。

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
   ```bash
   dotnet run
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

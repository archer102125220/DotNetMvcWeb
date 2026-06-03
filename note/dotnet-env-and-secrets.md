# .NET (ASP.NET Core) 環境變數與機密管理筆記

在 .NET (特別是 ASP.NET Core) 中，管理環境變數和設定檔的機制比單純的 `.env` 檔更強大且結構化。官方有一套非常成熟且推薦的標準做法。

## 1. 主要的設定檔管理方式

### appsettings.json (最主要的設定檔)
這是 .NET 專案中最核心的設定檔，相當於把 `.env` 的內容結構化成 JSON 格式。
* **分層機制**：可以建立 `appsettings.json` (全環境共用)、`appsettings.Development.json` (開發環境專用)、`appsettings.Production.json` (正式環境專用)。
* .NET 會根據環境變數自動載入對應檔案，**後載入的設定會覆蓋先載入的設定**。

### Properties/launchSettings.json (本機開發的環境變數)
這個檔案**只在本地開發時生效** (不會發佈到正式環境)。如果有只想在本地設定的環境變數，通常會寫在這裡的 `environmentVariables` 區塊：
```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development",
  "MY_CUSTOM_VAR": "SomeValue"
}
```

## 2. 不想上傳到 GitHub 的敏感資訊：User Secrets (機密管理員)

在 Node.js 中，通常會把 `.env` 加到 `.gitignore` 以防密碼外洩。而在 .NET 中，最標準且安全的做法是使用 **User Secrets**。

User Secrets 會把設定檔存在電腦系統的**個人資料夾**中，完全與專案目錄結構分離，因此**絕對不可能被 commit 到 Git**。

### User Secrets 存在電腦的哪裡？
根據不同的作業系統，User Secrets 會被儲存在不同的實體路徑下：

* **Windows**:
  `%APPDATA%\Microsoft\UserSecrets\<你的專案機密ID>\secrets.json`
* **Mac / Linux**:
  `~/.microsoft/usersecrets/<你的專案機密ID>/secrets.json`

## 3. User Secrets 操作教學 (CLI)

我們通常透過終端機 (`dotnet` CLI) 來管理，完全不需要手動尋找或編輯隱藏檔案。

**步驟 1：初始化專案的機密管理 (只要做一次)**
在你的專案目錄下（與 `.csproj` 同一層）執行：
```bash
dotnet user-secrets init
```
這會在 `.csproj` 檔案中加入一個 `UserSecretsId` (一段隨機的 GUID)，用來將你的專案與本機的 `secrets.json` 綁定。這串 ID 上傳到 GitHub 是安全的。

**步驟 2：設定機密資料**
假設要儲存資料庫密碼：
```bash
dotnet user-secrets set "Database:Password" "MySuperSecretPassword123"
```

**步驟 3：在程式碼中讀取**
這與從 `appsettings.json` 讀取的方式完全相同。在本地開發環境啟動時，.NET 會自動將 User Secrets 合併進去：
```csharp
var dbPassword = builder.Configuration["Database:Password"];
```

### 其他常用指令
* **列出專案的所有機密**：`dotnet user-secrets list`
* **移除特定機密**：`dotnet user-secrets remove "Database:Password"`
* **清除專案所有的機密**：`dotnet user-secrets clear`

## 4. Visual Studio (GUI) 操作方式

如果你使用的是強大的 IDE，如 **Visual Studio** 或 **JetBrains Rider**，你可以完全不需要記上面的 CLI 指令：

1. 在方案總管 (Solution Explorer) 中，**對著你的專案點擊右鍵**。
2. 選擇 **「管理使用者機密」(Manage User Secrets)**。
3. IDE 會自動幫你執行綁定，並直接在編輯器中打開那個隱藏的 `secrets.json` 檔案。
4. 你可以直接在裡面像寫一般 JSON 檔一樣填入機密，存檔後立刻生效！

## 5. 總結比較：Node.js vs .NET

| 需求情境 | Node.js 常用做法 | .NET 官方推薦做法 |
| :--- | :--- | :--- |
| **全域/預設設定** | 硬編碼或 `.env.example` | `appsettings.json` |
| **依環境不同的設定** | `.env.development`, `.env.production` | `appsettings.Development.json` |
| **本地開發環境變數** | `.env` | `launchSettings.json` 的 `environmentVariables` |
| **本地開發的機密(密碼/Key)** | 把 `.env` 加到 `.gitignore` | **User Secrets** (機密管理員) |
| **正式環境部署的變數** | 系統環境變數 / Docker Env | 系統環境變數 (`__` 階層命名) / Azure Key Vault 等雲端金鑰管理 |

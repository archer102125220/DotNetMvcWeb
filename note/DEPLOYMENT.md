# 部署說明教學 (Deployment Guide)

這份文件說明了如何將 .NET Core MVC 應用程式部署到正式環境 (Production)。部署過程通常包含：發佈 (Publish)、環境變數設定，以及選擇合適的託管 (Hosting) 環境。

## 1. 專案發佈 (Publish)

在部署之前，必須先將專案編譯並打包成可用於部署的格式。使用 .NET CLI 進行發佈：

```bash
# 進入專案目錄
cd /path/to/DotNetMvcWeb

# 發佈專案到指定的目錄 (例如: ./publish)
# -c Release: 指定建置組態為 Release (效能最佳化)
# -o ./publish: 指定輸出目錄
dotnet publish -c Release -o ./publish
```

發佈後，`./publish` 目錄下會包含所有執行應用程式所需的檔案（包含 `.dll`、`appsettings.json`、`wwwroot` 等）。

## 2. 環境變數設定

在正式環境中，強烈建議將環境變數 `ASPNETCORE_ENVIRONMENT` 設為 `Production`。這會影響應用程式的行為（例如：關閉開發人員例外狀況網頁，改用正式的錯誤處理機制，並載入 `appsettings.Production.json`）。

- **Linux (Bash):** `export ASPNETCORE_ENVIRONMENT=Production`
- **Windows (PowerShell):** `$Env:ASPNETCORE_ENVIRONMENT = "Production"`
- **Windows (CMD):** `set ASPNETCORE_ENVIRONMENT=Production`

## 3. 部署策略 (Deployment Strategies)

您可以根據基礎設施選擇合適的部署方式：

### 選項 A: 使用 Docker 部署 (推薦)

容器化部署提供了一致的執行環境，非常適合微服務架構或雲端部署。

**Dockerfile 範例:**
```dockerfile
# 基礎映像檔 (包含 .NET 執行環境)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 將 publish 資料夾的內容複製到容器內
COPY ./publish/ .

# 設定環境變數
ENV ASPNETCORE_ENVIRONMENT=Production

# 啟動應用程式
ENTRYPOINT ["dotnet", "DotNetMvcWeb.dll"]
```

**建置與執行:**
```bash
# 建置 Docker Image
docker build -t dotnet-mvc-web:latest -f Dockerfile .

# 執行 Docker Container
docker run -d -p 8080:8080 --name my-mvc-app dotnet-mvc-web:latest
```

### 選項 B: 部署到 Linux (使用 Nginx + Kestrel)

在 Linux 環境中，通常會使用反向代理伺服器（如 Nginx 或 Apache）來將 HTTP 流量轉發給 .NET 內建的 Kestrel 伺服器。

1. **將發佈的檔案複製到 Linux 伺服器** (例如 `/var/www/dotnetmvcweb`)。
2. **建立 Systemd 服務 (Service) 以確保應用程式保持運行:**
   建立 `/etc/systemd/system/dotnetmvcweb.service`
   ```ini
   [Unit]
   Description=DotNet MVC Web App running on Linux

   [Service]
   WorkingDirectory=/var/www/dotnetmvcweb
   ExecStart=/usr/bin/dotnet /var/www/dotnetmvcweb/DotNetMvcWeb.dll
   Restart=always
   # Restart service after 10 seconds if the dotnet service crashes:
   RestartSec=10
   KillSignal=SIGINT
   SyslogIdentifier=dotnetmvcweb
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

   [Install]
   WantedBy=multi-user.target
   ```
   啟用並啟動服務：`sudo systemctl enable dotnetmvcweb.service` 及 `sudo systemctl start dotnetmvcweb.service`
3. **設定 Nginx 反向代理:**
   建立或修改站台設定檔：
   ```nginx
   server {
       listen 80;
       server_name example.com;

       location / {
           proxy_pass         http://localhost:5000; # 預設 Kestrel port
           proxy_http_version 1.1;
           proxy_set_header   Upgrade $http_upgrade;
           proxy_set_header   Connection keep-alive;
           proxy_set_header   Host $host;
           proxy_cache_bypass $http_upgrade;
           proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header   X-Forwarded-Proto $scheme;
       }
   }
   ```

### 選項 C: 部署到 Windows (使用 IIS)

如果您在 Windows Server 上運行，IIS 是一個常見的選擇。

1. 確保伺服器已安裝 **.NET Core Hosting Bundle**。
2. 在 IIS 中建立一個新的網站。
3. 將網站的實體路徑指向您的 `./publish` 資料夾。
4. 設定應用程式集區 (Application Pool) 的「.NET CLR 版本」為 **「沒有受控程式碼」 (No Managed Code)**，因為 IIS 只負責反向代理，實際執行由 Kestrel 處理。

## 4. 常見問題與檢查清單

* **資料庫連線字串:** 確保 `appsettings.Production.json` 或環境變數中的連線字串是指向正式環境的資料庫。
* **SSL/HTTPS:** 在正式環境中，強烈建議使用 HTTPS。可以在 Nginx 或 IIS 層級設定憑證。
* **靜態檔案:** MVC 專案包含 CSS, JS 等靜態檔案。確保發佈後的 `/wwwroot` 資料夾內容完整，且 `Program.cs` 中有調用 `app.UseStaticFiles()`。
* **日誌紀錄 (Logging):** 設定適當的 Log 等級 (例如 Warning 或 Error) 並將 Log 輸出到檔案或集中式 Log 管理系統，避免填滿硬碟。

> [!IMPORTANT]
> 部署前請務必確保所有的機密資訊（如 API Keys, 密碼）都已從程式碼中移除，並改用環境變數或安全的金鑰管理服務（如 Azure Key Vault, AWS Secrets Manager）來讀取。

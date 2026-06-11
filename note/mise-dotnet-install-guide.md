# 使用 mise 安裝與管理 .NET 環境指南

[mise](https://mise.jdx.dev/) (前身為 rtx) 是一個跨平台的開發環境與版本管理工具，類似於 `asdf` 但使用 Rust 開發，具有更快的執行速度。本指南將介紹如何使用 `mise` 來安裝及管理專案所需的 .NET SDK。

## 1. 安裝 mise

首先需要在系統上安裝 `mise`。

### macOS (使用 Homebrew)
```bash
brew install mise
```

### Windows, Linux 及其他安裝方式
請參考官方安裝指南：[https://mise.jdx.dev/getting-started.html](https://mise.jdx.dev/getting-started.html)

---

## 2. 將 mise 載入 Shell 環境

安裝完成後，必須將 `mise` 載入到目前的 Shell 中（如 Zsh, Bash 或 Fish），這樣才能使 `mise` 安裝的工具生效。

以 `zsh` 為例，將以下內容加入你的 `~/.zshrc`：
```bash
eval "$(mise activate zsh)"
```

重新載入設定檔：
```bash
source ~/.zshrc
```

*註：其他 Shell 的設定方式可以透過 `mise activate --help` 查詢。*

---

## 3. 安裝 .NET SDK

`mise` 提供了非常簡單的語法來安裝與切換不同版本的 .NET。

### 查詢可用的 .NET 版本
你可以透過以下指令列出目前可安裝的 .NET 版本：
```bash
mise ls-remote dotnet
```

### 安裝特定版本
例如，安裝 .NET 8.0 的最新版本：
```bash
mise install dotnet@8.0
```

你也可以安裝特定的完整版號：
```bash
mise install dotnet@8.0.204
```

---

## 4. 設定與切換版本

安裝完版本後，你可以設定全域 (Global) 或專案級別 (Local) 的預設版本。

### 設定全域版本
如果你希望系統預設使用 .NET 8.0：
```bash
mise use --global dotnet@8.0
```
這會在你的家目錄產生或修改 `~/.config/mise/config.toml`。

### 設定專案專用版本 (推薦)
在專案根目錄下執行以下指令，可以將該目錄的 .NET 版本鎖定為 8.0：
```bash
mise use dotnet@8.0
```
這個指令會在目前的目錄產生一個 `.mise.toml` 檔案，當你進入這個資料夾時，`mise` 會自動幫你切換到這個設定的 .NET 版本。

### 檢查目前生效的版本
```bash
mise current dotnet
```
或者是查看所有由 `mise` 管理的工具目前的狀態：
```bash
mise ls
```

---

## 5. 與 `.tool-versions` 相容

如果你或是團隊之前是使用 `asdf`，並且專案中已經存在 `.tool-versions` 檔案，**`mise` 預設就支援讀取 `.tool-versions`**！

你只需要在專案目錄下直接執行：
```bash
mise install
```
`mise` 就會自動根據 `.tool-versions` 檔案內的指定版本，將所有的工具與 SDK 下載並安裝。

---

## 常見問題與除錯

1. **指令找不到 (`dotnet: command not found`)**
   - 確認是否已經將 `mise activate` 加入到你的 shell 設定檔 (如 `~/.zshrc`) 中。
   - 確認是否有針對該目錄設定 `mise use`，或使用全域設定 `mise use -g`。

2. **強制重新下載或更新**
   - 移除特定版本後再重新安裝：
     ```bash
     mise uninstall dotnet@8.0
     mise install dotnet@8.0
     ```

## 參考資料
- [mise 官方文件](https://mise.jdx.dev/)
- [mise 支援的語言與工具清單 (Registry)](https://mise.jdx.dev/registry.html)

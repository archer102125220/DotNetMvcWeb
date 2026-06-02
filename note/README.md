# DotNetMvcWeb 筆記與文件導覽 (Documentation Index)

這是一份針對 `DotNetMvcWeb` 專案中各種筆記、開發指南與文件說明的導覽入口，您可以透過下方的分類快速跳轉到您需要的文件。

## 📚 開發規範與指南 (Development Guidelines)
這部分包含了專案的核心架構以及團隊開發所需要遵循的規範。

- 📜 **[AI 開發指南 / 規範 (AI_CODING_GUIDELINES.md)](./AI_CODING_GUIDELINES.md)**
  - 給 AI 以及開發者的程式碼撰寫與最佳實踐準則。
- 🏗️ **[專案架構說明 (PROJECT_STRUCTURE.md)](./PROJECT_STRUCTURE.md)**
  - 介紹 MVC 專案的目錄結構及各資料夾的作用。
- 📐 **[Models 開發規範與指南 (MODELS_GUIDELINE.md)](./MODELS_GUIDELINE.md)**
  - 包含 Entity, ViewModel, DTO 等 Models 的設計與驗證規範。

## ⚙️ 環境與部署 (Environment & Deployment)
幫助您在不同的作業系統或容器環境中順利把專案跑起來。

- 🍏 **[Mac 下使用 asdf 安裝 .NET 環境指南 (ASDF_DOTNET_INSTALL_MAC.md)](./ASDF_DOTNET_INSTALL_MAC.md)**
  - 在 macOS 上使用 `asdf` 版本控制工具安裝 .NET SDK 的步驟。
- 🐳 **[Docker 環境設置 (DOCKER_ENVIRONMENT.md)](./DOCKER_ENVIRONMENT.md)**
  - 如何使用 Docker 運行本專案與周邊服務。
- 🚀 **[部署指南 (DEPLOYMENT.md)](./DEPLOYMENT.md)**
  - 應用程式發布與部署至正式/測試環境的說明。

## 🗄️ 資料庫與 ORM (Database & ORM)
記錄了各種資料庫系統（包含 Oracle）以及 Entity Framework Core 相關的教學與操作指令。

- 🐘 **[EF Core ORM 開發指南 (ef-core-orm-guide.md)](./ef-core-orm-guide.md)**
  - Entity Framework Core 的整合、Migration 使用方式以及查詢最佳實踐。
- ⚖️ **[ORM 架構比較 (orm-architecture-comparison.md)](./orm-architecture-comparison.md)**
  - 詳細對比了 Entity Framework Core 與 Dapper 的差異以及各自的使用情境。
- 🛢️ **[Oracle MVC 整合 Demo 指南 (oracle-mvc-demo-guide.md)](./oracle-mvc-demo-guide.md)**
  - 在 ASP.NET Core MVC 中如何連接與操作 Oracle 資料庫的示範指南。
- ⌨️ **[Oracle 資料庫指令指南 (oracle-database-commands-guide.md)](./oracle-database-commands-guide.md)**
  - Oracle 常用的 SQL 指令、Docker 容器操作及常用語法速查。
- 🎭 **[Mock Database 實作指南 (mock_database_guide.md)](./mock_database_guide.md)**
  - 在不依賴實體資料庫的情況下，使用假資料進行開發的架構實作。

## 🔐 核心功能與實作 (Core Features)
針對特定核心功能的深入說明與實作教學。

- 🔑 **[JWT 身份驗證實作指南 (JWT_AUTHENTICATION.md)](./JWT_AUTHENTICATION.md)**
  - 在本專案中實作 JWT (JSON Web Token) 登入與 API 驗證的說明。

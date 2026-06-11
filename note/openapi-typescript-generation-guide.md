# OpenAPI 與 TypeScript 型別產生指南

本指南介紹如何在使用 **Scalar** (`Scalar.AspNetCore`) 以及 .NET 內建的 `Microsoft.AspNetCore.OpenApi` 的專案中，將 API 規格匯出並產生給前端（如 Node.js, React, Vue）使用的 TypeScript 型別或 SDK。

## 為什麼需要產生 TypeScript 型別？

在現代的前後端分離開發中，確保前端 API 呼叫與後端實際介面保持一致非常重要。透過自動從後端的 OpenAPI 文件產生 TypeScript 型別，我們可以獲得以下好處：
- **強型別保護**：在編譯時期就能發現欄位名稱拼錯或型別不符的問題。
- **自動補全**：IDE (如 VS Code) 能提供精準的屬性提示。
- **免手寫型別**：當後端 API 發生變更時，只需要重新執行產生指令即可同步最新介面。

## 關於 Scalar 與 OpenAPI

本專案使用 **Scalar** 替代傳統的 Swagger UI，提供一個更現代、更漂亮的 API 閱讀介面。但**它底層所依賴的依然是標準的 OpenAPI (JSON) 格式**，這與你以前使用 Swagger 所產出的格式是完全一致的。

在專案執行時（`dotnet run`），.NET 會自動產生一份 OpenAPI 的 JSON 文件（預設路徑通常在 `http://localhost:5015/openapi/v1.json`）。因此，你可以直接使用 npm 生態系中任何支援 OpenAPI 的工具來產生 TypeScript 型別。

---

## 推薦的型別產生工具

以下推薦目前前端與 Node.js 生態中最主流的 3 套開源工具。請確保在執行指令前，你的 .NET 專案已經在背景運行中。

### 1. `openapi-typescript` (最推薦，純型別)

這套非常輕巧，它**只產生 TypeScript 型別** (Types / Interfaces)，不會產生任何多餘的執行期程式碼（Zero runtime）。如果你已經有自己習慣的 fetch / axios 寫法，只想拿到純粹的型別，這是首選。

**執行方式：**

```bash
# 確保 .NET 專案正在執行中，直接透過 URL 抓取並產出 api-types.ts
npx openapi-typescript http://localhost:5015/openapi/v1.json -o ./src/api-types.ts
```

### 2. `@hey-api/openapi-ts` (產生完整的 API Client SDK)

這套是目前非常熱門的現代化產生器。它不僅會幫你產生所有的 TypeScript 型別，還會直接幫你把所有的 API 呼叫方法包裝成一個現成的 SDK，並支援底層使用 fetch 或 axios。

**執行方式：**

```bash
# 安裝套件
npm install @hey-api/openapi-ts -D

# 產生 Client
npx @hey-api/openapi-ts -i http://localhost:5015/openapi/v1.json -o ./src/client
```

### 3. `orval` (適合有使用 React Query / Vue Query 的前端)

如果你的前端專案有使用 `@tanstack/react-query` 或 Vue Query，Orval 可以直接幫你把所有的 API 連同對應的 Query Hooks 都一起產生出來，超級方便。

**執行方式：**

```bash
npx orval --input http://localhost:5015/openapi/v1.json --output ./src/api
```

---

## 總結

不用擔心專案從 Swagger 換成 Scalar 會失去產生 TypeScript 型別的能力。只要是標準的 OpenAPI 生態，所有原本能用的 npm 產生工具在這個專案上都能無縫繼續使用！只要取得 `openapi.json` 的網址，你就能自由選擇最適合你前端框架的工具。

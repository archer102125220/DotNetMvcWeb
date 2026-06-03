# 使用者體驗優化 (UX Optimization) 指南

這份文件記錄了專案中針對使用者體驗（UX）所做的各項優化與實踐，特別是在 MVC 架構下模擬 SPA（Single Page Application）操作體驗的技巧。

## 核心精神與設計準則 (Core Principles)

為了提供真正優良且符合 Web 精神的使用者體驗，在進行任何 UI/UX 開發時，應將以下幾個核心精神納入考量：

### 1. URL 是唯一的真相來源 (Single Source of Truth)
在現代前端框架盛行的現在，開發者很容易過度依賴「記憶體狀態」來隱藏/顯示畫面，導致**畫面重整後狀態全部遺失、網址無法複製分享**的嚴重反模式（Anti-Pattern）。本專案嚴格要求：
- **狀態網址化**：任何「會改變使用者看到什麼內容」的狀態（例如：搜尋關鍵字、分頁、開啟的表單），都必須同步到 URL 中。
- **重整狀態不遺失**：使用者重新整理或是分享網址時，看到的畫面與狀態必須與當下完全一致。
- **善用歷史推送**：在發送 AJAX 請求替換局部畫面時，必須搭配 `hx-push-url="true"`，讓瀏覽器的歷史紀錄能與畫面同步，完美支援上一/下一頁。

### 2. 即時的視覺回饋 (Instant Visual Feedback)
使用者在進行任何操作（點擊、搜尋、送出）時，系統不應毫無反應。必須透過全域載入條、按鈕反灰或骨架屏 (Skeleton) 等方式，明確告知系統正在處理中，以減輕等待焦慮。

### 3. 漸進式增強 (Progressive Enhancement)
利用 HTMX 提供如同 SPA 般的流暢局部刷新（免整頁重整）體驗。但同時必須確保，若使用者透過直接輸入網址或重新整理頁面存取特定狀態（例如直接進入編輯頁），系統後端也能直接回傳包含該狀態的**完整頁面結構**，確保任何存取情境都不會破版。

---

## 1. 全域載入動畫 (Global Loading Bar)

### 目的
在使用 HTMX 進行局部渲染或頁面跳轉時，為了減少使用者的等待焦慮，並提供明確的系統回饋，我們在畫面頂部加入了一條類似 SPA 常見的漸層無限滑動載入條（類似 NProgress）。

### 實作方式

利用 HTMX 提供的生命週期與狀態類別（State Classes），我們可以在**完全不寫 JavaScript** 的情況下完成。

#### 步驟 1：Layout 結構修改
在 `_Layout.cshtml` 的 `<body>` 加上 `hx-indicator` 屬性，並在其內部放置 `div#global-loading-bar`。
這利用了 HTMX 的**繼承性**，所有在 body 內的 HTMX 請求都會預設觸發這個 indicator。

```html
<body hx-indicator="#global-loading-bar">
    <!-- 全域 Loading Bar -->
    <div id="global-loading-bar" class="htmx-indicator"></div>
    
    <!-- 頁面內容 -->
</body>
```

#### 步驟 2：CSS 動畫設計
在 `site.css` 中定義動畫。HTMX 的 `.htmx-indicator` 預設透明度為 0。當發出請求時，HTMX 會動態加上 `.htmx-request` 類別，此時將透明度設為 1 顯示。

```css
#global-loading-bar {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 4px;
    z-index: 9999;
    /* 藍色漸層背景搭配 background-position 來做無限滑動動畫 */
    background: linear-gradient(90deg, transparent, #0d6efd, transparent);
    background-size: 200% 100%;
    animation: loadingBarAnim 1.2s infinite linear;
    
    /* 預設隱藏並加上淡入淡出過渡效果 */
    opacity: 0;
    pointer-events: none;
    transition: opacity 0.2s ease-in-out;
}

/* 當 HTMX 發出請求時顯示 */
#global-loading-bar.htmx-request {
    opacity: 1;
}

@keyframes loadingBarAnim {
    0% { background-position: 100% 0; }
    100% { background-position: -100% 0; }
}
```

### 優點
1. **宣告式設計**：不需要手動綁定事件或操作 DOM。
2. **自動管理**：請求結束後 HTMX 自動移除類別，進度條優雅消失。
3. **極致輕量**：純 HTML 與 CSS 實作，效能消耗趨近於零。

---

## 2. 漸進式增強與 HTMX 視圖載入 (Progressive Enhancement)

### 目的
讓系統在保有 SPA 的流暢局部刷新（免重整）體驗的同時，也能完美處理使用者直接輸入網址或重新整理頁面的情況，達到「重整不破版、狀態不遺失」的目標。我們透過嚴謹的前後端配合，實作出這個機制。

### 實作方式與詳細連動流程

#### 步驟 1：觸發條件與網址同步 (`hx-push-url`)
在前端觸發按鈕（如新增、編輯、搜尋）上，我們不僅使用 `hx-get` 告訴 HTMX 發送非同步請求，更重要地是加上 `hx-push-url="true"`。
這會讓 HTMX 在發出請求時，自動將請求的網址（例如 `/OracleDemo/Edit/5`）推播到瀏覽器的網址列，實現網址狀態同步。

```html
<!-- 【程式碼撰寫與設定解說】前端發動請求 -->
<button hx-get="@Url.Action("Edit", "OracleDemo", new { id = item.Id })"
        hx-push-url="true"
        hx-target="#oracle-demo-form-container"
        hx-swap="innerHTML">
    Edit
</button>
```

#### 步驟 2：後端動態判斷回傳內容 (`HX-Request`)
在 Controller 中，我們透過檢查 Request Header 是否包含 `HX-Request`，來區分這是一個來自 HTMX 的 AJAX 請求，還是來自瀏覽器直接重整的完整請求。

```csharp
// 【程式碼撰寫與設定解說】如果是 HTMX 請求
// 為了節省頻寬與避免畫面重疊，我們只回傳需要更新的局部視圖 (Partial View)。
// 框架會按照慣例到 Views/OracleDemo/ 資料夾下尋找 _CreateOrEdit.cshtml。
if (Request.Headers.ContainsKey("HX-Request"))
{
    return PartialView("_CreateOrEdit", item);
}
```

#### 步驟 3：處理直接重整的情況 (ViewBag 狀態保留)
當使用者直接將網址 `/OracleDemo/Edit/5` 分享給別人或按下 F5 重新整理時，該請求是不會帶有 `HX-Request` 的。如果這時我們只回傳 PartialView，畫面就會只剩下一個表單，周圍的佈局與 CSS 全都消失（破版）。

為了解決這個問題，如果判斷這是一般的完整請求，我們將撈出來的資料塞入 `ViewBag.ActiveItem`，並且**強制回傳整頁的 Index 視圖**。

```csharp
// 【程式碼撰寫與設定解說】一般瀏覽器請求
// 將狀態塞入 ViewBag，由 Index 視圖負責整頁渲染，實現「無縫狀態接軌」。
ViewBag.ActiveItem = item;
ViewBag.IsEdit = true;
return View("Index", await GetItemsAsync(null));
```

#### 步驟 4：主畫面 (Index) 的預先渲染
在主頁面中，我們準備好對應的 `hx-target` 容器，並且判斷如果 `ViewBag.ActiveItem` 有值，代表使用者是直接帶著狀態進來的，我們就在畫面載入時直接把表單畫出來。

```html
<div id="oracle-demo-form-container">
    @if (ViewBag.ActiveItem != null)
    {
        @await Html.PartialAsync("_CreateOrEdit", ViewBag.ActiveItem)
    }
    else
    {
        <p>Select an item to edit...</p>
    }
</div>
```

### 流程總結
1. **路由對應**：前端 `Url.Action("Create", "OracleDemo")` 產生路徑，ASP.NET Core 的路由機制會對應到 Controller 的指定方法。
2. **接收與處理**：Controller 根據 Header 判斷，這是局部更新 (`PartialView`) 還是整頁重整 (`View`)。
3. **替換與渲染**：HTMX 收到 HTML 原始碼後，尋找指定的 `hx-target` 並透過 `hx-swap="innerHTML"` 無縫替換掉原有內容。

透過這套機制，無論是「從列表點擊進入編輯」還是「直接複製貼上編輯頁面網址」，使用者的體驗都會是一致且完美的。

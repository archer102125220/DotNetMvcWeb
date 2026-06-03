# 使用者體驗優化 (UX Optimization) 指南

這份文件記錄了專案中針對使用者體驗（UX）所做的各項優化與實踐，特別是在 MVC 架構下模擬 SPA（Single Page Application）操作體驗的技巧。

## 核心精神：URL 是唯一的真相來源 (Single Source of Truth)

在現代前端框架（如 React, Vue）盛行的現在，開發者很容易陷入「組件化」與「記憶體狀態管理」的舒適圈，依賴前端狀態（State）來隱藏/顯示畫面或處理搜尋條件。這常導致一個嚴重的反模式（Anti-Pattern）：**畫面重整後狀態全部遺失、網址無法複製分享**。

為了提供真正優良且符合 Web 精神的使用者體驗，本專案嚴格遵守以下原則：

1. **狀態網址化**：任何「會改變使用者看到什麼內容」的狀態（例如：搜尋關鍵字 `?q=apple`、分頁 `?page=3`、開啟的頁籤 `?tab=profile`），都必須同步到 URL Query String 中。
2. **重整狀態不遺失**：使用者重新整理頁面，或是將網址分享給他人時，看到的畫面與狀態必須與當下完全一致。
3. **善用 HTMX 歷史推送**：在發送 AJAX 請求替換局部畫面時，必須搭配 `hx-push-url="true"`，讓瀏覽器的歷史紀錄（History API）與網址列能與畫面同步更新，完美支援上一頁/下一頁的功能。

本指南記錄的所有做法，都是基於上述精神，確保我們在提供 SPA 流暢體驗的同時，絕不犧牲掉 Web 平台最核心的優勢。

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

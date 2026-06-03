# ASP.NET Core MVC 路由與視圖渲染機制教學

這份教學筆記專門用來解釋在 .NET MVC 專案中，**前端的操作（網址）是如何精準對應到後端程式碼（Controller），以及後端又是如何決定要顯示哪一個畫面（View）的。**

我們將以 `OracleDemo` 為例，特別是針對「點擊新增按鈕後，表單是如何出現的」這個流程進行深度解剖。

## 1. 路由對應：網址如何找到 Controller？

當我們在 `Index.cshtml` 寫下這段程式碼：
```html
<button hx-get="@Url.Action("Create", "OracleDemo")" ...>
    Create New Item
</button>
```

### 💡 `@Url.Action()` 的作用
`@Url.Action("Action名稱", "Controller名稱")` 是 ASP.NET MVC 提供的 Helper 方法。
在這裡，它會幫你動態產生一段網址字串：`/OracleDemo/Create`。

### 💡 預設路由機制 (Convention Routing)
當瀏覽器（或是 HTMX）對 `/OracleDemo/Create` 發送 HTTP GET 請求時，ASP.NET Core 內建的路由機制會開始工作：
1. **尋找 Controller**：它看到網址的第一段是 `OracleDemo`，就會自動去尋找名字叫做 `OracleDemoController` 的類別（框架規定 Controller 類別必須以 `Controller` 結尾）。
2. **尋找 Action 方法**：它看到網址的第二段是 `Create`，就會進入 `OracleDemoController` 裡面，尋找叫做 `Create()` 的方法。

這就是為什麼我們不需要寫額外的設定檔，只要命名符合規範，網址就能自動對應到後端程式碼的原因。

---

## 2. 回傳視圖：Controller 如何決定畫面？

當程式進入 `OracleDemoController` 的 `Create()` 方法後，程式碼長這樣：

```csharp
public IActionResult Create()
{
    // ... 前面的邏輯 ...
    
    // 關鍵在這一行
    return PartialView("_CreateOrEdit", new OracleDemoItem());
}
```

### 💡 為什麼是 `PartialView()`？
傳統的 `return View()` 會把整個網頁（包含外層的 `_Layout.cshtml` 導覽列、頁尾等）重新渲染一次。
因為我們搭配了 HTMX 來做**局部刷新**，我們只需要「表單本身」的 HTML 片段就好，所以我們使用 `return PartialView()`。

### 💡 視圖的尋找機制 (View Discovery)
當我們寫下 `return PartialView("_CreateOrEdit")` 時，我們是明確告訴 MVC 框架：「我要使用 `_CreateOrEdit` 這個視圖檔案」。

此時，MVC 框架會依照「慣例優於設定 (Convention over Configuration)」的原則，按順序去以下資料夾尋找對應的 `.cshtml` 檔案：
1. **優先尋找專屬資料夾**：`/Views/OracleDemo/_CreateOrEdit.cshtml`（因為這個方法是在 `OracleDemoController` 裡執行的）。
2. **找不到就去共用資料夾**：`/Views/Shared/_CreateOrEdit.cshtml`。

只要檔案存在，框架就會讀取該檔案的內容準備渲染。

---

## 3. 模型綁定：為什麼新增和編輯可以共用一個 View？

在 `Create()` 和 `Edit()` 方法中，我們都呼叫了同一個視圖，但卻能產生不同的效果：

### 🌟 產生「新增」表單
```csharp
// 傳入一個全新、沒有資料的 OracleDemoItem
return PartialView("_CreateOrEdit", new OracleDemoItem());
```
因為傳入的模型 `Id` 是 0（預設值），所以 `_CreateOrEdit.cshtml` 裡面寫的 `@(Model.Id == 0 ? "Create" : "Edit")` 就會顯示 "Create"。同時所有輸入框都是空的。

### 🌟 產生「編輯」表單
```csharp
// 從資料庫撈出舊資料 item 後傳入
var item = await _context.OracleDemoItems.FindAsync(id);
return PartialView("_CreateOrEdit", item);
```
因為傳入的模型 `Id` 有值，視圖就會顯示 "Edit"。而且 `<input asp-for="Name">` 這種寫法（Tag Helpers）會自動把傳入模型裡的 `Name` 值，填入到 HTML 的 `value` 屬性中，這就叫做**模型綁定 (Model Binding)**。

---

## 總結流程

1. 使用者點擊按鈕 ➡️ 觸發 `GET /OracleDemo/Create`。
2. 路由將請求導向 ➡️ `OracleDemoController.cs` 裡的 `Create()` 方法。
3. `Create()` 方法執行 ➡️ 準備一個空的 `OracleDemoItem` 模型。
4. 方法呼叫 `PartialView` ➡️ 框架找到 `Views/OracleDemo/_CreateOrEdit.cshtml`。
5. 結合模型與視圖 ➡️ 產生一段 HTML 表單字串。
6. 回傳給前端 ➡️ HTMX 將這段 HTML 塞入指定區塊，完成顯示！

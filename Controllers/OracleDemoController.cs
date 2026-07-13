using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// Oracle 資料庫示範控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫進行 CRUD (新增、讀取、更新、刪除) 操作
    /// </summary>
    public class OracleDemoController : Controller
    {
        private readonly IOracleDemoItemService _itemService;
        private readonly IOracleDemoCategoryService _categoryService;
        private readonly IConfiguration _configuration;

        // [教學註解] 依賴注入 (Dependency Injection, DI)
        // 這裡不直接注入 DbContext，而是注入定義好的 Service 介面。
        // 這樣可以達到「關注點分離」，讓 Controller 只負責接收請求與回傳結果，商業邏輯交給 Service 處理。
        public OracleDemoController(IOracleDemoItemService itemService, IOracleDemoCategoryService categoryService, IConfiguration configuration)
        {
            _itemService = itemService;
            _categoryService = categoryService;
            _configuration = configuration;
        }

        /// <summary>
        /// GET: /OracleDemo
        /// 顯示主頁面，並載入初始資料列表。若為 HTMX 請求則回傳 PartialView。
        /// </summary>
        public async Task<IActionResult> Index(string? keyword = null)
        {
            List<OracleDemoItem> items = await _itemService.GetItemsAsync(keyword);

            // [教學註解] 漸進式增強 (Progressive Enhancement) 的核心：
            // 透過檢查 Request Header 是否包含 "HX-Request"，我們可以知道這個請求是由 HTMX (AJAX) 發出的，
            // 還是由瀏覽器直接重整 (F5) 或輸入網址發出的。
            // 若為 HTMX 請求，我們只需要回傳清單的部分視圖 (PartialView)，不用回傳整個帶有 Layout 的網頁，節省頻寬。
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_DemoList", items);
            }

            // 若為一般瀏覽器請求 (例如剛進來或重整)，則回傳完整的 View (包含 _Layout)
            ViewBag.Keyword = keyword;
            return View(items);
        }

        /// <summary>
        /// GET: /OracleDemo/Create
        /// 回傳「新增項目」的表單 Partial View，供 HTMX 載入到畫面上。
        /// 
        /// 【程式碼撰寫與設定解說：如何載入 _CreateOrEdit.cshtml】
        /// 1. 路由對應：前端使用 `Url.Action("Create", "OracleDemo")` 會產生 `/OracleDemo/Create` 的網址。
        ///    ASP.NET Core 的預設路由機制會自動找到 `OracleDemoController` 底下名稱為 `Create` 的這個方法。
        /// 2. 回傳視圖：方法最後呼叫了 `PartialView("_CreateOrEdit", new OracleDemoItem())`。
        ///    - 這裡明確指定了要尋找名稱為 `_CreateOrEdit` 的視圖檔案。
        ///    - 框架會按照慣例到 `Views/OracleDemo/` 資料夾下尋找 `_CreateOrEdit.cshtml`。
        ///    - 將一個全新的空 `OracleDemoItem` 模型傳遞給該視圖，以便產生空表單。
        /// </summary>
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropDownListAsync();
            var model = new OracleDemoItem();

            // [教學註解] 若是透過 HTMX 點擊「Create」按鈕進來，只回傳表單的部分 HTML
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            // [教學註解] 漸進式增強 (Progressive Enhancement) - 處理重整問題：
            // 若使用者直接重整 /OracleDemo/Create 網頁（此時不會有 HX-Request header），
            // 我們如果只回傳 PartialView，畫面就會破版 (沒有選單與 CSS)。
            // 因此我們將表單狀態 (model) 放入 ViewBag，然後改為渲染整頁的 "Index" 視圖。
            // 這樣使用者重新整理時，就會看到完整的列表頁面，且左側自動開啟新增表單！
            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await _itemService.GetItemsAsync(null));
        }

        /// <summary>
        /// POST: /OracleDemo/Create
        /// 處理「新增項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> Create([Bind("Name,Description,CategoryId")] OracleDemoItem item)
        {
            if (ModelState.IsValid) // 檢查資料驗證是否通過
            {
                await _itemService.CreateItemAsync(item);
                
                // [教學註解] 狀態網址化 (URL State Sync) - 送出後的還原：
                // 當成功新增後，我們利用 Response Header 告訴 HTMX 去推播 (Push) 一個新網址。
                // 這樣可以把原本是 /OracleDemo/Create 的網址，自動還原回乾淨的 /OracleDemo，
                // 確保使用者如果此時按下 F5，不會不小心又進入 Create 頁面發送 POST 請求。
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemo"));
                
                // 重新呼叫 Index() 取得最新列表並回傳 (因為是 HTMX 請求，Index 會自動回傳 PartialView)
                return await Index();
            }
            
            // 若驗證失敗，指示 HTMX 將錯誤表單重新渲染回表單區塊中
            Response.Headers.Append("HX-Retarget", "#oracle-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            await PopulateCategoriesDropDownListAsync(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /OracleDemo/Edit/5
        /// 根據 ID 回傳「編輯項目」的表單 Partial View，供 HTMX 載入到畫面上。
        /// 
        /// 【程式碼撰寫與設定解說：如何載入 _CreateOrEdit.cshtml 作為編輯用】
        /// 1. 路由對應：前端使用 `Url.Action("Edit", "OracleDemo", new { id = item.Id })` 會產生如 `/OracleDemo/Edit/5` 的網址。
        ///    路由機制會對應到這個 `Edit(int? id)` 方法，並將網址結尾的數字作為 `id` 參數傳入。
        /// 2. 回傳視圖：從資料庫撈出對應的 `item` 資料後，同樣呼叫 `PartialView("_CreateOrEdit", item)`。
        ///    - 這表示「新增」和「編輯」共用了同一個 `.cshtml` 檔案。
        ///    - 視圖內部會根據傳入的模型（`Model.Id == 0` 或有值）來決定顯示「Create」還是「Edit」的標題及行為。
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            OracleDemoItem? item = await _itemService.GetItemByIdAsync(id.Value);
            if (item == null) return NotFound();
            
            await PopulateCategoriesDropDownListAsync(item.CategoryId);

            // [教學註解] 若是點擊列表的 Edit 按鈕 (HTMX 請求)，回傳表單的部分視圖
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", item);
            }

            // [教學註解] 漸進式增強 (Progressive Enhancement)：
            // 處理使用者直接複製 /OracleDemo/Edit/5 貼給別人，或是直接 F5 重整頁面的情況。
            // 把讀取到的 item 塞入 ViewBag，由 Index 視圖做整頁渲染，實現「無縫狀態接軌」。
            ViewBag.ActiveItem = item;
            ViewBag.IsEdit = true;
            return View("Index", await _itemService.GetItemsAsync(null));
        }

        /// <summary>
        /// POST: /OracleDemo/Edit/5
        /// 處理「編輯項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedAt,CategoryId")] OracleDemoItem item)
        {
            if (id != item.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _itemService.UpdateItemAsync(item);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_itemService.ItemExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                // 更新成功，回傳列表 Partial View，並還原網址
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemo"));
                return await Index();
            }
            
            // 驗證失敗，將錯誤表單重新渲染
            Response.Headers.Append("HX-Retarget", "#oracle-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            await PopulateCategoriesDropDownListAsync(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// POST: /OracleDemo/Delete/5
        /// 處理刪除項目的請求 (直接透過 HTMX 發送 POST)
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _itemService.DeleteItemAsync(id);
            
            // 刪除成功後，回傳更新後的列表，並確保網址維持在根目錄
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemo"));
            return await Index();
        }

        /// <summary>
        /// POST: /OracleDemo/UpdateDescriptionViaProcedure/5
        /// 處理更新敘述的請求 (直接透過 HTMX 發送 POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDescriptionViaProcedure(int id, string newDescription)
        {
            if (!string.IsNullOrWhiteSpace(newDescription))
            {
                await _itemService.UpdateItemDescriptionViaProcAsync(id, newDescription);
            }
            
            return await Index();
        }

        private async Task PopulateCategoriesDropDownListAsync(object? selectedCategory = null)
        {
            List<OracleDemoCategory> categories = await _categoryService.GetCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategory);
        }

        /// <summary>
        /// GET: /OracleDemo/AdoNetDemo
        /// 示範如何直接使用 Oracle.ManagedDataAccess.Client 原生 ADO.NET 方式連線與查詢
        /// </summary>
        public async Task<IActionResult> AdoNetDemo(string? keyword = null)
        {
            List<OracleDemoItem> items = await _itemService.GetItemsViaAdoNetAsync(keyword);

            ViewBag.Keyword = keyword;
            // [教學註解] 回傳給具備 UI 畫面的 View，並將剛剛手動組裝好的 List 傳遞給 @model
            return View(items);
        }
    }
}

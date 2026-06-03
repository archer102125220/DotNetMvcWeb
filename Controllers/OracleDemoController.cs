using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// Oracle 資料庫示範控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫進行 CRUD (新增、讀取、更新、刪除) 操作
    /// </summary>
    public class OracleDemoController : Controller
    {
        private readonly AppDbContext _context;

        // 透過依賴注入 (Dependency Injection) 取得資料庫上下文
        public OracleDemoController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /OracleDemo
        /// 顯示主頁面，並載入初始資料列表
        /// </summary>
        public async Task<IActionResult> Index(string? keyword = null)
        {
            List<OracleDemoItem> items = await GetItemsAsync(keyword);
            return View(items);
        }

        /// <summary>
        /// GET: /OracleDemo/List
        /// 專門給 HTMX 呼叫，用來回傳更新後的資料列表 Partial View
        /// </summary>
        public async Task<IActionResult> List(string? keyword = null)
        {
            List<OracleDemoItem> items = await GetItemsAsync(keyword);
            return PartialView("_DemoList", items);
        }

        /// <summary>
        /// 取得資料列表 (支援 Raw SQL 關鍵字搜尋)
        /// </summary>
        private async Task<List<OracleDemoItem>> GetItemsAsync(string? keyword)
        {
            // 如果有輸入關鍵字，就使用原生 SQL 進行查詢
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                // ⚠️ 深度檢查注意：使用 FromSqlInterpolated 會自動進行參數化，安全防止 SQL Injection。
                // 另外，查詢後仍必須加上 .AsNoTracking() 來進行唯讀優化。
                return await _context.OracleDemoItems
                    .FromSqlInterpolated($"SELECT * FROM \"OracleDemoItems\" WHERE \"Name\" LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }

            // 如果沒有關鍵字，就使用一般的 LINQ 查詢全部
            return await _context.OracleDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// GET: /OracleDemo/Create
        /// 【程式碼撰寫與設定解說：如何載入 _CreateOrEdit.cshtml】
        /// 1. 路由對應：前端使用 `Url.Action("Create", "OracleDemo")` 會產生 `/OracleDemo/Create` 的網址。
        ///    ASP.NET Core 的預設路由機制會自動找到 `OracleDemoController` 底下名稱為 `Create` 的這個方法。
        /// 2. 回傳視圖：方法最後呼叫了 `PartialView("_CreateOrEdit", new OracleDemoItem())`。
        ///    - 這裡明確指定了要尋找名稱為 `_CreateOrEdit` 的視圖檔案。
        ///    - 框架會按照慣例到 `Views/OracleDemo/` 資料夾下尋找 `_CreateOrEdit.cshtml`。
        ///    - 將一個全新的空 `OracleDemoItem` 模型傳遞給該視圖，以便產生空表單。
        /// </summary>
        public IActionResult Create()
        {
            PopulateCategoriesDropDownList();
            return PartialView("_CreateOrEdit", new OracleDemoItem());
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
                item.CreatedAt = DateTime.UtcNow;
                _context.Add(item);
                await _context.SaveChangesAsync(); // 非同步寫入資料庫
                
                // 成功後，回傳更新後的列表給 HTMX 進行局部刷新
                return await List();
            }
            
            // 若驗證失敗，指示 HTMX 將錯誤表單重新渲染回表單區塊中
            Response.Headers.Append("HX-Retarget", "#oracle-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /OracleDemo/Edit/5
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

            OracleDemoItem? item = await _context.OracleDemoItems.FindAsync(id);
            if (item == null) return NotFound();
            
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
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
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OracleDemoItemExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                // 更新成功，回傳列表 Partial View
                return await List();
            }
            
            // 驗證失敗，將錯誤表單重新渲染
            Response.Headers.Append("HX-Retarget", "#oracle-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
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
            OracleDemoItem? item = await _context.OracleDemoItems.FindAsync(id);
            if (item != null)
            {
                _context.OracleDemoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            
            // 刪除成功後，回傳更新後的列表
            return await List();
        }

        // 檢查項目是否存在的輔助方法
        private bool OracleDemoItemExists(int id)
        {
            return _context.OracleDemoItems.Any(e => e.Id == id);
        }

        private void PopulateCategoriesDropDownList(object? selectedCategory = null)
        {
            var categoriesQuery = _context.OracleDemoCategories.OrderBy(c => c.Name);
            ViewBag.Categories = new SelectList(categoriesQuery.AsNoTracking(), "Id", "Name", selectedCategory);
        }
    }
}

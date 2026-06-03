using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// Oracle Demo 分類控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫中 OracleDemoCategories 進行 CRUD 操作
    /// </summary>
    public class OracleDemoCategoryController : Controller
    {
        private readonly AppDbContext _context;

        // 透過依賴注入 (Dependency Injection) 取得資料庫上下文
        public OracleDemoCategoryController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /OracleDemoCategory
        /// 顯示分類管理主頁面，並載入初始資料列表
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var items = await GetItemsAsync();

            // [教學註解] 檢查是否為 HTMX (AJAX) 請求，是的話只回傳部分視圖
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CategoryList", items);
            }

            // [教學註解] 一般瀏覽器請求，回傳完整視圖
            return View(items);
        }

        /// <summary>
        /// 取得分類列表，預設依建立時間反向排序
        /// </summary>
        private async Task<List<OracleDemoCategory>> GetItemsAsync()
        {
            // ⚠️ 深度檢查注意：唯讀查詢必須加上 .AsNoTracking() 以節省記憶體並提升效能
            return await _context.OracleDemoCategories
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// GET: /OracleDemoCategory/Create
        /// 回傳「新增分類」的表單 Partial View，供 HTMX 載入到畫面上。
        /// 
        /// 【程式碼撰寫與設定解說：如何載入 _CreateOrEdit.cshtml】
        /// 1. 路由對應：前端使用 `Url.Action("Create", "OracleDemoCategory")` 會產生 `/OracleDemoCategory/Create` 的網址。
        ///    ASP.NET Core 的預設路由機制會自動找到 `OracleDemoCategoryController` 底下名稱為 `Create` 的這個方法。
        /// 2. 回傳視圖：方法最後呼叫了 `PartialView("_CreateOrEdit", new OracleDemoCategory())`。
        ///    - 這裡明確指定了要尋找名稱為 `_CreateOrEdit` 的視圖檔案。
        ///    - 框架會按照慣例到 `Views/OracleDemoCategory/` 資料夾下尋找 `_CreateOrEdit.cshtml`。
        ///    - 將一個全新的空 `OracleDemoCategory` 模型傳遞給該視圖，以便產生空表單。
        /// </summary>
        public async Task<IActionResult> Create()
        {
            var model = new OracleDemoCategory();

            // [教學註解] 若是 HTMX 點擊進來，只回傳表單
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            // [教學註解] 解決重新整理重置版：如果是重整，我們回傳完整的 Index 頁面，
            // 並且把 ActiveItem 透過 ViewBag 帶過去，讓 Index 在載入時自己把表單畫出來。
            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await GetItemsAsync());
        }

        /// <summary>
        /// POST: /OracleDemoCategory/Create
        /// 處理「新增分類」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> Create([Bind("Name")] OracleDemoCategory item)
        {
            if (ModelState.IsValid) // 檢查資料驗證是否通過
            {
                item.CreatedAt = DateTime.UtcNow;
                _context.Add(item);
                await _context.SaveChangesAsync(); // 非同步寫入資料庫
                
                // [教學註解] 透過 Response Header 指示 HTMX 更新瀏覽器的網址列，避免網址停留在 /Create
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemoCategory"));
                return await Index();
            }
            
            // 若驗證失敗，指示 HTMX 將錯誤表單重新渲染回表單區塊中
            Response.Headers.Append("HX-Retarget", "#oracle-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /OracleDemoCategory/Edit/5
        /// 根據 ID 回傳「編輯分類」的表單 Partial View，供 HTMX 載入到畫面上。
        /// 
        /// 【程式碼撰寫與設定解說：如何載入 _CreateOrEdit.cshtml 作為編輯用】
        /// 1. 路由對應：前端使用 `Url.Action("Edit", "OracleDemoCategory", new { id = item.Id })` 會產生如 `/OracleDemoCategory/Edit/5` 的網址。
        ///    路由機制會對應到這個 `Edit(int? id)` 方法，並將網址結尾的數字作為 `id` 參數傳入。
        /// 2. 回傳視圖：從資料庫撈出對應的 `item` 資料後，同樣呼叫 `PartialView("_CreateOrEdit", item)`。
        ///    - 這表示「新增」和「編輯」共用了同一個 `.cshtml` 檔案。
        ///    - 視圖內部會根據傳入的模型（`Model.Id == 0` 或有值）來決定顯示「Create」還是「Edit」的標題及行為。
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.OracleDemoCategories.FindAsync(id);
            if (item == null) return NotFound();
            
            // [教學註解] 若是 HTMX 請求，只回傳表單
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", item);
            }

            // [教學註解] 若是直接輸入網址或重新整理，回傳完整的 Index 頁面，並自動帶入表單內容
            ViewBag.ActiveItem = item;
            ViewBag.IsEdit = true;
            return View("Index", await GetItemsAsync());
        }

        /// <summary>
        /// POST: /OracleDemoCategory/Edit/5
        /// 處理「編輯分類」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedAt")] OracleDemoCategory item)
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
                    if (!OracleDemoCategoryExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                // 更新成功，回傳列表 Partial View
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemoCategory"));
                return await Index();
            }
            
            // 驗證失敗，將錯誤表單重新渲染
            Response.Headers.Append("HX-Retarget", "#oracle-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// POST: /OracleDemoCategory/Delete/5
        /// 處理刪除分類的請求 (直接透過 HTMX 發送 POST)
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.OracleDemoCategories.FindAsync(id);
            if (item != null)
            {
                _context.OracleDemoCategories.Remove(item);
                await _context.SaveChangesAsync();
            }
            
            // 刪除成功後，回傳更新後的列表
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemoCategory"));
            return await Index();
        }

        private bool OracleDemoCategoryExists(int id)
        {
            return _context.OracleDemoCategories.Any(e => e.Id == id);
        }
    }
}

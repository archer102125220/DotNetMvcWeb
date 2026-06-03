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
            return View(items);
        }

        /// <summary>
        /// GET: /OracleDemoCategory/List
        /// 專門給 HTMX 呼叫，用來回傳更新後的分類資料列表 Partial View
        /// </summary>
        public async Task<IActionResult> List()
        {
            var items = await GetItemsAsync();
            return PartialView("_CategoryList", items);
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
        /// 回傳「新增分類」的表單 Partial View，供 HTMX 載入到畫面上
        /// </summary>
        public IActionResult Create()
        {
            return PartialView("_CreateOrEdit", new OracleDemoCategory());
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
                
                // 成功後，回傳更新後的列表給 HTMX 進行局部刷新
                return await List();
            }
            
            // 若驗證失敗，指示 HTMX 將錯誤表單重新渲染回表單區塊中
            Response.Headers.Append("HX-Retarget", "#oracle-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /OracleDemoCategory/Edit/5
        /// 根據 ID 回傳「編輯分類」的表單 Partial View
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.OracleDemoCategories.FindAsync(id);
            if (item == null) return NotFound();
            
            return PartialView("_CreateOrEdit", item);
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
                return await List();
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
            return await List();
        }

        private bool OracleDemoCategoryExists(int id)
        {
            return _context.OracleDemoCategories.Any(e => e.Id == id);
        }
    }
}

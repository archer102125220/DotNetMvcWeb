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
        public async Task<IActionResult> Index()
        {
            // 使用 AsNoTracking() 進行唯讀查詢，提升效能 (Deep Check 規範)
            var items = await _context.OracleDemoItems
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            return View(items);
        }

        /// <summary>
        /// GET: /OracleDemo/List
        /// 專門給 HTMX 呼叫，用來回傳更新後的資料列表 Partial View
        /// </summary>
        public async Task<IActionResult> List()
        {
            var items = await _context.OracleDemoItems
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            return PartialView("_DemoList", items);
        }

        /// <summary>
        /// GET: /OracleDemo/Create
        /// 回傳「新增項目」的表單 Partial View，供 HTMX 載入到畫面上
        /// </summary>
        public IActionResult Create()
        {
            return PartialView("_CreateOrEdit", new OracleDemoItem());
        }

        /// <summary>
        /// POST: /OracleDemo/Create
        /// 處理「新增項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> Create([Bind("Name,Description")] OracleDemoItem item)
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
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /OracleDemo/Edit/5
        /// 根據 ID 回傳「編輯項目」的表單 Partial View
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.OracleDemoItems.FindAsync(id);
            if (item == null) return NotFound();
            
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// POST: /OracleDemo/Edit/5
        /// 處理「編輯項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedAt")] OracleDemoItem item)
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
            var item = await _context.OracleDemoItems.FindAsync(id);
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
    }
}

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
    /// MySQL 資料庫示範控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫進行 CRUD (新增、讀取、更新、刪除) 操作
    /// </summary>
    public class MysqlDemoController : Controller
    {
        private readonly MysqlDbContext _context;

        // 透過依賴注入 (Dependency Injection) 取得資料庫上下文
        public MysqlDemoController(MysqlDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /MysqlDemo
        /// 顯示主頁面，並載入初始資料列表。若為 HTMX 請求則回傳 PartialView。
        /// </summary>
        public async Task<IActionResult> Index(string? keyword = null)
        {
            List<MysqlDemoItem> items = await GetItemsAsync(keyword);

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_DemoList", items);
            }

            ViewBag.Keyword = keyword;
            return View(items);
        }

        /// <summary>
        /// 取得資料列表 (支援 Raw SQL 關鍵字搜尋)
        /// </summary>
        private async Task<List<MysqlDemoItem>> GetItemsAsync(string? keyword)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                // ⚠️ 深度檢查注意：使用 FromSqlInterpolated 會自動進行參數化，安全防止 SQL Injection。
                // 注意 MySQL 表名不一定需要加上雙引號，但為了安全與習慣我們使用反引號 ` 或交由 EF Core
                return await _context.MysqlDemoItems
                    .FromSqlInterpolated($"SELECT * FROM `MysqlDemoItems` WHERE `Name` LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }

            return await _context.MysqlDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// GET: /MysqlDemo/Create
        /// 回傳「新增項目」的表單 Partial View，供 HTMX 載入到畫面上。
        /// </summary>
        public async Task<IActionResult> Create()
        {
            PopulateCategoriesDropDownList();
            var model = new MysqlDemoItem();

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await GetItemsAsync(null));
        }

        /// <summary>
        /// POST: /MysqlDemo/Create
        /// 處理「新增項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,CategoryId")] MysqlDemoItem item)
        {
            if (ModelState.IsValid)
            {
                item.CreatedAt = DateTime.UtcNow;
                _context.Add(item);
                await _context.SaveChangesAsync();
                
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MysqlDemo"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#mysql-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /MysqlDemo/Edit/5
        /// 根據 ID 回傳「編輯項目」的表單 Partial View，供 HTMX 載入到畫面上。
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            MysqlDemoItem? item = await _context.MysqlDemoItems.FindAsync(id);
            if (item == null) return NotFound();
            
            PopulateCategoriesDropDownList(item.CategoryId);

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", item);
            }

            ViewBag.ActiveItem = item;
            ViewBag.IsEdit = true;
            return View("Index", await GetItemsAsync(null));
        }

        /// <summary>
        /// POST: /MysqlDemo/Edit/5
        /// 處理「編輯項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedAt,CategoryId")] MysqlDemoItem item)
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
                    if (!MysqlDemoItemExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MysqlDemo"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#mysql-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// POST: /MysqlDemo/Delete/5
        /// 處理刪除項目的請求 (直接透過 HTMX 發送 POST)
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            MysqlDemoItem? item = await _context.MysqlDemoItems.FindAsync(id);
            if (item != null)
            {
                _context.MysqlDemoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MysqlDemo"));
            return await Index();
        }

        private bool MysqlDemoItemExists(int id)
        {
            return _context.MysqlDemoItems.Any(e => e.Id == id);
        }

        private void PopulateCategoriesDropDownList(object? selectedCategory = null)
        {
            var categoriesQuery = _context.MysqlDemoCategories.OrderBy(c => c.Name);
            ViewBag.Categories = new SelectList(categoriesQuery.AsNoTracking(), "Id", "Name", selectedCategory);
        }
    }
}

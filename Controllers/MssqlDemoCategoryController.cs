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
    /// MSSQL Demo 分類控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫中 MssqlDemoCategories 進行 CRUD 操作
    /// </summary>
    public class MssqlDemoCategoryController : Controller
    {
        private readonly MssqlDbContext _context;

        public MssqlDemoCategoryController(MssqlDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<MssqlDemoCategory> items = await GetItemsAsync();

            // [教學註解] 漸進式增強 (Progressive Enhancement)：若為 HTMX 請求，僅回傳 PartialView 節省頻寬。
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CategoryList", items);
            }

            return View(items);
        }

        private async Task<List<MssqlDemoCategory>> GetItemsAsync()
        {
            return await _context.MssqlDemoCategories
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> Create()
        {
            var model = new MssqlDemoCategory();

            // [教學註解] 若是透過 HTMX 點擊「Create」按鈕進來，只回傳表單的部分 HTML
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            // [教學註解] 漸進式增強：若使用者直接存取 /MssqlDemoCategory/Create，將渲染整頁 Index 並自動帶入新增狀態。
            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await GetItemsAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] MssqlDemoCategory item)
        {
            if (ModelState.IsValid)
            {
                item.CreatedAt = DateTime.UtcNow;
                _context.Add(item);
                await _context.SaveChangesAsync();
                
                // [教學註解] 成功新增後，推播新網址以還原狀態，並回傳更新後的列表。
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemoCategory"));
                return await Index();
            }
            
            // [教學註解] 若驗證失敗，指示 HTMX 將錯誤表單重新渲染回表單區塊中
            Response.Headers.Append("HX-Retarget", "#mssql-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            MssqlDemoCategory? item = await _context.MssqlDemoCategories.FindAsync(id);
            if (item == null) return NotFound();
            
            // [教學註解] 若是透過 HTMX 點擊 Edit，只回傳編輯表單的部分 HTML
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", item);
            }

            // [教學註解] 漸進式增強：直接重整 Edit 網頁時，將渲染整頁 Index 並自動帶入編輯狀態。
            ViewBag.ActiveItem = item;
            ViewBag.IsEdit = true;
            return View("Index", await GetItemsAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedAt")] MssqlDemoCategory item)
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
                    if (!MssqlDemoCategoryExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                // [教學註解] 成功更新後，推播新網址以還原狀態，並回傳更新後的列表。
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemoCategory"));
                return await Index();
            }
            
            // [教學註解] 若驗證失敗，指示 HTMX 將錯誤表單重新渲染回表單區塊中
            Response.Headers.Append("HX-Retarget", "#mssql-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            MssqlDemoCategory? item = await _context.MssqlDemoCategories.FindAsync(id);
            if (item != null)
            {
                _context.MssqlDemoCategories.Remove(item);
                await _context.SaveChangesAsync();
            }
            
            // [教學註解] 刪除成功後，確保網址維持在根目錄
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemoCategory"));
            return await Index();
        }

        private bool MssqlDemoCategoryExists(int id)
        {
            return _context.MssqlDemoCategories.Any(e => e.Id == id);
        }
    }
}

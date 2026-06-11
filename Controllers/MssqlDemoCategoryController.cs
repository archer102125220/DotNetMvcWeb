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
            var items = await GetItemsAsync();

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

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

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
                
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemoCategory"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#mssql-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.MssqlDemoCategories.FindAsync(id);
            if (item == null) return NotFound();
            
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", item);
            }

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
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemoCategory"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#mssql-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.MssqlDemoCategories.FindAsync(id);
            if (item != null)
            {
                _context.MssqlDemoCategories.Remove(item);
                await _context.SaveChangesAsync();
            }
            
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemoCategory"));
            return await Index();
        }

        private bool MssqlDemoCategoryExists(int id)
        {
            return _context.MssqlDemoCategories.Any(e => e.Id == id);
        }
    }
}

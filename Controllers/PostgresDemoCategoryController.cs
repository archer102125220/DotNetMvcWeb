using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// Postgres Demo 分類控制器
    /// </summary>
    public class PostgresDemoCategoryController : Controller
    {
        private readonly PostgresDbContext _context;

        public PostgresDemoCategoryController(PostgresDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<PostgresDemoCategory> items = await GetItemsAsync();

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CategoryList", items);
            }

            return View(items);
        }

        private async Task<List<PostgresDemoCategory>> GetItemsAsync()
        {
            return await _context.PostgresDemoCategories
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> Create()
        {
            PostgresDemoCategory model = new PostgresDemoCategory();

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
        public async Task<IActionResult> Create([Bind("Name")] PostgresDemoCategory item)
        {
            if (ModelState.IsValid)
            {
                item.CreatedAt = DateTime.UtcNow;
                _context.Add(item);
                await _context.SaveChangesAsync();
                
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "PostgresDemoCategory"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#postgres-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            PostgresDemoCategory? item = await _context.PostgresDemoCategories.FindAsync(id);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedAt")] PostgresDemoCategory item)
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
                    if (!PostgresDemoCategoryExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "PostgresDemoCategory"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#postgres-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            PostgresDemoCategory? item = await _context.PostgresDemoCategories.FindAsync(id);
            if (item != null)
            {
                _context.PostgresDemoCategories.Remove(item);
                await _context.SaveChangesAsync();
            }
            
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "PostgresDemoCategory"));
            return await Index();
        }

        private bool PostgresDemoCategoryExists(int id)
        {
            return _context.PostgresDemoCategories.Any(e => e.Id == id);
        }
    }
}

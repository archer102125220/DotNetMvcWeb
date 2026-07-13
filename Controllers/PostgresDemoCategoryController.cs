using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// Postgres Demo 分類控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫中 PostgresDemoCategories 進行 CRUD 操作
    /// </summary>
    public class PostgresDemoCategoryController : Controller
    {
        private readonly IPostgresDemoCategoryService _categoryService;

        // [教學註解] 依賴注入 (Dependency Injection, DI)
        // 這裡不再直接注入 DbContext，而是注入定義好的 Service 介面。
        public PostgresDemoCategoryController(IPostgresDemoCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// GET: /PostgresDemoCategory
        /// </summary>
        public async Task<IActionResult> Index()
        {
            List<PostgresDemoCategory> items = await _categoryService.GetCategoriesAsync();

            // [教學註解] 檢查是否為 HTMX (AJAX) 請求，是的話只回傳部分視圖
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CategoryList", items);
            }

            // [教學註解] 一般瀏覽器請求，回傳完整視圖
            return View(items);
        }

        /// <summary>
        /// GET: /PostgresDemoCategory/Create
        /// </summary>
        public async Task<IActionResult> Create()
        {
            PostgresDemoCategory model = new PostgresDemoCategory();

            // [教學註解] 若是 HTMX 點擊進來，只回傳表單
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            // [教學註解] 解決重新整理重置版：如果是重整，我們回傳完整的 Index 頁面，
            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await _categoryService.GetCategoriesAsync());
        }

        /// <summary>
        /// POST: /PostgresDemoCategory/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] PostgresDemoCategory item)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.CreateCategoryAsync(item);
                
                // [教學註解] 透過 Response Header 指示 HTMX 更新瀏覽器的網址列，避免網址停留在 /Create
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "PostgresDemoCategory"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#postgres-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /PostgresDemoCategory/Edit/5
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            PostgresDemoCategory? item = await _categoryService.GetCategoryByIdAsync(id.Value);
            if (item == null) return NotFound();
            
            // [教學註解] 若是 HTMX 請求，只回傳表單
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", item);
            }

            // [教學註解] 若是直接輸入網址或重新整理，回傳完整的 Index 頁面，並自動帶入表單內容
            ViewBag.ActiveItem = item;
            ViewBag.IsCreate = false;
            ViewBag.IsEdit = true;
            return View("Index", await _categoryService.GetCategoriesAsync());
        }

        /// <summary>
        /// POST: /PostgresDemoCategory/Edit/5
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedAt")] PostgresDemoCategory item)
        {
            if (id != item.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.UpdateCategoryAsync(item);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_categoryService.CategoryExists(item.Id))
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

        /// <summary>
        /// POST: /PostgresDemoCategory/Delete/5
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "PostgresDemoCategory"));
            return await Index();
        }
    }
}

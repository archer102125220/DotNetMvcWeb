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
    /// Oracle Demo 分類控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫中 OracleDemoCategories 進行 CRUD 操作
    /// </summary>
    public class OracleDemoCategoryController : Controller
    {
        private readonly IOracleDemoCategoryService _categoryService;

        // [教學註解] 依賴注入 (Dependency Injection, DI)
        // 這裡不再直接注入 DbContext，而是注入定義好的 Service 介面。
        public OracleDemoCategoryController(IOracleDemoCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// GET: /OracleDemoCategory
        /// </summary>
        public async Task<IActionResult> Index()
        {
            List<OracleDemoCategory> items = await _categoryService.GetCategoriesAsync();

            // [教學註解] 漸進式增強 (Progressive Enhancement)：若為 HTMX 請求，僅回傳 PartialView 節省頻寬。
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CategoryList", items);
            }

            return View(items);
        }

        /// <summary>
        /// GET: /OracleDemoCategory/Create
        /// </summary>
        public async Task<IActionResult> Create()
        {
            var model = new OracleDemoCategory();

            // [教學註解] 若是透過 HTMX 點擊「Create」按鈕進來，只回傳表單的部分 HTML
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            // [教學註解] 漸進式增強：若使用者直接存取 /OracleDemoCategory/Create，將渲染整頁 Index 並自動帶入新增狀態。
            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await _categoryService.GetCategoriesAsync());
        }

        /// <summary>
        /// POST: /OracleDemoCategory/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] OracleDemoCategory item)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.CreateCategoryAsync(item);
                
                // [教學註解] 成功新增後，推播新網址以還原狀態，並回傳更新後的列表。
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemoCategory"));
                return await Index();
            }
            
            // [教學註解] 若驗證失敗，指示 HTMX 將錯誤表單重新渲染回表單區塊中
            Response.Headers.Append("HX-Retarget", "#oracle-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /OracleDemoCategory/Edit/5
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            OracleDemoCategory? item = await _categoryService.GetCategoryByIdAsync(id.Value);
            if (item == null) return NotFound();
            
            // [教學註解] 若是透過 HTMX 點擊 Edit，只回傳編輯表單的部分 HTML
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", item);
            }

            // [教學註解] 漸進式增強：直接重整 Edit 網頁時，將渲染整頁 Index 並自動帶入編輯狀態。
            ViewBag.ActiveItem = item;
            ViewBag.IsEdit = true;
            return View("Index", await _categoryService.GetCategoriesAsync());
        }

        /// <summary>
        /// POST: /OracleDemoCategory/Edit/5
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
                    await _categoryService.UpdateCategoryAsync(item);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_categoryService.CategoryExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                // [教學註解] 成功更新後，推播新網址以還原狀態，並回傳更新後的列表。
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemoCategory"));
                return await Index();
            }
            
            // [教學註解] 若驗證失敗，指示 HTMX 將錯誤表單重新渲染回表單區塊中
            Response.Headers.Append("HX-Retarget", "#oracle-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// POST: /OracleDemoCategory/Delete/5
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            
            // [教學註解] 刪除成功後，確保網址維持在根目錄
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemoCategory"));
            return await Index();
        }
    }
}

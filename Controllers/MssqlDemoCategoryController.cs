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
    /// MSSQL Demo 分類控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫中 MssqlDemoCategories 進行 CRUD 操作
    /// </summary>
    public class MssqlDemoCategoryController : Controller
    {
        private readonly IMssqlDemoCategoryService _categoryService;

        public MssqlDemoCategoryController(IMssqlDemoCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            List<MssqlDemoCategory> items = await _categoryService.GetCategoriesAsync();

            // [教學註解] 漸進式增強 (Progressive Enhancement)：若為 HTMX 請求，僅回傳 PartialView 節省頻寬。
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CategoryList", items);
            }

            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            MssqlDemoCategory model = new MssqlDemoCategory();

            // [教學註解] 若是透過 HTMX 點擊「Create」按鈕進來，只回傳表單的部分 HTML
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            // [教學註解] 漸進式增強：若使用者直接存取 /MssqlDemoCategory/Create，將渲染整頁 Index 並自動帶入新增狀態。
            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await _categoryService.GetCategoriesAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] MssqlDemoCategory item)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.CreateCategoryAsync(item);
                
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

            MssqlDemoCategory? item = await _categoryService.GetCategoryByIdAsync(id.Value);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedAt")] MssqlDemoCategory item)
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
            await _categoryService.DeleteCategoryAsync(id);
            
            // [教學註解] 刪除成功後，確保網址維持在根目錄
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemoCategory"));
            return await Index();
        }
    }
}

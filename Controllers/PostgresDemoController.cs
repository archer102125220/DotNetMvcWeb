using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// Postgres 資料庫示範控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫進行 CRUD (新增、讀取、更新、刪除) 操作
    /// </summary>
    public class PostgresDemoController : Controller
    {
        private readonly IPostgresDemoItemService _itemService;
        private readonly IPostgresDemoCategoryService _categoryService;
        private readonly IConfiguration _configuration;

        // [教學註解] 依賴注入 (Dependency Injection, DI)
        // 這裡不直接注入 DbContext，而是注入定義好的 Service 介面。
        // 這樣可以達到「關注點分離」，讓 Controller 只負責接收請求與回傳結果，商業邏輯交給 Service 處理。
        public PostgresDemoController(IPostgresDemoItemService itemService, IPostgresDemoCategoryService categoryService, IConfiguration configuration)
        {
            _itemService = itemService;
            _categoryService = categoryService;
            _configuration = configuration;
        }

        /// <summary>
        /// GET: /PostgresDemo
        /// 顯示主頁面，並載入初始資料列表。若為 HTMX 請求則回傳 PartialView。
        /// </summary>
        public async Task<IActionResult> Index(string? keyword = null)
        {
            List<PostgresDemoItem> items = await _itemService.GetItemsAsync(keyword);

            // [教學註解] 檢查是否為 HTMX (AJAX) 請求，是的話只回傳部分視圖
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_DemoList", items);
            }

            ViewBag.Keyword = keyword;
            // [教學註解] 一般瀏覽器請求，回傳完整視圖
            return View(items);
        }

        /// <summary>
        /// GET: /PostgresDemo/Create
        /// 回傳「新增項目」的表單 Partial View，供 HTMX 載入到畫面上。
        /// </summary>
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropDownListAsync();
            PostgresDemoItem model = new PostgresDemoItem();

            // [教學註解] 若是 HTMX 點擊進來，只回傳表單
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            // [教學註解] 解決重新整理重置版：如果是重整，我們回傳完整的 Index 頁面，
            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await _itemService.GetItemsAsync(null));
        }

        /// <summary>
        /// POST: /PostgresDemo/Create
        /// 處理「新增項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,CategoryId")] PostgresDemoItem item)
        {
            if (ModelState.IsValid)
            {
                await _itemService.CreateItemAsync(item);
                
                // [教學註解] 透過 Response Header 指示 HTMX 更新瀏覽器的網址列，避免網址停留在 /Create
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "PostgresDemo"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#postgres-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            await PopulateCategoriesDropDownListAsync(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /PostgresDemo/Edit/5
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            PostgresDemoItem? item = await _itemService.GetItemByIdAsync(id.Value);
            if (item == null) return NotFound();
            
            await PopulateCategoriesDropDownListAsync(item.CategoryId);

            // [教學註解] 若是 HTMX 請求，只回傳表單
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", item);
            }

            // [教學註解] 若是直接輸入網址或重新整理，回傳完整的 Index 頁面，並自動帶入表單內容
            ViewBag.ActiveItem = item;
            ViewBag.IsCreate = false;
            ViewBag.IsEdit = true;
            return View("Index", await _itemService.GetItemsAsync(null));
        }

        /// <summary>
        /// POST: /PostgresDemo/Edit/5
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedAt,CategoryId")] PostgresDemoItem item)
        {
            if (id != item.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _itemService.UpdateItemAsync(item);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_itemService.ItemExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "PostgresDemo"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#postgres-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            await PopulateCategoriesDropDownListAsync(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// POST: /PostgresDemo/Delete/5
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _itemService.DeleteItemAsync(id);
            
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "PostgresDemo"));
            return await Index();
        }

        /// <summary>
        /// POST: /PostgresDemo/UpdateDescriptionViaProcedure/5
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDescriptionViaProcedure(int id, string newDescription)
        {
            if (!string.IsNullOrWhiteSpace(newDescription))
            {
                await _itemService.UpdateItemDescriptionViaProcAsync(id, newDescription);
            }
            
            return await Index();
        }

        private async Task PopulateCategoriesDropDownListAsync(object? selectedCategory = null)
        {
            List<PostgresDemoCategory> categories = await _categoryService.GetCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategory);
        }

        /// <summary>
        /// GET: /PostgresDemo/AdoNetDemo
        /// 示範如何直接使用 Npgsql 原生 ADO.NET 方式連線與查詢
        /// </summary>
        public async Task<IActionResult> AdoNetDemo(string? keyword = null)
        {
            List<PostgresDemoItem> items = await _itemService.GetItemsViaAdoNetAsync(keyword);

            ViewBag.Keyword = keyword;
            return View(items);
        }
    }
}

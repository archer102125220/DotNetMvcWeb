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
    /// MySQL Demo 分類控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫中 MysqlDemoCategories 進行 CRUD 操作
    /// </summary>
    public class MysqlDemoCategoryController : Controller
    {
        private readonly IMysqlDemoCategoryService _categoryService;

        // [教學註解] 依賴注入 (Dependency Injection, DI)
        // 這裡不直接注入 DbContext，而是注入定義好的 Service 介面。
        public MysqlDemoCategoryController(IMysqlDemoCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// GET: /MysqlDemoCategory
        /// 顯示分類管理主頁面，並載入初始資料列表
        /// </summary>
        public async Task<IActionResult> Index()
        {
            List<MysqlDemoCategory> items = await _categoryService.GetCategoriesAsync();

            // [教學註解] 檢查是否為 HTMX (AJAX) 請求，是的話只回傳部分視圖
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CategoryList", items);
            }

            // [教學註解] 一般瀏覽器請求，回傳完整視圖
            return View(items);
        }

        /// <summary>
        /// GET: /MysqlDemoCategory/Create
        /// 回傳「新增分類」的表單 Partial View，供 HTMX 載入到畫面上。
        /// </summary>
        public async Task<IActionResult> Create()
        {
            MysqlDemoCategory model = new MysqlDemoCategory();

            // [教學註解] 若是 HTMX 點擊進來，只回傳表單
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            // [教學註解] 解決重新整理重置版：如果是重整，我們回傳完整的 Index 頁面，
            // 並且把 ActiveItem 透過 ViewBag 帶過去，讓 Index 在載入時自己把表單畫出來。
            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await _categoryService.GetCategoriesAsync());
        }

        /// <summary>
        /// POST: /MysqlDemoCategory/Create
        /// 處理「新增分類」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> Create([Bind("Name")] MysqlDemoCategory item)
        {
            if (ModelState.IsValid) // 檢查資料驗證是否通過
            {
                await _categoryService.CreateCategoryAsync(item); // 非同步寫入資料庫
                
                // [教學註解] 透過 Response Header 指示 HTMX 更新瀏覽器的網址列，避免網址停留在 /Create
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MysqlDemoCategory"));
                return await Index();
            }
            
            // 若驗證失敗，指示 HTMX 將錯誤表單重新渲染回表單區塊中
            Response.Headers.Append("HX-Retarget", "#mysql-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /MysqlDemoCategory/Edit/5
        /// 根據 ID 回傳「編輯分類」的表單 Partial View，供 HTMX 載入到畫面上。
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            MysqlDemoCategory? item = await _categoryService.GetCategoryByIdAsync(id.Value);
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
        /// POST: /MysqlDemoCategory/Edit/5
        /// 處理「編輯分類」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedAt")] MysqlDemoCategory item)
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
                // 更新成功，回傳列表 Partial View
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MysqlDemoCategory"));
                return await Index();
            }

            // 驗證失敗，將錯誤表單重新渲染
            Response.Headers.Append("HX-Retarget", "#mysql-demo-category-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// POST: /MysqlDemoCategory/Delete/5
        /// 處理刪除分類的請求 (直接透過 HTMX 發送 POST)
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            
            // 刪除成功後，回傳更新後的列表
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MysqlDemoCategory"));
            return await Index();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// Postgres 資料庫示範控制器
    /// </summary>
    public class PostgresDemoController : Controller
    {
        private readonly PostgresDbContext _context;
        private readonly IConfiguration _configuration;

        public PostgresDemoController(PostgresDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(string? keyword = null)
        {
            List<PostgresDemoItem> items = await GetItemsAsync(keyword);

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_DemoList", items);
            }

            ViewBag.Keyword = keyword;
            return View(items);
        }

        private async Task<List<PostgresDemoItem>> GetItemsAsync(string? keyword)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                return await _context.PostgresDemoItems
                    .FromSqlInterpolated($"SELECT * FROM \"PostgresDemoItems\" WHERE \"Name\" LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }

            return await _context.PostgresDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> Create()
        {
            PopulateCategoriesDropDownList();
            var model = new PostgresDemoItem();

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await GetItemsAsync(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,CategoryId")] PostgresDemoItem item)
        {
            if (ModelState.IsValid)
            {
                item.CreatedAt = DateTime.UtcNow;
                _context.Add(item);
                await _context.SaveChangesAsync();
                
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "PostgresDemo"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#postgres-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            PostgresDemoItem? item = await _context.PostgresDemoItems.FindAsync(id);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedAt,CategoryId")] PostgresDemoItem item)
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
                    if (!PostgresDemoItemExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "PostgresDemo"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#postgres-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            PostgresDemoItem? item = await _context.PostgresDemoItems.FindAsync(id);
            if (item != null)
            {
                _context.PostgresDemoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "PostgresDemo"));
            return await Index();
        }

        private bool PostgresDemoItemExists(int id)
        {
            return _context.PostgresDemoItems.Any(e => e.Id == id);
        }

        private void PopulateCategoriesDropDownList(object? selectedCategory = null)
        {
            var categoriesQuery = _context.PostgresDemoCategories.OrderBy(c => c.Name);
            ViewBag.Categories = new SelectList(categoriesQuery.AsNoTracking(), "Id", "Name", selectedCategory);
        }

        /// <summary>
        /// GET: /PostgresDemo/AdoNetDemo
        /// 示範如何直接使用 Npgsql 原生 ADO.NET 方式連線與查詢
        /// </summary>
        public async Task<IActionResult> AdoNetDemo(string? keyword = null)
        {
            // [教學註解] 直接利用已經設定在 DbContext 內的連線字串，或是從 Configuration 取得
            string? connectionString = _configuration.GetConnectionString("PostgresDemoConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("無法從 appsettings.json 取得 PostgresDemoConnection 連接字串");
            }

            var resultList = new List<PostgresDemoItem>();

            // ⚠️ 深度檢查注意：必須使用 await using 包覆 IDisposable 物件
            await using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                // [教學註解] Async First 政策
                await connection.OpenAsync();

                // [教學註解] 建立要送到 Postgres 執行的指令物件 (Command)
                await using (NpgsqlCommand command = connection.CreateCommand())
                {
                    // [教學註解] 撰寫原生 SQL 查詢
                    string sqlText = """
                        SELECT 
                            item."Id", 
                            item."Name", 
                            item."CreatedAt", 
                            item."Description", 
                            item."CategoryId", 
                            category."Name" AS "CategoryName"
                        FROM "PostgresDemoItems" item
                        LEFT JOIN "PostgresDemoCategories" category ON item."CategoryId" = category."Id"
                    """;

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sqlText += " WHERE item.\"Name\" LIKE @keyword";
                        // [教學註解] 防止 SQL Injection：必須使用參數化查詢
                        command.Parameters.Add(new NpgsqlParameter("keyword", $"%{keyword}%"));
                    }

                    sqlText += " ORDER BY item.\"CreatedAt\" DESC";
                    command.CommandText = sqlText;

                    // [教學註解] ExecuteReaderAsync 會回傳一個 DataReader，這是一種流式 (Streaming) 讀取方式
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new PostgresDemoItem
                            {
                                // [教學註解] 透過索引值取出對應的欄位，這是最快的
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                CreatedAt = reader.GetDateTime(2),
                                // [教學註解] 原生讀取資料時，必須先呼叫 IsDBNull 檢查
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CategoryId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                            };

                            if (!reader.IsDBNull(5))
                            {
                                item.Category = new PostgresDemoCategory { Name = reader.GetString(5) };
                            }

                            resultList.Add(item);
                        }
                    }
                }
            }

            ViewBag.Keyword = keyword;
            return View(resultList);
        }
    }
}

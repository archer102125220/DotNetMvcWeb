using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// MSSQL 資料庫示範控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫進行 CRUD (新增、讀取、更新、刪除) 操作
    /// </summary>
    public class MssqlDemoController : Controller
    {
        private readonly MssqlDbContext _context;
        private readonly IConfiguration _configuration;

        // 透過依賴注入 (Dependency Injection) 取得資料庫上下文與設定檔
        public MssqlDemoController(MssqlDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// GET: /MssqlDemo
        /// 顯示主頁面，並載入初始資料列表。若為 HTMX 請求則回傳 PartialView。
        /// </summary>
        public async Task<IActionResult> Index(string? keyword = null)
        {
            List<MssqlDemoItem> items = await GetItemsAsync(keyword);

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
        private async Task<List<MssqlDemoItem>> GetItemsAsync(string? keyword)
        {
            // 如果有輸入關鍵字，就使用原生 SQL 進行查詢
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                // ⚠️ 深度檢查注意：使用 FromSqlInterpolated 會自動進行參數化，安全防止 SQL Injection。
                return await _context.MssqlDemoItems
                    .FromSqlInterpolated($"SELECT * FROM [MssqlDemoItems] WHERE [Name] LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }

            // 如果沒有關鍵字，就使用一般的 LINQ 查詢全部
            return await _context.MssqlDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// GET: /MssqlDemo/Create
        /// 回傳「新增項目」的表單 Partial View，供 HTMX 載入到畫面上。
        /// </summary>
        public async Task<IActionResult> Create()
        {
            PopulateCategoriesDropDownList();
            var model = new MssqlDemoItem();

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await GetItemsAsync(null));
        }

        /// <summary>
        /// POST: /MssqlDemo/Create
        /// 處理「新增項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> Create([Bind("Name,Description,CategoryId")] MssqlDemoItem item)
        {
            if (ModelState.IsValid) // 檢查資料驗證是否通過
            {
                item.CreatedAt = DateTime.UtcNow;
                _context.Add(item);
                await _context.SaveChangesAsync(); // 非同步寫入資料庫
                
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemo"));
                
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#mssql-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /MssqlDemo/Edit/5
        /// 根據 ID 回傳「編輯項目」的表單 Partial View，供 HTMX 載入到畫面上。
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            MssqlDemoItem? item = await _context.MssqlDemoItems.FindAsync(id);
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
        /// POST: /MssqlDemo/Edit/5
        /// 處理「編輯項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedAt,CategoryId")] MssqlDemoItem item)
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
                    if (!MssqlDemoItemExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemo"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#mssql-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// POST: /MssqlDemo/Delete/5
        /// 處理刪除項目的請求 (直接透過 HTMX 發送 POST)
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            MssqlDemoItem? item = await _context.MssqlDemoItems.FindAsync(id);
            if (item != null)
            {
                _context.MssqlDemoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemo"));
            return await Index();
        }

        private bool MssqlDemoItemExists(int id)
        {
            return _context.MssqlDemoItems.Any(e => e.Id == id);
        }

        private void PopulateCategoriesDropDownList(object? selectedCategory = null)
        {
            var categoriesQuery = _context.MssqlDemoCategories.OrderBy(c => c.Name);
            ViewBag.Categories = new SelectList(categoriesQuery.AsNoTracking(), "Id", "Name", selectedCategory);
        }

        /// <summary>
        /// GET: /MssqlDemo/AdoNetDemo
        /// 示範如何直接使用 Microsoft.Data.SqlClient 原生 ADO.NET 方式連線與查詢
        /// </summary>
        public async Task<IActionResult> AdoNetDemo(string? keyword = null)
        {
            string? connectionString = _configuration.GetConnectionString("MssqlDemoConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("無法從 appsettings.json 取得 MssqlDemoConnection 連接字串");
            }

            var resultList = new List<MssqlDemoItem>();

            await using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                await using (SqlCommand command = connection.CreateCommand())
                {
                    string sqlText = """
                        SELECT 
                            item.[Id], 
                            item.[Name], 
                            item.[CreatedAt], 
                            item.[Description], 
                            item.[CategoryId], 
                            category.[Name] AS [CategoryName]
                        FROM [MssqlDemoItems] item
                        LEFT JOIN [MssqlDemoCategories] category ON item.[CategoryId] = category.[Id]
                    """;

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sqlText += " WHERE item.[Name] LIKE @keyword";
                        command.Parameters.Add(new SqlParameter("@keyword", $"%{keyword}%"));
                    }

                    sqlText += " ORDER BY item.[CreatedAt] DESC";
                    command.CommandText = sqlText;

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new MssqlDemoItem
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                CreatedAt = reader.GetDateTime(2),
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CategoryId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                            };

                            if (!reader.IsDBNull(5))
                            {
                                item.Category = new MssqlDemoCategory { Name = reader.GetString(5) };
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

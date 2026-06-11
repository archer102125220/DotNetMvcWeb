using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// MySQL 資料庫示範控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫進行 CRUD (新增、讀取、更新、刪除) 操作
    /// </summary>
    public class MysqlDemoController : Controller
    {
        private readonly MysqlDbContext _context;
        private readonly IConfiguration _configuration;

        // 透過依賴注入 (Dependency Injection) 取得資料庫上下文與設定檔
        public MysqlDemoController(MysqlDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// GET: /MysqlDemo
        /// 顯示主頁面，並載入初始資料列表。若為 HTMX 請求則回傳 PartialView。
        /// </summary>
        public async Task<IActionResult> Index(string? keyword = null)
        {
            List<MysqlDemoItem> items = await GetItemsAsync(keyword);

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
        private async Task<List<MysqlDemoItem>> GetItemsAsync(string? keyword)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                // ⚠️ 深度檢查注意：使用 FromSqlInterpolated 會自動進行參數化，安全防止 SQL Injection。
                // 注意 MySQL 表名不一定需要加上雙引號，但為了安全與習慣我們使用反引號 ` 或交由 EF Core
                return await _context.MysqlDemoItems
                    .FromSqlInterpolated($"SELECT * FROM `MysqlDemoItems` WHERE `Name` LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }

            return await _context.MysqlDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// GET: /MysqlDemo/Create
        /// 回傳「新增項目」的表單 Partial View，供 HTMX 載入到畫面上。
        /// </summary>
        public async Task<IActionResult> Create()
        {
            PopulateCategoriesDropDownList();
            var model = new MysqlDemoItem();

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await GetItemsAsync(null));
        }

        /// <summary>
        /// POST: /MysqlDemo/Create
        /// 處理「新增項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,CategoryId")] MysqlDemoItem item)
        {
            if (ModelState.IsValid)
            {
                item.CreatedAt = DateTime.UtcNow;
                _context.Add(item);
                await _context.SaveChangesAsync();
                
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MysqlDemo"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#mysql-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// GET: /MysqlDemo/Edit/5
        /// 根據 ID 回傳「編輯項目」的表單 Partial View，供 HTMX 載入到畫面上。
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            MysqlDemoItem? item = await _context.MysqlDemoItems.FindAsync(id);
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
        /// POST: /MysqlDemo/Edit/5
        /// 處理「編輯項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedAt,CategoryId")] MysqlDemoItem item)
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
                    if (!MysqlDemoItemExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MysqlDemo"));
                return await Index();
            }
            
            Response.Headers.Append("HX-Retarget", "#mysql-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// POST: /MysqlDemo/Delete/5
        /// 處理刪除項目的請求 (直接透過 HTMX 發送 POST)
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            MysqlDemoItem? item = await _context.MysqlDemoItems.FindAsync(id);
            if (item != null)
            {
                _context.MysqlDemoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MysqlDemo"));
            return await Index();
        }

        private bool MysqlDemoItemExists(int id)
        {
            return _context.MysqlDemoItems.Any(e => e.Id == id);
        }

        private void PopulateCategoriesDropDownList(object? selectedCategory = null)
        {
            var categoriesQuery = _context.MysqlDemoCategories.OrderBy(c => c.Name);
            ViewBag.Categories = new SelectList(categoriesQuery.AsNoTracking(), "Id", "Name", selectedCategory);
        }

        /// <summary>
        /// GET: /MysqlDemo/AdoNetDemo
        /// 示範如何直接使用 MySql.Data.MySqlClient 原生 ADO.NET 方式連線與查詢
        /// </summary>
        public async Task<IActionResult> AdoNetDemo(string? keyword = null)
        {
            // [教學註解] 直接利用已經設定在 DbContext 內的連線字串的情況，
            // 可參考 Controllers/Api/MysqlDemoApiController.cs，
            // 這裡示範「手動剖析」設定檔：
            // 透過依賴注入取得 IConfiguration，直接從 appsettings.json 中讀取連線字串。
            // 這在沒有使用 Entity Framework (DbContext) 的純 ADO.NET 專案中是標準作法。
            string? connectionString = _configuration.GetConnectionString("MysqlDemoConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("無法從 appsettings.json 取得 MysqlDemoConnection 連接字串");
            }

            var resultList = new List<MysqlDemoItem>();

            // ⚠️ 深度檢查注意：必須使用 await using 包覆 IDisposable 物件 (MySqlConnection, MySqlCommand, DbDataReader)
            // 原生的 ADO.NET 操作需要開發者自行負責釋放連線。如果忘記 using，會造成 Connection Pool 被耗盡。
            await using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // [教學註解] Async First 政策：所有資料庫操作都必須使用 Async 版本。
                await connection.OpenAsync();

                await using (MySqlCommand command = connection.CreateCommand())
                {
                    // [教學註解] 撰寫原生 SQL 查詢，這裡示範了如何做 JOIN。
                    // ⚠️ 注意：MySQL 對於保留字和欄位名稱會習慣使用反引號 ` 包起來。
                    string sqlText = """
                        SELECT 
                            item.`Id`, 
                            item.`Name`, 
                            item.`CreatedAt`, 
                            item.`Description`, 
                            item.`CategoryId`, 
                            category.`Name` AS `CategoryName`
                        FROM `MysqlDemoItems` item
                        LEFT JOIN `MysqlDemoCategories` category ON item.`CategoryId` = category.`Id`
                    """;

                    // [教學註解] 動態加入搜尋條件 (WHERE)
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sqlText += " WHERE item.`Name` LIKE @keyword";
                        // [教學註解] ⚠️ 絕對禁止字串拼接！必須使用 Parameter 參數化查詢，防止 SQL Injection (隱碼攻擊)
                        command.Parameters.Add(new MySqlParameter("@keyword", $"%{keyword}%"));
                    }

                    sqlText += " ORDER BY item.`CreatedAt` DESC";
                    command.CommandText = sqlText;

                    // [教學註解] ExecuteReaderAsync 會開啟資料流讀取器
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        // [教學註解] ReadAsync() 會逐筆將資料拉到應用程式記憶體中。
                        while (await reader.ReadAsync())
                        {
                            var item = new MysqlDemoItem
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                CreatedAt = reader.GetDateTime(2),
                                // [教學註解] 原生讀取資料時，針對可能為 NULL 的欄位，必須先呼叫 IsDBNull 進行檢查。
                                // 否則呼叫 GetString 或 GetInt32 時會引發 SqlNullValueException！
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CategoryId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                            };

                            // [教學註解] 如果有對應的 CategoryName，就手動建構關聯物件 (Navigation Property)
                            if (!reader.IsDBNull(5))
                            {
                                item.Category = new MysqlDemoCategory { Name = reader.GetString(5) };
                            }

                            resultList.Add(item);
                        }
                    }
                }
            }

            ViewBag.Keyword = keyword;
            // [教學註解] 回傳給具備 UI 畫面的 View，並將剛剛手動組裝好的 List 傳遞給 @model
            return View(resultList);
        }
    }
}

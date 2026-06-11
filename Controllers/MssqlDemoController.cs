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
using System.Data.Common;

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

            // [教學註解] 漸進式增強 (Progressive Enhancement) 的核心：
            // 透過檢查 Request Header 是否包含 "HX-Request"，我們可以知道這個請求是由 HTMX (AJAX) 發出的，
            // 還是由瀏覽器直接重整 (F5) 或輸入網址發出的。
            // 若為 HTMX 請求，我們只需要回傳清單的部分視圖 (PartialView)，不用回傳整個帶有 Layout 的網頁，節省頻寬。
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_DemoList", items);
            }

            // 若為一般瀏覽器請求 (例如剛進來或重整)，則回傳完整的 View (包含 _Layout)
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
                // 另外，查詢後仍必須加上 .AsNoTracking() 來進行唯讀優化。
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
        /// 
        /// 【程式碼撰寫與設定解說：如何載入 _CreateOrEdit.cshtml】
        /// 1. 路由對應：前端使用 `Url.Action("Create", "MssqlDemo")` 會產生 `/MssqlDemo/Create` 的網址。
        ///    ASP.NET Core 的預設路由機制會自動找到 `MssqlDemoController` 底下名稱為 `Create` 的這個方法。
        /// 2. 回傳視圖：方法最後呼叫了 `PartialView("_CreateOrEdit", new MssqlDemoItem())`。
        ///    - 這裡明確指定了要尋找名稱為 `_CreateOrEdit` 的視圖檔案。
        ///    - 框架會按照慣例到 `Views/MssqlDemo/` 資料夾下尋找 `_CreateOrEdit.cshtml`。
        ///    - 將一個全新的空 `MssqlDemoItem` 模型傳遞給該視圖，以便產生空表單。
        /// </summary>
        public async Task<IActionResult> Create()
        {
            PopulateCategoriesDropDownList();
            var model = new MssqlDemoItem();

            // [教學註解] 若是透過 HTMX 點擊「Create」按鈕進來，只回傳表單的部分 HTML
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            // [教學註解] 漸進式增強 (Progressive Enhancement) - 處理重整問題：
            // 若使用者直接重整 /MssqlDemo/Create 網頁（此時不會有 HX-Request header），
            // 我們如果只回傳 PartialView，畫面就會破版 (沒有選單與 CSS)。
            // 因此我們將表單狀態 (model) 放入 ViewBag，然後改為渲染整頁的 "Index" 視圖。
            // 這樣使用者重新整理時，就會看到完整的列表頁面，且左側自動開啟新增表單！
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
                
                // [教學註解] 狀態網址化 (URL State Sync) - 送出後的還原：
                // 當成功新增後，我們利用 Response Header 告訴 HTMX 去推播 (Push) 一個新網址。
                // 這樣可以把原本是 /MssqlDemo/Create 的網址，自動還原回乾淨的 /MssqlDemo，
                // 確保使用者如果此時按下 F5，不會不小心又進入 Create 頁面發送 POST 請求。
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemo"));
                
                // 重新呼叫 Index() 取得最新列表並回傳 (因為是 HTMX 請求，Index 會自動回傳 PartialView)
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
        /// 
        /// 【程式碼撰寫與設定解說：如何載入 _CreateOrEdit.cshtml 作為編輯用】
        /// 1. 路由對應：前端使用 `Url.Action("Edit", "MssqlDemo", new { id = item.Id })` 會產生如 `/MssqlDemo/Edit/5` 的網址。
        ///    路由機制會對應到這個 `Edit(int? id)` 方法，並將網址結尾的數字作為 `id` 參數傳入。
        /// 2. 回傳視圖：從資料庫撈出對應的 `item` 資料後，同樣呼叫 `PartialView("_CreateOrEdit", item)`。
        ///    - 這表示「新增」和「編輯」共用了同一個 `.cshtml` 檔案。
        ///    - 視圖內部會根據傳入的模型（`Model.Id == 0` 或有值）來決定顯示「Create」還是「Edit」的標題及行為。
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            MssqlDemoItem? item = await _context.MssqlDemoItems.FindAsync(id);
            if (item == null) return NotFound();
            
            PopulateCategoriesDropDownList(item.CategoryId);

            // [教學註解] 若是點擊列表的 Edit 按鈕 (HTMX 請求)，回傳表單的部分視圖
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", item);
            }

            // [教學註解] 漸進式增強 (Progressive Enhancement)：
            // 處理使用者直接複製 /MssqlDemo/Edit/5 貼給別人，或是直接 F5 重整頁面的情況。
            // 把讀取到的 item 塞入 ViewBag，由 Index 視圖做整頁渲染，實現「無縫狀態接軌」。
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
                // 更新成功，回傳列表 Partial View，並還原網址
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
            
            // 刪除成功後，回傳更新後的列表，並確保網址維持在根目錄
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "MssqlDemo"));
            return await Index();
        }

        private bool MssqlDemoItemExists(int id)
        {
            return _context.MssqlDemoItems.Any(e => e.Id == id);
        }

        private void PopulateCategoriesDropDownList(object? selectedCategory = null)
        {
            IQueryable<MssqlDemoCategory> categoriesQuery = _context.MssqlDemoCategories.OrderBy(c => c.Name);
            ViewBag.Categories = new SelectList(categoriesQuery.AsNoTracking(), "Id", "Name", selectedCategory);
        }

        /// <summary>
        /// GET: /MssqlDemo/AdoNetDemo
        /// 示範如何直接使用 Microsoft.Data.SqlClient 原生 ADO.NET 方式連線與查詢
        /// </summary>
        public async Task<IActionResult> AdoNetDemo(string? keyword = null)
        {
            // [教學註解] 直接利用已經設定在 DbContext 內的連線字串的情況，
            // 可參考 Controllers/Api/MssqlDemoApiController.cs，
            // 這裡示範「手動剖析」設定檔：
            // 透過依賴注入取得 IConfiguration，直接從 appsettings.json 中讀取連線字串。
            // 這在沒有使用 Entity Framework (DbContext) 的純 ADO.NET 專案中是標準作法。
            string? connectionString = _configuration.GetConnectionString("MssqlDemoConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("無法從 appsettings.json 取得 MssqlDemoConnection 連接字串");
            }

            var resultList = new List<MssqlDemoItem>();

            // ⚠️ 深度檢查注意：必須使用 await using 包覆 IDisposable 物件 (SqlConnection, SqlCommand, SqlDataReader)
            // 原生的 ADO.NET 操作需要開發者自行負責釋放連線。如果忘記 using，會造成 Connection Pool 被耗盡。
            await using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // [教學註解] Async First 政策：所有資料庫操作都必須使用 Async 版本。
                await connection.OpenAsync();

                // [教學註解] 建立要送到 MSSQL 執行的指令物件 (Command)
                await using (SqlCommand command = connection.CreateCommand())
                {
                    // [教學註解] 撰寫原生 SQL 查詢，這裡示範了如何做 JOIN。
                    // ⚠️ 注意：MSSQL 中，使用中括號 [] 可以避免與保留字衝突，並明確指定物件名稱。
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

                    // [教學註解] 動態加入搜尋條件 (WHERE)
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sqlText += " WHERE item.[Name] LIKE @keyword";
                        // [教學註解] ⚠️ 絕對禁止字串拼接！必須使用 Parameter 參數化查詢，防止 SQL Injection (隱碼攻擊)
                        command.Parameters.Add(new SqlParameter("@keyword", $"%{keyword}%"));
                    }

                    sqlText += " ORDER BY item.[CreatedAt] DESC";
                    command.CommandText = sqlText;

                    // [教學註解] ExecuteReaderAsync 會開啟資料流讀取器
                    await using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // [教學註解] ReadAsync() 會逐筆將資料拉到應用程式記憶體中。
                        while (await reader.ReadAsync())
                        {
                            var item = new MssqlDemoItem
                            {
                                // [教學註解] 透過索引值取出對應的欄位，這是最快的。
                                // 如果要用欄位名稱取值，可以使用 reader.GetOrdinal("Id") 取得索引。
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                // MSSQL DATETIME 或 DATETIME2 型別可以直接轉換為 C# 的 DateTime
                                CreatedAt = reader.GetDateTime(2),
                                // [教學註解] 原生讀取資料時，針對可能為 NULL 的欄位，必須先呼叫 IsDBNull 進行檢查。
                                // 否則呼叫 GetString 或 GetInt32 時會引發 SqlNullValueException！
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CategoryId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                            };

                            // [教學註解] 如果有對應的 CategoryName，就手動建構關聯物件 (Navigation Property)
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
            // [教學註解] 回傳給具備 UI 畫面的 View，並將剛剛手動組裝好的 List 傳遞給 @model
            return View(resultList);
        }
    }
}

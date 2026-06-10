using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// Oracle 資料庫示範控制器
    /// 負責處理來自前端的 HTMX 請求，並對資料庫進行 CRUD (新增、讀取、更新、刪除) 操作
    /// </summary>
    public class OracleDemoController : Controller
    {
        private readonly AppDbContext _context;

        // 透過依賴注入 (Dependency Injection) 取得資料庫上下文
        public OracleDemoController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /OracleDemo
        /// 顯示主頁面，並載入初始資料列表。若為 HTMX 請求則回傳 PartialView。
        /// </summary>
        public async Task<IActionResult> Index(string? keyword = null)
        {
            List<OracleDemoItem> items = await GetItemsAsync(keyword);

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
        private async Task<List<OracleDemoItem>> GetItemsAsync(string? keyword)
        {
            // 如果有輸入關鍵字，就使用原生 SQL 進行查詢
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                // ⚠️ 深度檢查注意：使用 FromSqlInterpolated 會自動進行參數化，安全防止 SQL Injection。
                // 另外，查詢後仍必須加上 .AsNoTracking() 來進行唯讀優化。
                return await _context.OracleDemoItems
                    .FromSqlInterpolated($"SELECT * FROM \"OracleDemoItems\" WHERE \"Name\" LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }

            // 如果沒有關鍵字，就使用一般的 LINQ 查詢全部
            return await _context.OracleDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }


        /// <summary>
        /// GET: /OracleDemo/Create
        /// 回傳「新增項目」的表單 Partial View，供 HTMX 載入到畫面上。
        /// 
        /// 【程式碼撰寫與設定解說：如何載入 _CreateOrEdit.cshtml】
        /// 1. 路由對應：前端使用 `Url.Action("Create", "OracleDemo")` 會產生 `/OracleDemo/Create` 的網址。
        ///    ASP.NET Core 的預設路由機制會自動找到 `OracleDemoController` 底下名稱為 `Create` 的這個方法。
        /// 2. 回傳視圖：方法最後呼叫了 `PartialView("_CreateOrEdit", new OracleDemoItem())`。
        ///    - 這裡明確指定了要尋找名稱為 `_CreateOrEdit` 的視圖檔案。
        ///    - 框架會按照慣例到 `Views/OracleDemo/` 資料夾下尋找 `_CreateOrEdit.cshtml`。
        ///    - 將一個全新的空 `OracleDemoItem` 模型傳遞給該視圖，以便產生空表單。
        /// </summary>
        public async Task<IActionResult> Create()
        {
            PopulateCategoriesDropDownList();
            var model = new OracleDemoItem();

            // [教學註解] 若是透過 HTMX 點擊「Create」按鈕進來，只回傳表單的部分 HTML
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", model);
            }

            // [教學註解] 漸進式增強 (Progressive Enhancement) - 處理重整問題：
            // 若使用者直接重整 /OracleDemo/Create 網頁（此時不會有 HX-Request header），
            // 我們如果只回傳 PartialView，畫面就會破版 (沒有選單與 CSS)。
            // 因此我們將表單狀態 (model) 放入 ViewBag，然後改為渲染整頁的 "Index" 視圖。
            // 這樣使用者重新整理時，就會看到完整的列表頁面，且左側自動開啟新增表單！
            ViewBag.ActiveItem = model;
            ViewBag.IsCreate = true;
            return View("Index", await GetItemsAsync(null));
        }

        /// <summary>
        /// POST: /OracleDemo/Create
        /// 處理「新增項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public async Task<IActionResult> Create([Bind("Name,Description,CategoryId")] OracleDemoItem item)
        {
            if (ModelState.IsValid) // 檢查資料驗證是否通過
            {
                item.CreatedAt = DateTime.UtcNow;
                _context.Add(item);
                await _context.SaveChangesAsync(); // 非同步寫入資料庫
                
                // [教學註解] 狀態網址化 (URL State Sync) - 送出後的還原：
                // 當成功新增後，我們利用 Response Header 告訴 HTMX 去推播 (Push) 一個新網址。
                // 這樣可以把原本是 /OracleDemo/Create 的網址，自動還原回乾淨的 /OracleDemo，
                // 確保使用者如果此時按下 F5，不會不小心又進入 Create 頁面發送 POST 請求。
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemo"));
                
                // 重新呼叫 Index() 取得最新列表並回傳 (因為是 HTMX 請求，Index 會自動回傳 PartialView)
                return await Index();
            }
            
            // 若驗證失敗，指示 HTMX 將錯誤表單重新渲染回表單區塊中
            Response.Headers.Append("HX-Retarget", "#oracle-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }


        /// <summary>
        /// GET: /OracleDemo/Edit/5
        /// 根據 ID 回傳「編輯項目」的表單 Partial View，供 HTMX 載入到畫面上。
        /// 
        /// 【程式碼撰寫與設定解說：如何載入 _CreateOrEdit.cshtml 作為編輯用】
        /// 1. 路由對應：前端使用 `Url.Action("Edit", "OracleDemo", new { id = item.Id })` 會產生如 `/OracleDemo/Edit/5` 的網址。
        ///    路由機制會對應到這個 `Edit(int? id)` 方法，並將網址結尾的數字作為 `id` 參數傳入。
        /// 2. 回傳視圖：從資料庫撈出對應的 `item` 資料後，同樣呼叫 `PartialView("_CreateOrEdit", item)`。
        ///    - 這表示「新增」和「編輯」共用了同一個 `.cshtml` 檔案。
        ///    - 視圖內部會根據傳入的模型（`Model.Id == 0` 或有值）來決定顯示「Create」還是「Edit」的標題及行為。
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            OracleDemoItem? item = await _context.OracleDemoItems.FindAsync(id);
            if (item == null) return NotFound();
            
            PopulateCategoriesDropDownList(item.CategoryId);

            // [教學註解] 若是點擊列表的 Edit 按鈕 (HTMX 請求)，回傳表單的部分視圖
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CreateOrEdit", item);
            }

            // [教學註解] 漸進式增強 (Progressive Enhancement)：
            // 處理使用者直接複製 /OracleDemo/Edit/5 貼給別人，或是直接 F5 重整頁面的情況。
            // 把讀取到的 item 塞入 ViewBag，由 Index 視圖做整頁渲染，實現「無縫狀態接軌」。
            ViewBag.ActiveItem = item;
            ViewBag.IsEdit = true;
            return View("Index", await GetItemsAsync(null));
        }

        /// <summary>
        /// POST: /OracleDemo/Edit/5
        /// 處理「編輯項目」的表單送出
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedAt,CategoryId")] OracleDemoItem item)
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
                    if (!OracleDemoItemExists(item.Id))
                        return NotFound();
                    else
                        throw;
                }
                // 更新成功，回傳列表 Partial View，並還原網址
                Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemo"));
                return await Index();
            }
            
            // 驗證失敗，將錯誤表單重新渲染
            Response.Headers.Append("HX-Retarget", "#oracle-demo-form-container");
            Response.Headers.Append("HX-Reswap", "innerHTML");
            PopulateCategoriesDropDownList(item.CategoryId);
            return PartialView("_CreateOrEdit", item);
        }

        /// <summary>
        /// POST: /OracleDemo/Delete/5
        /// 處理刪除項目的請求 (直接透過 HTMX 發送 POST)
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            OracleDemoItem? item = await _context.OracleDemoItems.FindAsync(id);
            if (item != null)
            {
                _context.OracleDemoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            
            // 刪除成功後，回傳更新後的列表，並確保網址維持在根目錄
            Response.Headers.Append("HX-Push-Url", Url.Action("Index", "OracleDemo"));
            return await Index();
        }

        // 檢查項目是否存在的輔助方法
        private bool OracleDemoItemExists(int id)
        {
            return _context.OracleDemoItems.Any(e => e.Id == id);
        }

        private void PopulateCategoriesDropDownList(object? selectedCategory = null)
        {
            var categoriesQuery = _context.OracleDemoCategories.OrderBy(c => c.Name);
            ViewBag.Categories = new SelectList(categoriesQuery.AsNoTracking(), "Id", "Name", selectedCategory);
        }

        /// <summary>
        /// GET: /OracleDemo/AdoNetDemo
        /// 示範如何直接使用 Oracle.ManagedDataAccess.Client 原生 ADO.NET 方式連線與查詢
        /// </summary>
        public async Task<IActionResult> AdoNetDemo(string? keyword = null)
        {
            // [教學註解] 從 EF Core 上下文取得連接字串，這比讀取 IConfiguration 更簡潔。
            string? connectionString = _context.Database.GetConnectionString();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("無法取得資料庫連接字串");
            }

            var resultList = new List<OracleDemoItem>();

            // ⚠️ 深度檢查注意：必須使用 await using 包覆 IDisposable 物件 (OracleConnection, OracleCommand, DbDataReader)
            // 原生的 ADO.NET 操作需要開發者自行負責釋放連線。如果忘記 using，會造成 Connection Pool 被耗盡。
            await using (OracleConnection connection = new OracleConnection(connectionString))
            {
                // [教學註解] Async First 政策：所有資料庫操作都必須使用 Async 版本。
                await connection.OpenAsync();

                await using (OracleCommand command = connection.CreateCommand())
                {
                    // [教學註解] 撰寫原生 SQL 查詢，這裡示範了如何做 JOIN。
                    // ⚠️ 注意：Oracle 對於加了雙引號建立的欄位和表格會「強制區分大小寫」，所以這裡的 SQL 也要有雙引號。
                    string sqlText = """
                        SELECT 
                            item."Id", 
                            item."Name", 
                            item."CreatedAt", 
                            item."Description", 
                            item."CategoryId", 
                            category."Name" AS "CategoryName"
                        FROM "OracleDemoItems" item
                        LEFT JOIN "OracleDemoCategories" category ON item."CategoryId" = category."Id"
                    """;

                    // [教學註解] 動態加入搜尋條件 (WHERE)
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sqlText += " WHERE item.\"Name\" LIKE :keyword";
                        // [教學註解] ⚠️ 絕對禁止字串拼接！必須使用 Parameter 參數化查詢，防止 SQL Injection (隱碼攻擊)
                        command.Parameters.Add(new OracleParameter("keyword", $"%{keyword}%"));
                    }

                    sqlText += " ORDER BY item.\"CreatedAt\" DESC";
                    command.CommandText = sqlText;

                    // [教學註解] ExecuteReaderAsync 會開啟資料流讀取器
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        // [教學註解] ReadAsync() 會逐筆將資料拉到應用程式記憶體中。
                        while (await reader.ReadAsync())
                        {
                            var item = new OracleDemoItem
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
                                item.Category = new OracleDemoCategory { Name = reader.GetString(5) };
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

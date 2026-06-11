using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace DotNetMvcWeb.Controllers.Api
{
    /// <summary>
    /// MSSQL Demo API 控制器
    /// 提供給前端或外部系統串接的純 JSON API 介面
    /// </summary>
    [Route("api/mssql-demo")]
    [ApiController]
    public class MssqlDemoApiController : ControllerBase
    {
        private readonly MssqlDbContext _context;

        public MssqlDemoApiController(MssqlDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MssqlDemoItem>>> GetItems([FromQuery] string? keyword = null)
        {
            // ⚠️ 深度檢查: 讀取資料必須使用 AsNoTracking() 進行優化
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                List<MssqlDemoItem> searchResult = await _context.MssqlDemoItems
                    .FromSqlInterpolated($"SELECT * FROM [MssqlDemoItems] WHERE [Name] LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
                
                return Ok(searchResult);
            }

            List<MssqlDemoItem> items = await _context.MssqlDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MssqlDemoItem>> GetItem(int id)
        {
            // 由於 FindAsync 會做 tracking，但這裡只是單純回傳，
            // 若要嚴謹可改用 FirstOrDefaultAsync 搭配 AsNoTracking
            MssqlDemoItem? item = await _context.MssqlDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound(new { message = "找不到指定的項目" });
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<MssqlDemoItem>> CreateItem([FromBody] MssqlDemoItem item)
        {
            // 確保系統自行設定建立時間
            item.CreatedAt = DateTime.UtcNow;
            
            _context.MssqlDemoItems.Add(item);
            await _context.SaveChangesAsync();

            // 成功建立後，回傳 201 Created，並附上取得該資源的 Location URL
            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] MssqlDemoItem item)
        {
            if (id != item.Id)
            {
                return BadRequest(new { message = "路徑中的 ID 與資料本身的 ID 不相符" });
            }

            // 將狀態設為 Modified，EF Core 會在 SaveChanges 時產生 UPDATE SQL
            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MssqlDemoItemExists(id))
                {
                    return NotFound(new { message = "找不到指定的項目，可能已被刪除" });
                }
                else
                {
                    throw; // 將例外往上拋出
                }
            }

            // 更新成功，回傳 204 No Content
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            MssqlDemoItem? item = await _context.MssqlDemoItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "找不到指定的項目，可能已被刪除" });
            }

            _context.MssqlDemoItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "刪除成功" });
        }

        private bool MssqlDemoItemExists(int id)
        {
            return _context.MssqlDemoItems.Any(e => e.Id == id);
        }

        [HttpGet("ado-net-demo")]
        public async Task<IActionResult> AdoNetDemo([FromQuery] string? keyword = null)
        {
            // [教學註解] 雖然我們示範的是原生 ADO.NET，但還是可以直接利用已經設定在 DbContext 內的連線字串
            // 這讓我們不用去 appsettings.json 裡面手動剖析 (parse) IConfiguration。
            string? connectionString = _context.Database.GetConnectionString();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest(new { message = "無法取得資料庫連接字串" });
            }

            var resultList = new List<object>();

            // ⚠️ 深度檢查注意：必須使用 await using 包覆 IDisposable 物件 (SqlConnection, SqlCommand, SqlDataReader)
            // 由於資料庫連線是非常昂貴的資源，務必要確保執行完畢或發生例外時，連線能被正確關閉與釋放 (Dispose)。
            await using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // [教學註解] Async First 政策：不要使用 connection.Open()，以免在流量大時阻塞執行緒 (Thread starvation)。
                await connection.OpenAsync();

                // [教學註解] 建立要送到 MSSQL 執行的指令物件 (Command)
                await using (SqlCommand command = connection.CreateCommand())
                {
                    // [教學註解] 撰寫原生 SQL 查詢，這裡同樣示範如何做 JOIN。
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

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sqlText += " WHERE item.[Name] LIKE @keyword";
                        // [教學註解] 防止 SQL Injection：必須使用參數化查詢
                        command.Parameters.Add(new SqlParameter("@keyword", $"%{keyword}%"));
                    }

                    sqlText += " ORDER BY item.[CreatedAt] DESC";
                    command.CommandText = sqlText;

                    // [教學註解] ExecuteReaderAsync 會回傳一個 DataReader，這是一種流式 (Streaming) 讀取方式。
                    // 它不會一次把幾百萬筆資料塞爆記憶體，而是透過 ReadAsync() 逐筆向資料庫要資料。
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            resultList.Add(new 
                            {
                                // [教學註解] 透過索引值取出對應的欄位，這是最快的。
                                // 如果要用欄位名稱取值，可以使用 reader.GetOrdinal("Id") 取得索引。
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                // MSSQL DATETIME 或 DATETIME2 型別可以直接轉換為 C# 的 DateTime
                                CreatedAt = reader.GetDateTime(2),
                                // [教學註解] 原生讀取資料時，必須先呼叫 IsDBNull 檢查
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CategoryId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                CategoryName = reader.IsDBNull(5) ? null : reader.GetString(5)
                            });
                        }
                    }
                }
            }

            return Ok(resultList);
        }
    }
}

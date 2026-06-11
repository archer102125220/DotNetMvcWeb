using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// MySQL Demo API 控制器
    /// 提供給前端 (如 Vue, React) 或其他系統串接的純 JSON API 介面
    /// </summary>
    [Route("api/mysql-demo")]
    [ApiController]
    public class MysqlDemoApiController : ControllerBase
    {
        private readonly MysqlDbContext _context;

        public MysqlDemoApiController(MysqlDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/mysql-demo
        /// 取得所有項目 (支援 keyword 關鍵字搜尋)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MysqlDemoItem>>> GetItems([FromQuery] string? keyword = null)
        {
            // ⚠️ 深度檢查: 讀取資料必須使用 AsNoTracking() 進行優化
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                List<MysqlDemoItem> searchResult = await _context.MysqlDemoItems
                    .FromSqlInterpolated($"SELECT * FROM `MysqlDemoItems` WHERE `Name` LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
                
                return Ok(searchResult);
            }

            List<MysqlDemoItem> items = await _context.MysqlDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// GET: api/mysql-demo/5
        /// 取得單一項目的詳細資料
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MysqlDemoItem>> GetItem(int id)
        {
            MysqlDemoItem? item = await _context.MysqlDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound(new { message = "找不到指定的項目" });
            }

            return Ok(item);
        }

        /// <summary>
        /// POST: api/mysql-demo
        /// 建立一筆新項目
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<MysqlDemoItem>> CreateItem([FromBody] MysqlDemoItem item)
        {
            item.CreatedAt = DateTime.UtcNow;
            
            _context.MysqlDemoItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
        }

        /// <summary>
        /// PUT: api/mysql-demo/5
        /// 更新一筆項目的資料
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] MysqlDemoItem item)
        {
            if (id != item.Id)
            {
                return BadRequest(new { message = "路徑中的 ID 與資料本身的 ID 不相符" });
            }

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MysqlDemoItemExists(id))
                {
                    return NotFound(new { message = "找不到指定的項目，可能已被刪除" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// DELETE: api/mysql-demo/5
        /// 刪除一筆項目
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            MysqlDemoItem? item = await _context.MysqlDemoItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "找不到指定的項目，可能已被刪除" });
            }

            _context.MysqlDemoItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "刪除成功" });
        }

        private bool MysqlDemoItemExists(int id)
        {
            return _context.MysqlDemoItems.Any(e => e.Id == id);
        }

        /// <summary>
        /// GET: api/mysql-demo/ado-net-demo
        /// 示範如何直接使用 MySql.Data.MySqlClient 原生 ADO.NET 方式連線與查詢
        /// </summary>
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

            // ⚠️ 深度檢查注意：必須使用 await using 包覆 IDisposable 物件 (MySqlConnection, MySqlCommand, DbDataReader)
            // 由於資料庫連線是非常昂貴的資源，務必要確保執行完畢或發生例外時，連線能被正確關閉與釋放 (Dispose)。
            await using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // [教學註解] Async First 政策：不要使用 connection.Open()，以免在流量大時阻塞執行緒 (Thread starvation)。
                await connection.OpenAsync();

                // [教學註解] 建立要送到 MySQL 執行的指令物件 (Command)
                await using (MySqlCommand command = connection.CreateCommand())
                {
                    // [教學註解] 撰寫原生 SQL 查詢，這裡同樣示範如何做 JOIN。
                    // ⚠️ 注意：MySQL 習慣上會使用反引號 ` 來包覆資料表或欄位名稱。
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

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sqlText += " WHERE item.`Name` LIKE @keyword";
                        // [教學註解] 防止 SQL Injection：必須使用參數化查詢
                        command.Parameters.Add(new MySqlParameter("@keyword", $"%{keyword}%"));
                    }

                    sqlText += " ORDER BY item.`CreatedAt` DESC";
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
                                // MySQL DATETIME 或 TIMESTAMP 型別可以直接轉換為 C# 的 DateTime
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

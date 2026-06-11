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
            item.CreatedAt = DateTime.UtcNow;
            
            _context.MssqlDemoItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] MssqlDemoItem item)
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
                if (!MssqlDemoItemExists(id))
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
            string? connectionString = _context.Database.GetConnectionString();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest(new { message = "無法取得資料庫連接字串" });
            }

            var resultList = new List<object>();

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
                            resultList.Add(new 
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                CreatedAt = reader.GetDateTime(2),
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

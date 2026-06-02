using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Controllers
{
    /// <summary>
    /// Oracle Demo API 控制器
    /// 提供給前端 (如 Vue, React) 或其他系統串接的純 JSON API 介面
    /// </summary>
    [Route("api/oracle-demo")]
    [ApiController]
    public class OracleDemoApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OracleDemoApiController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/oracle-demo
        /// 取得所有項目 (支援 keyword 關鍵字搜尋)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OracleDemoItem>>> GetItems([FromQuery] string? keyword = null)
        {
            // ⚠️ 深度檢查: 讀取資料必須使用 AsNoTracking() 進行優化
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                List<OracleDemoItem> searchResult = await _context.OracleDemoItems
                    .FromSqlInterpolated($"SELECT * FROM \"OracleDemoItems\" WHERE \"Name\" LIKE {searchPattern}")
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
                
                return Ok(searchResult);
            }

            List<OracleDemoItem> items = await _context.OracleDemoItems
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// GET: api/oracle-demo/5
        /// 取得單一項目的詳細資料
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OracleDemoItem>> GetItem(int id)
        {
            // 由於 FindAsync 會做 tracking，但這裡只是單純回傳，
            // 若要嚴謹可改用 FirstOrDefaultAsync 搭配 AsNoTracking
            OracleDemoItem? item = await _context.OracleDemoItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound(new { message = "找不到指定的項目" });
            }

            return Ok(item);
        }

        /// <summary>
        /// POST: api/oracle-demo
        /// 建立一筆新項目
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OracleDemoItem>> CreateItem([FromBody] OracleDemoItem item)
        {
            // 確保系統自行設定建立時間
            item.CreatedAt = DateTime.UtcNow;
            
            _context.OracleDemoItems.Add(item);
            await _context.SaveChangesAsync();

            // 成功建立後，回傳 201 Created，並附上取得該資源的 Location URL
            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
        }

        /// <summary>
        /// PUT: api/oracle-demo/5
        /// 更新一筆項目的資料
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] OracleDemoItem item)
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
                if (!OracleDemoItemExists(id))
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

        /// <summary>
        /// DELETE: api/oracle-demo/5
        /// 刪除一筆項目
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            OracleDemoItem? item = await _context.OracleDemoItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "找不到指定的項目，可能已被刪除" });
            }

            _context.OracleDemoItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "刪除成功" });
        }

        private bool OracleDemoItemExists(int id)
        {
            return _context.OracleDemoItems.Any(e => e.Id == id);
        }
    }
}

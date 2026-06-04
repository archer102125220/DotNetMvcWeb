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
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
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
        private readonly IOracleDemoItemService _itemService;

        // [教學註解] 這裡利用依賴注入 (DI) 來取得 Service。
        // WebAPI 和一般的 MVC Controller 可以共用同一個 Service 的商業邏輯，
        // 這樣就不會因為不同的接入點而把相同的資料庫操作寫兩遍！
        public OracleDemoApiController(IOracleDemoItemService itemService)
        {
            _itemService = itemService;
        }

        /// <summary>
        /// GET: api/oracle-demo
        /// 取得所有項目 (支援 keyword 關鍵字搜尋)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OracleDemoItem>>> GetItems([FromQuery] string? keyword = null)
        {
            List<OracleDemoItem> items = await _itemService.GetItemsAsync(keyword);
            return Ok(items);
        }

        /// <summary>
        /// GET: api/oracle-demo/5
        /// 取得單一項目的詳細資料
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OracleDemoItem>> GetItem(int id)
        {
            OracleDemoItem? item = await _itemService.GetItemByIdAsync(id, includeCategory: true);

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
            await _itemService.CreateItemAsync(item);

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
                return BadRequest(new { message = "路徑中的 ID 與內容中的 ID 不相符" });
            }

            try
            {
                await _itemService.UpdateItemAsync(item);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_itemService.ItemExists(id))
                {
                    return NotFound(new { message = "找不到指定的項目" });
                }
                else
                {
                    throw;
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
            if (!_itemService.ItemExists(id))
            {
                return NotFound(new { message = "找不到指定的項目" });
            }

            await _itemService.DeleteItemAsync(id);
            return NoContent();
        }

        /// <summary>
        /// GET: api/oracle-demo/adonet
        /// 透過原生 ADO.NET 方式取得資料
        /// </summary>
        [HttpGet("adonet")]
        public async Task<ActionResult<IEnumerable<OracleDemoItem>>> GetItemsViaAdoNet([FromQuery] string? keyword = null)
        {
            try
            {
                List<OracleDemoItem> resultList = await _itemService.GetItemsViaAdoNetAsync(keyword);
                return Ok(resultList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "執行原生 SQL 發生錯誤", details = ex.Message });
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private readonly IMssqlDemoItemService _itemService;

        // [教學註解] 這裡利用依賴注入 (DI) 來取得 Service。
        // WebAPI 和一般的 MVC Controller 可以共用同一個 Service 的商業邏輯，
        // 這樣就不會因為不同的接入點而把相同的資料庫操作寫兩遍！
        public MssqlDemoApiController(IMssqlDemoItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MssqlDemoItem>>> GetItems([FromQuery] string? keyword = null)
        {
            List<MssqlDemoItem> items = await _itemService.GetItemsAsync(keyword);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MssqlDemoItem>> GetItem(int id)
        {
            MssqlDemoItem? item = await _itemService.GetItemByIdAsync(id, includeCategory: true);

            if (item == null)
            {
                return NotFound(new { message = "找不到指定的項目" });
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<MssqlDemoItem>> CreateItem([FromBody] MssqlDemoItem item)
        {
            await _itemService.CreateItemAsync(item);

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

            try
            {
                await _itemService.UpdateItemAsync(item);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_itemService.ItemExists(id))
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
            if (!_itemService.ItemExists(id))
            {
                return NotFound(new { message = "找不到指定的項目，可能已被刪除" });
            }

            await _itemService.DeleteItemAsync(id);

            return Ok(new { message = "刪除成功" });
        }

        [HttpGet("ado-net-demo")]
        public async Task<IActionResult> AdoNetDemo([FromQuery] string? keyword = null)
        {
            try
            {
                List<MssqlDemoItem> resultList = await _itemService.GetItemsViaAdoNetAsync(keyword);
                
                // 為了與之前的 API 回傳格式相同，將 MssqlDemoItem 轉為包含 CategoryName 的匿名物件
                List<object> responseList = new List<object>();
                foreach(MssqlDemoItem item in resultList)
                {
                    responseList.Add(new {
                        Id = item.Id,
                        Name = item.Name,
                        CreatedAt = item.CreatedAt,
                        Description = item.Description,
                        CategoryId = item.CategoryId,
                        CategoryName = item.Category?.Name
                    });
                }
                
                return Ok(responseList);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

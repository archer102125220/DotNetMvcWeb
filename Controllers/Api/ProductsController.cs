using Microsoft.AspNetCore.Mvc;
using DotNetMvcWeb.Models;

namespace DotNetMvcWeb.Controllers.Api;

/// <summary>
/// 產品 API 控制器，提供產品的 CRUD 操作範例。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    /// <summary>
    /// 取得所有產品列表
    /// </summary>
    /// <returns>產品列表</returns>
    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProducts()
    {
        return Ok(ProductRepository.GetAll());
    }

    /// <summary>
    /// 根據 ID 取得特定產品
    /// </summary>
    /// <param name="id">產品 ID</param>
    /// <returns>產品詳細資訊</returns>
    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        var product = ProductRepository.GetById(id);
        
        if (product == null)
        {
            return NotFound(new { message = $"找不到 ID 為 {id} 的產品。" });
        }

        return Ok(product);
    }

    /// <summary>
    /// 新增一個產品
    /// </summary>
    /// <param name="product">產品資訊</param>
    /// <returns>新增成功後的產品</returns>
    [HttpPost]
    public ActionResult<Product> CreateProduct([FromBody] Product product)
    {
        ProductRepository.Add(product);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// 更新現有產品
    /// </summary>
    /// <param name="id">要更新的產品 ID</param>
    /// <param name="product">更新的產品內容</param>
    /// <returns>無回傳內容 (204 No Content)</returns>
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, [FromBody] Product product)
    {
        if (ProductRepository.GetById(id) == null)
        {
            return NotFound(new { message = $"找不到 ID 為 {id} 的產品，無法更新。" });
        }

        product.Id = id;
        ProductRepository.Update(product);

        return NoContent();
    }

    /// <summary>
    /// 刪除特定產品
    /// </summary>
    /// <param name="id">要刪除的產品 ID</param>
    /// <returns>無回傳內容 (204 No Content)</returns>
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        if (ProductRepository.GetById(id) == null)
        {
            return NotFound(new { message = $"找不到 ID 為 {id} 的產品，無法刪除。" });
        }

        ProductRepository.Delete(id);

        return NoContent();
    }
}

namespace DotNetMvcWeb.Models;

/// <summary>
/// 產品實體模型 (Product Entity Model)
/// </summary>
public class Product
{
    /// <summary>
    /// 產品唯一識別碼
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 產品名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 產品價格
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 產品描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

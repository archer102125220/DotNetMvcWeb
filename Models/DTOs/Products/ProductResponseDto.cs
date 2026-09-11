using System.Globalization;
using DotNetMvcWeb.Models;

namespace DotNetMvcWeb.Models.DTOs.Products
{
    /// <summary>
    /// 產品資料回應物件 (Product Response DTO)
    /// ------------------------------------------------------------------------------------------------
    /// 💡 為什麼回傳資料給前端時也推薦使用 Response DTO？
    /// 1. 🔒 資訊隱藏：防止機密或內部欄位（如軟刪除標記 IsDeleted、內部審計欄位等）外洩。
    /// 2. 🎨 格式化輔助：提供前端友善的計算欄位（例如 FormattedPrice "NT$ 1,000"），減輕前端渲染負擔。
    /// 3. 📦 封裝對映邏輯：透過靜態工廠方法 FromEntity 進行清晰轉換，保持 Controller 精簡。
    /// ------------------------------------------------------------------------------------------------
    /// </summary>
    public class ProductResponseDto
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
        /// 格式化後的幣別顯示字串 (例如 "NT$ 999.00")
        /// </summary>
        public string FormattedPrice => Price.ToString("C", CultureInfo.CurrentCulture);

        /// <summary>
        /// 產品詳細描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 從 Product 實體模型轉換為 ProductResponseDto 的工廠方法
        /// </summary>
        /// <param name="product">來源產品實體</param>
        /// <returns>轉換後的 ProductResponseDto 物件</returns>
        public static ProductResponseDto FromEntity(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description
            };
        }
    }
}

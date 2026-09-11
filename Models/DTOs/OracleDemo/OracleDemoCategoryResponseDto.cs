using DotNetMvcWeb.Models;

namespace DotNetMvcWeb.Models.DTOs.OracleDemo
{
    /// <summary>
    /// Oracle 示範分類回應資料傳輸物件 (OracleDemoCategory Response DTO)
    /// 用於回傳分類資訊與包含的項目統計
    /// </summary>
    public class OracleDemoCategoryResponseDto
    {
        /// <summary>
        /// 分類唯一識別碼
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 分類名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 建立時間 (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 分類底下的示範項目數量
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// 從 OracleDemoCategory 實體轉換為 Response DTO 的工廠方法
        /// </summary>
        /// <param name="category">來源分類實體</param>
        /// <returns>轉換後的 OracleDemoCategoryResponseDto 物件</returns>
        public static OracleDemoCategoryResponseDto FromEntity(OracleDemoCategory category)
        {
            ArgumentNullException.ThrowIfNull(category);

            return new OracleDemoCategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAt = category.CreatedAt,
                ItemCount = category.Items?.Count ?? 0
            };
        }
    }
}

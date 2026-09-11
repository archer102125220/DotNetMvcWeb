using DotNetMvcWeb.Models;

namespace DotNetMvcWeb.Models.DTOs.OracleDemo
{
    /// <summary>
    /// [教學示範：Oracle 項目回應 DTO 與物件關聯扁平化]
    /// ------------------------------------------------------------------------------------------------
    /// 💡 為什麼回傳關聯資料時強烈建議使用 Response DTO？
    /// 
    /// 1. 🔄 避免物件循環引用 (Object Cycle / Circular Reference)：
    ///    - EF Core 的 Entity 模型中，OracleDemoItem 含有 Category 導覽屬性，而 OracleDemoCategory 又含有 Items 集合。
    ///    - 若直接將 Entity 序列化為 JSON，會因為相互引用引發 System.Text.Json.JsonException: A possible object cycle was detected。
    ///    - 透過 Response DTO，我們將關聯資料「扁平化」為純文字屬性（如 CategoryName），徹底解決循環引用問題！
    /// 
    /// 2. ⚡ 避免 Lazy Loading / N+1 查詢與效能黑洞：
    ///    - 若使用 Entity，序列化器在讀取導覽屬性時可能在背後觸發不必要的資料庫查詢。
    ///    - 使用 Response DTO 能明確定義需要傳輸的欄位，提升 API 回應速度。
    /// ------------------------------------------------------------------------------------------------
    /// </summary>
    public class OracleDemoItemResponseDto
    {
        /// <summary>
        /// 項目唯一識別碼
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 項目名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 項目描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 建立時間 (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 所屬分類 ID
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 所屬分類名稱 (扁平化關聯欄位，若未設定分類則為 null)
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// 從 OracleDemoItem 實體轉換為 Response DTO 的工廠方法
        /// </summary>
        /// <param name="item">來源項目實體</param>
        /// <returns>轉換後的 OracleDemoItemResponseDto 物件</returns>
        public static OracleDemoItemResponseDto FromEntity(OracleDemoItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            return new OracleDemoItemResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                CreatedAt = item.CreatedAt,
                CategoryId = item.CategoryId,
                CategoryName = item.Category?.Name
            };
        }
    }
}

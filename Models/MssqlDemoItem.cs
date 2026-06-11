namespace DotNetMvcWeb.Models
{
    /// <summary>
    /// MSSQL 資料庫示範用的實體模型 (Entity Model)
    /// 對應到資料庫中的 MssqlDemoItems 資料表
    /// </summary>
    public class MssqlDemoItem
    {
        /// <summary>
        /// 唯一識別碼 (Primary Key)
        /// EF Core 預設會將名為 Id 的屬性設為主鍵，並自動遞增
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 項目名稱 (必填)
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 項目描述 (選填，可為 Null)
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// 建立時間
        /// 預設值為當前的 UTC 時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 分類 ID (Foreign Key)
        /// 選填，允許項目沒有分類
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 關聯的分類實體 (Navigation Property)
        /// </summary>
        public MssqlDemoCategory? Category { get; set; }
    }
}

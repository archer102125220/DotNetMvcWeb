namespace DotNetMvcWeb.Models
{
    /// <summary>
    /// Oracle 資料庫示範用的分類實體模型 (Entity Model)
    /// 對應到資料庫中的 OracleDemoCategories 資料表，用來展示一對多關聯
    /// </summary>
    public class OracleDemoCategory
    {
        /// <summary>
        /// 唯一識別碼 (Primary Key)
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 分類名稱 (必填)
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 關聯的示範項目集合 (一對多)
        /// </summary>
        public ICollection<OracleDemoItem> Items { get; set; } = new List<OracleDemoItem>();
    }
}

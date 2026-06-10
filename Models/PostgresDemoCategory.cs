namespace DotNetMvcWeb.Models
{
    /// <summary>
    /// Postgres 資料庫示範用的分類實體模型 (Entity Model)
    /// </summary>
    public class PostgresDemoCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<PostgresDemoItem> Items { get; set; } = new List<PostgresDemoItem>();
    }
}

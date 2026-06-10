namespace DotNetMvcWeb.Models
{
    /// <summary>
    /// Postgres 資料庫示範用的實體模型 (Entity Model)
    /// </summary>
    public class PostgresDemoItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CategoryId { get; set; }
        public PostgresDemoCategory? Category { get; set; }
    }
}

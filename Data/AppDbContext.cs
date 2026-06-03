using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Data
{
    /// <summary>
    /// 應用程式的資料庫上下文 (DbContext)
    /// 負責與 Entity Framework Core 溝通，進行資料庫操作
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// 定義 OracleDemoItem 對應的資料表
        /// EF Core 會依據此屬性名稱 (OracleDemoItems) 來建立與查詢資料表
        /// </summary>
        public DbSet<Models.OracleDemoItem> OracleDemoItems { get; set; }
        
        /// <summary>
        /// 定義 OracleDemoCategory 對應的資料表
        /// </summary>
        public DbSet<Models.OracleDemoCategory> OracleDemoCategories { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // 可以在此處設定資料表欄位的進階限制或關聯
            // 例如限制 Name 欄位必填且最大長度為 200
            modelBuilder.Entity<Models.OracleDemoItem>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            // 建立示範用的分類 Seed Data
            modelBuilder.Entity<Models.OracleDemoCategory>().HasData(
                new Models.OracleDemoCategory
                {
                    Id = 1,
                    Name = "一般",
                    CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new Models.OracleDemoCategory
                {
                    Id = 2,
                    Name = "重要",
                    CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
                }
            );

            // 建立示範用的 Seed Data
            modelBuilder.Entity<Models.OracleDemoItem>().HasData(
                new Models.OracleDemoItem
                {
                    Id = 1,
                    Name = "測試示範項目 1",
                    Description = "這是第一筆透過 EF Core Seed 建立的測試資料。",
                    CategoryId = 1,
                    CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new Models.OracleDemoItem
                {
                    Id = 2,
                    Name = "測試示範項目 2",
                    Description = "示範如何在 Oracle 資料庫中儲存繁體中文內容。",
                    CategoryId = 2,
                    CreatedAt = new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc)
                },
                new Models.OracleDemoItem
                {
                    Id = 3,
                    Name = "教學用項目",
                    Description = "可以嘗試在畫面上點擊編輯或刪除這筆資料，測試 HTMX 的互動效果！",
                    CategoryId = null,
                    CreatedAt = new DateTime(2026, 6, 2, 12, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}

using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Data
{
    /// <summary>
    /// 應用程式的 MySQL 資料庫上下文 (DbContext)
    /// 負責與 Entity Framework Core 溝通，進行 MySQL 資料庫操作
    /// </summary>
    public class MysqlDbContext : DbContext
    {
        public MysqlDbContext(DbContextOptions<MysqlDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// 定義 MysqlDemoItem 對應的資料表
        /// </summary>
        public DbSet<Models.MysqlDemoItem> MysqlDemoItems { get; set; }
        
        /// <summary>
        /// 定義 MysqlDemoCategory 對應的資料表
        /// </summary>
        public DbSet<Models.MysqlDemoCategory> MysqlDemoCategories { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // 可以在此處設定資料表欄位的進階限制或關聯
            // 例如限制 Name 欄位必填且最大長度為 200
            modelBuilder.Entity<Models.MysqlDemoItem>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            // 建立示範用的分類 Seed Data
            modelBuilder.Entity<Models.MysqlDemoCategory>().HasData(
                new Models.MysqlDemoCategory
                {
                    Id = 1,
                    Name = "一般",
                    CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new Models.MysqlDemoCategory
                {
                    Id = 2,
                    Name = "重要",
                    CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
                }
            );

            // 建立示範用的 Seed Data
            modelBuilder.Entity<Models.MysqlDemoItem>().HasData(
                new Models.MysqlDemoItem
                {
                    Id = 1,
                    Name = "測試示範項目 1",
                    Description = "這是第一筆透過 EF Core Seed 建立的測試資料。",
                    CategoryId = 1,
                    CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new Models.MysqlDemoItem
                {
                    Id = 2,
                    Name = "測試示範項目 2",
                    Description = "示範如何在 MySQL 資料庫中儲存繁體中文內容。",
                    CategoryId = 2,
                    CreatedAt = new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc)
                },
                new Models.MysqlDemoItem
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

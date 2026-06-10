using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Data
{
    /// <summary>
    /// Postgres 應用程式的資料庫上下文 (DbContext)
    /// 負責與 Entity Framework Core 溝通，進行資料庫操作
    /// </summary>
    public class PostgresDbContext : DbContext
    {
        public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options)
        {
        }

        public DbSet<Models.PostgresDemoItem> PostgresDemoItems { get; set; }
        
        public DbSet<Models.PostgresDemoCategory> PostgresDemoCategories { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Models.PostgresDemoItem>()
                .Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            // 建立示範用的分類 Seed Data
            modelBuilder.Entity<Models.PostgresDemoCategory>().HasData(
                new Models.PostgresDemoCategory
                {
                    Id = 1,
                    Name = "一般 (PG)",
                    CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new Models.PostgresDemoCategory
                {
                    Id = 2,
                    Name = "重要 (PG)",
                    CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
                }
            );

            // 建立示範用的 Seed Data
            modelBuilder.Entity<Models.PostgresDemoItem>().HasData(
                new Models.PostgresDemoItem
                {
                    Id = 1,
                    Name = "測試示範項目 1 (PG)",
                    Description = "這是第一筆透過 EF Core Seed 建立的 Postgres 測試資料。",
                    CategoryId = 1,
                    CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new Models.PostgresDemoItem
                {
                    Id = 2,
                    Name = "測試示範項目 2 (PG)",
                    Description = "示範如何在 Postgres 資料庫中儲存內容。",
                    CategoryId = 2,
                    CreatedAt = new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc)
                },
                new Models.PostgresDemoItem
                {
                    Id = 3,
                    Name = "教學用項目 (PG)",
                    Description = "測試 HTMX 互動效果的 Postgres 範例資料！",
                    CategoryId = null,
                    CreatedAt = new DateTime(2026, 6, 2, 12, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}

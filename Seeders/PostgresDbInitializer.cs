using System;
using System.Linq;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Seeders
{
    public static class PostgresDbInitializer
    {
        public static void Initialize(PostgresDbContext context)
        {
            // 自動執行未套用的 Migration
            context.Database.Migrate();

            // 檢查是否已經存在我們即將動態寫入的資料
            if (context.PostgresDemoItems.Any(i => i.Name == "動態種子資料 1 (PG)"))
            {
                return;   
            }

            // 建立要動態寫入的資料，不指定 ID 讓 DB Sequence 自動產生
            PostgresDemoItem[] dynamicItems = new PostgresDemoItem[]
            {
                new PostgresDemoItem 
                { 
                    Name = "動態種子資料 1 (PG)", 
                    Description = "這筆資料是專門示範從獨立 Seeder 資料夾寫入的", 
                    CreatedAt = DateTime.UtcNow 
                },
                new PostgresDemoItem 
                { 
                    Name = "動態種子資料 2 (PG)", 
                    Description = "這種寫法在實務上很適合用來生成大量需要隨機時間的假資料", 
                    CreatedAt = DateTime.UtcNow.AddHours(-2) 
                }
            };

            // 寫入資料庫
            context.PostgresDemoItems.AddRange(dynamicItems);
            context.SaveChanges();
        }
    }
}

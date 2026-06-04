using System;
using System.Linq;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;

namespace DotNetMvcWeb.Seeders
{
    /// <summary>
    /// 第二種 Seed Data 做法：獨立的 MySQL 資料庫初始化器
    /// 適用於：產生大量動態假資料、隨機時間、或是串接外部 API 取得初始資料。
    /// 這裡的資料不會被寫死進 Migration 檔案中。
    /// </summary>
    public static class MysqlDbInitializer
    {
        public static void Initialize(MysqlDbContext context)
        {
            // 由於在 MysqlDbContext (HasData) 中我們已經靜態產生了 ID 1~3 的資料
            // 這裡我們示範如何用程式邏輯動態產生額外的測試資料。
            
            // 檢查是否已經存在我們即將動態寫入的資料，如果有了就直接 return
            if (context.MysqlDemoItems.Any(i => i.Name == "動態種子資料 1"))
            {
                return;   
            }

            // 建立要動態寫入的資料
            MysqlDemoItem[] dynamicItems = new MysqlDemoItem[]
            {
                new MysqlDemoItem 
                { 
                    Id = 4, // 明確指定 ID 以避免 Identity 序號與前面 Migration 的固定 ID 衝突
                    Name = "動態種子資料 1", 
                    Description = "這筆資料是專門示範從獨立 Seeder 資料夾寫入 MySQL 的", 
                    CreatedAt = DateTime.UtcNow 
                },
                new MysqlDemoItem 
                { 
                    Id = 5,
                    Name = "動態種子資料 2", 
                    Description = "這種寫法在實務上很適合用來生成大量需要隨機時間的假資料", 
                    CreatedAt = DateTime.UtcNow.AddHours(-2) 
                }
            };

            // 寫入資料庫
            context.MysqlDemoItems.AddRange(dynamicItems);
            context.SaveChanges();
        }
    }
}

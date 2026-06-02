using System;
using System.Linq;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;

namespace DotNetMvcWeb.Seeders
{
    /// <summary>
    /// 第二種 Seed Data 做法：獨立的資料庫初始化器
    /// 適用於：產生大量動態假資料、隨機時間、或是串接外部 API 取得初始資料。
    /// 這裡的資料不會被寫死進 Migration 檔案中。
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // 由於在 AppDbContext (HasData) 中我們已經靜態產生了 ID 1~3 的資料
            // 這裡我們示範如何用程式邏輯動態產生額外的測試資料。
            
            // 檢查是否已經存在我們即將動態寫入的資料，如果有了就直接 return
            if (context.OracleDemoItems.Any(i => i.Name == "動態種子資料 1"))
            {
                return;   
            }

            // 建立要動態寫入的資料
            // 在這種作法下，我們就可以自由使用 DateTime.UtcNow 或其他動態函式，
            // 甚至可以使用像是 Bogus 這樣的套件來生成隨機的假資料。
            var dynamicItems = new OracleDemoItem[]
            {
                new OracleDemoItem 
                { 
                    Id = 4, // 明確指定 ID 以避免 Oracle Identity 序號與前面 Migration 的固定 ID 衝突
                    Name = "動態種子資料 1", 
                    Description = "這筆資料是專門示範從獨立 Seeder 資料夾寫入的", 
                    CreatedAt = DateTime.UtcNow 
                },
                new OracleDemoItem 
                { 
                    Id = 5,
                    Name = "動態種子資料 2", 
                    Description = "這種寫法在實務上很適合用來生成大量需要隨機時間的假資料", 
                    CreatedAt = DateTime.UtcNow.AddHours(-2) 
                }
            };

            // 寫入資料庫
            context.OracleDemoItems.AddRange(dynamicItems);
            context.SaveChanges();
        }
    }
}

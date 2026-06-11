using System;
using System.Linq;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Seeders
{
    /// <summary>
    /// 第二種 Seed Data 做法：獨立的資料庫初始化器
    /// 適用於：產生大量動態假資料、隨機時間、或是串接外部 API 取得初始資料。
    /// 這裡的資料不會被寫死進 Migration 檔案中。
    /// </summary>
    public static class MssqlDbInitializer
    {
        public static void Initialize(MssqlDbContext context)
        {
            // 在開發環境下，如果需要自動建立資料庫並套用 Migration
            // context.Database.Migrate(); // 或者 EnsureCreated();

            // 檢查是否已經存在我們即將動態寫入的資料，如果有了就直接 return
            if (context.MssqlDemoItems.Any(i => i.Name == "動態種子資料 1"))
            {
                return;   
            }

            // 建立要動態寫入的資料
            MssqlDemoItem[] dynamicItems = new MssqlDemoItem[]
            {
                new MssqlDemoItem 
                { 
                    Name = "動態種子資料 1", 
                    Description = "這筆資料是專門示範從獨立 Seeder 資料夾寫入的", 
                    CreatedAt = DateTime.UtcNow 
                },
                new MssqlDemoItem 
                { 
                    Name = "動態種子資料 2", 
                    Description = "這種寫法在實務上很適合用來生成大量需要隨機時間的假資料", 
                    CreatedAt = DateTime.UtcNow.AddHours(-2) 
                }
            };

            // 注意：MSSQL 中 Identity column 若已啟用，動態塞入的 id 若不寫 SET IDENTITY_INSERT 會出錯，
            // 所以這裡我們不手動指定 Id。

            // 寫入資料庫
            context.MssqlDemoItems.AddRange(dynamicItems);
            context.SaveChanges();
        }
    }
}

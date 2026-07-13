using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DotNetMvcWeb.Services.Implements
{
    /// <summary>
    /// [教學註解] 服務層實作 (Service Implementation)
    /// 這裡是實際處理商業邏輯與資料庫互動的地方。將原本寫在 Controller 裡的 DbContext 操作全部集中於此。
    /// 這種做法稱為「Service Layer Pattern」或是簡化版的 Repository Pattern。
    /// </summary>
    public class PostgresDemoItemService : IPostgresDemoItemService
    {
        private readonly PostgresDbContext _context;

        public PostgresDemoItemService(PostgresDbContext context)
        {
            _context = context;
        }

        public async Task<List<PostgresDemoItem>> GetItemsAsync(string? keyword = null)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                // ⚠️ 深度檢查注意：使用 FromSqlInterpolated 會自動進行參數化，安全防止 SQL Injection。
                return await _context.PostgresDemoItems
                    .FromSqlInterpolated($"SELECT * FROM \"PostgresDemoItems\" WHERE \"Name\" LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }

            return await _context.PostgresDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<PostgresDemoItem?> GetItemByIdAsync(int id, bool includeCategory = false)
        {
            IQueryable<PostgresDemoItem> query = _context.PostgresDemoItems;

            if (includeCategory)
            {
                query = query.Include(i => i.Category);
            }

            return await query.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task CreateItemAsync(PostgresDemoItem item)
        {
            if (item.CreatedAt == default)
            {
                item.CreatedAt = DateTime.UtcNow;
            }
            _context.PostgresDemoItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(PostgresDemoItem item)
        {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(int id)
        {
            PostgresDemoItem? item = await _context.PostgresDemoItems.FindAsync(id);
            if (item != null)
            {
                _context.PostgresDemoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public bool ItemExists(int id)
        {
            return _context.PostgresDemoItems.Any(e => e.Id == id);
        }

        public async Task<List<PostgresDemoItem>> GetItemsViaAdoNetAsync(string? keyword = null)
        {
            // [教學註解] 雖然我們示範的是原生 ADO.NET，但還是可以直接利用已經設定在 DbContext 內的連線字串
            // 這讓我們不用去 appsettings.json 裡面手動剖析 (parse) IConfiguration。
            string? connectionString = _context.Database.GetConnectionString();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("無法從 DbContext 取得連接字串");
            }

            List<PostgresDemoItem> resultList = new List<PostgresDemoItem>();

            // ⚠️ 深度檢查注意：必須使用 await using 包覆 IDisposable 物件
            await using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                // [教學註解] Async First 政策：不要使用 connection.Open()，以免在流量大時阻塞執行緒 (Thread starvation)。
                await connection.OpenAsync();

                // [教學註解] 建立要送到 Postgres 執行的指令物件 (Command)
                await using (NpgsqlCommand command = connection.CreateCommand())
                {
                    // [教學註解] 撰寫原生 SQL 查詢
                    string sqlText = """
                        SELECT 
                            item."Id", 
                            item."Name", 
                            item."CreatedAt", 
                            item."Description", 
                            item."CategoryId", 
                            category."Name" AS "CategoryName"
                        FROM "PostgresDemoItems" item
                        LEFT JOIN "PostgresDemoCategories" category ON item."CategoryId" = category."Id"
                    """;

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sqlText += " WHERE item.\"Name\" LIKE @keyword";
                        // [教學註解] 防止 SQL Injection：必須使用參數化查詢
                        command.Parameters.Add(new NpgsqlParameter("keyword", $"%{keyword}%"));
                    }

                    sqlText += " ORDER BY item.\"CreatedAt\" DESC";
                    command.CommandText = sqlText;

                    // [教學註解] ExecuteReaderAsync 會回傳一個 DataReader，這是一種流式 (Streaming) 讀取方式。
                    // 它不會一次把幾百萬筆資料塞爆記憶體，而是透過 ReadAsync() 逐筆向資料庫要資料。
                    await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            PostgresDemoItem item = new PostgresDemoItem
                            {
                                // [教學註解] 透過索引值取出對應的欄位，這是最快的
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                CreatedAt = reader.GetDateTime(2),
                                // [教學註解] 原生讀取資料時，必須先呼叫 IsDBNull 檢查
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CategoryId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                            };

                            if (!reader.IsDBNull(5))
                            {
                                item.Category = new PostgresDemoCategory { Name = reader.GetString(5) };
                            }

                            resultList.Add(item);
                        }
                    }
                }
            }

            return resultList;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DotNetMvcWeb.Services.Implements
{
    /// <summary>
    /// [教學註解] 服務層實作 (Service Implementation)
    /// 這裡是實際處理商業邏輯與資料庫互動的地方。將原本寫在 Controller 裡的 DbContext 操作全部集中於此。
    /// 這種做法稱為「Service Layer Pattern」或是簡化版的 Repository Pattern。
    /// </summary>
    public class MssqlDemoItemService : IMssqlDemoItemService
    {
        private readonly MssqlDbContext _context;

        public MssqlDemoItemService(MssqlDbContext context)
        {
            _context = context;
        }

        public async Task<List<MssqlDemoItem>> GetItemsAsync(string? keyword = null)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                // ⚠️ 深度檢查注意：使用 FromSqlInterpolated 會自動進行參數化，安全防止 SQL Injection。
                // 另外，查詢後仍必須加上 .AsNoTracking() 來進行唯讀優化。
                return await _context.MssqlDemoItems
                    .FromSqlInterpolated($"SELECT * FROM [MssqlDemoItems] WHERE [Name] LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }

            return await _context.MssqlDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<MssqlDemoItem?> GetItemByIdAsync(int id, bool includeCategory = false)
        {
            IQueryable<MssqlDemoItem> query = _context.MssqlDemoItems;

            if (includeCategory)
            {
                query = query.Include(i => i.Category);
            }

            // Using AsNoTracking here makes it safer for API endpoints that just return data.
            // But if it's used for Edit in MVC before Update, EF Core's Update() handles detached entities anyway.
            return await query.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task CreateItemAsync(MssqlDemoItem item)
        {
            if (item.CreatedAt == default)
            {
                item.CreatedAt = DateTime.UtcNow;
            }
            _context.MssqlDemoItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(MssqlDemoItem item)
        {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(int id)
        {
            MssqlDemoItem? item = await _context.MssqlDemoItems.FindAsync(id);
            if (item != null)
            {
                _context.MssqlDemoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public bool ItemExists(int id)
        {
            return _context.MssqlDemoItems.Any(e => e.Id == id);
        }

        public async Task<List<MssqlDemoItem>> GetItemsViaAdoNetAsync(string? keyword = null)
        {
            // [教學註解] 這裡直接利用已經設定在 DbContext 內的連線字串
            string? connectionString = _context.Database.GetConnectionString();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("無法取得資料庫連接字串");
            }

            var resultList = new List<MssqlDemoItem>();

            // ⚠️ 深度檢查注意：必須使用 await using 包覆 IDisposable 物件 (SqlConnection, SqlCommand, SqlDataReader)
            // 原生的 ADO.NET 操作需要開發者自行負責釋放連線。如果忘記 using，會造成 Connection Pool 被耗盡。
            await using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // [教學註解] Async First 政策：所有資料庫操作都必須使用 Async 版本。
                await connection.OpenAsync();

                // [教學註解] 建立要送到 MSSQL 執行的指令物件 (Command)
                await using (SqlCommand command = connection.CreateCommand())
                {
                    // [教學註解] 撰寫原生 SQL 查詢，這裡示範了如何做 JOIN。
                    // ⚠️ 注意：MSSQL 中，使用中括號 [] 可以避免與保留字衝突，並明確指定物件名稱。
                    string sqlText = """
                        SELECT 
                            item.[Id], 
                            item.[Name], 
                            item.[CreatedAt], 
                            item.[Description], 
                            item.[CategoryId], 
                            category.[Name] AS [CategoryName]
                        FROM [MssqlDemoItems] item
                        LEFT JOIN [MssqlDemoCategories] category ON item.[CategoryId] = category.[Id]
                    """;

                    // [教學註解] 動態加入搜尋條件 (WHERE)
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sqlText += " WHERE item.[Name] LIKE @keyword";
                        // [教學註解] ⚠️ 絕對禁止字串拼接！必須使用 Parameter 參數化查詢，防止 SQL Injection (隱碼攻擊)
                        command.Parameters.Add(new SqlParameter("@keyword", $"%{keyword}%"));
                    }

                    sqlText += " ORDER BY item.[CreatedAt] DESC";
                    command.CommandText = sqlText;

                    // [教學註解] ExecuteReaderAsync 會開啟資料流讀取器，回傳一個 DataReader。
                    // 這是一種流式 (Streaming) 讀取方式，不會一次把幾百萬筆資料塞爆記憶體。
                    await using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // [教學註解] ReadAsync() 會逐筆將資料拉到應用程式記憶體中。
                        while (await reader.ReadAsync())
                        {
                            var item = new MssqlDemoItem
                            {
                                // [教學註解] 透過索引值取出對應的欄位，這是最快的。
                                // 如果要用欄位名稱取值，可以使用 reader.GetOrdinal("Id") 取得索引。
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                // MSSQL DATETIME 或 DATETIME2 型別可以直接轉換為 C# 的 DateTime
                                CreatedAt = reader.GetDateTime(2),
                                // [教學註解] 原生讀取資料時，針對可能為 NULL 的欄位，必須先呼叫 IsDBNull 進行檢查。
                                // 否則呼叫 GetString 或 GetInt32 時會引發 SqlNullValueException！
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CategoryId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                            };

                            // [教學註解] 如果有對應的 CategoryName，就手動建構關聯物件 (Navigation Property)
                            if (!reader.IsDBNull(5))
                            {
                                item.Category = new MssqlDemoCategory { Name = reader.GetString(5) };
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

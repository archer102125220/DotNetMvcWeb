using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace DotNetMvcWeb.Repositories.Implements
{
    /// <summary>
    /// [教學註解] 倉儲層實作 (Repository Implementation)
    /// 這裡封裝所有與 Oracle 資料庫存取的底層細節，包含：
    /// 1. EF Core LINQ 查詢與異動 (AsNoTracking, Include, FromSqlInterpolated)
    /// 2. 原生 ADO.NET 批次/流式查詢 (OracleConnection, OracleCommand, DbDataReader)
    /// 3. 預存程序 (Stored Procedure) 執行
    /// </summary>
    public class OracleDemoItemRepository : IOracleDemoItemRepository
    {
        private readonly AppDbContext _context;

        public OracleDemoItemRepository(AppDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _context = context;
        }

        public async Task<List<OracleDemoItem>> GetItemsAsync(string? keyword = null)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                // ⚠️ 深度檢查注意：使用 FromSqlInterpolated 會自動進行參數化，安全防止 SQL Injection。
                // 另外，查詢後仍必須加上 .AsNoTracking() 來進行唯讀優化。
                return await _context.OracleDemoItems
                    .FromSqlInterpolated($"SELECT * FROM \"OracleDemoItems\" WHERE \"Name\" LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }

            return await _context.OracleDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<OracleDemoItem?> GetItemByIdAsync(int id, bool includeCategory = false)
        {
            IQueryable<OracleDemoItem> query = _context.OracleDemoItems;

            if (includeCategory)
            {
                query = query.Include(i => i.Category);
            }

            return await query.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task AddItemAsync(OracleDemoItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            _context.OracleDemoItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(OracleDemoItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(int id)
        {
            OracleDemoItem? item = await _context.OracleDemoItems.FindAsync(id);
            if (item is not null)
            {
                _context.OracleDemoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public bool ItemExists(int id)
        {
            return _context.OracleDemoItems.Any(e => e.Id == id);
        }

        public async Task<List<OracleDemoItem>> GetItemsViaAdoNetAsync(string? keyword = null)
        {
            string? connectionString = _context.Database.GetConnectionString();

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("無法取得資料庫連接字串");
            }

            List<OracleDemoItem> resultList = new();

            // ⚠️ 深度檢查注意：必須使用 await using 包覆 IDisposable 物件 (OracleConnection, OracleCommand, DbDataReader)
            // 由於資料庫連線是非常昂貴的資源，務必要確保執行完畢或發生例外時，連線能被正確關閉與釋放 (Dispose)。
            await using (OracleConnection connection = new(connectionString))
            {
                // [教學註解] Async First 政策：不要使用 connection.Open()，以免在流量大時阻塞執行緒 (Thread starvation)。
                await connection.OpenAsync();

                // [教學註解] 建立要送到 Oracle 執行的指令物件 (Command)
                await using (OracleCommand command = connection.CreateCommand())
                {
                    command.BindByName = true;

                    // [教學註解] 撰寫原生 SQL 查詢，這裡同樣示範如何做 JOIN。
                    // ⚠️ 注意：Oracle 資料庫中，如果資料表或欄位名稱被 EF Core 加上了雙引號 (強迫區分大小寫)，
                    // 這裡的原生 SQL 也必須加上雙引號 (例如 \"Id\")，否則會發生 ORA-00904: invalid identifier 錯誤。
                    string sqlText = """
                        SELECT
                            item."Id",
                            item."Name",
                            item."CreatedAt",
                            item."Description",
                            item."CategoryId",
                            category."Name" AS "CategoryName"
                        FROM "OracleDemoItems" item
                        LEFT JOIN "OracleDemoCategories" category ON item."CategoryId" = category."Id"
                    """;

                    // [教學註解] 動態加入搜尋條件 (WHERE)
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sqlText += " WHERE item.\"Name\" LIKE :keyword";
                        // [教學註解] ⚠️ 絕對禁止字串拼接！必須使用 Parameter 參數化查詢，防止 SQL Injection (隱碼攻擊)
                        command.Parameters.Add(new OracleParameter("keyword", $"%{keyword}%"));
                    }

                    sqlText += " ORDER BY item.\"CreatedAt\" DESC";
                    command.CommandText = sqlText;

                    // [教學註解] ExecuteReaderAsync 會開啟資料流讀取器，回傳一個 DataReader。
                    // 這是一種流式 (Streaming) 讀取方式，不會一次把幾百萬筆資料塞爆記憶體。
                    await using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // [教學註解] ReadAsync() 會逐筆將資料拉到應用程式記憶體中。
                        while (await reader.ReadAsync())
                        {
                            OracleDemoItem item = new()
                            {
                                // [教學註解] 透過索引值取出對應的欄位，這是最快的。
                                // 如果要用欄位名稱取值，可以使用 reader.GetOrdinal("Id") 取得索引。
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                // Oracle DATE 或 TIMESTAMP 型別可以轉換為 C# 的 DateTime
                                CreatedAt = reader.GetDateTime(2),
                                // [教學註解] 原生讀取資料時，針對可能為 NULL 的欄位，必須先呼叫 IsDBNull 進行檢查。
                                // 否則呼叫 GetString 或 GetInt32 時會引發異常！
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CategoryId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                            };

                            // [教學註解] 如果有對應的 CategoryName，就手動建構關聯物件 (Navigation Property)
                            if (!reader.IsDBNull(5))
                            {
                                item.Category = new OracleDemoCategory { Name = reader.GetString(5) };
                            }

                            resultList.Add(item);
                        }
                    }
                }
            }

            return resultList;
        }

        public async Task UpdateItemDescriptionViaProcAsync(int id, string newDescription)
        {
            // ⚠️ 深度檢查注意：使用 ExecuteSqlInterpolatedAsync 會自動處理參數化，防止 SQL Injection
            // 在 Oracle 中呼叫 Procedure 習慣使用 BEGIN ... END; 包住
            await _context.Database.ExecuteSqlInterpolatedAsync($"BEGIN SP_UPDATE_ITEM_DESCRIPTION({id}, {newDescription}); END;");
        }
    }
}

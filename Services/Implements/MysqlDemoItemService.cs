using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using DotNetMvcWeb.Data;
using DotNetMvcWeb.Models;
using DotNetMvcWeb.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace DotNetMvcWeb.Services.Implements
{
    /// <summary>
    /// [教學註解] 服務層實作 (Service Implementation)
    /// 這裡是實際處理商業邏輯與資料庫互動的地方。將原本寫在 Controller 裡的 DbContext 操作全部集中於此。
    /// 這種做法稱為「Service Layer Pattern」或是簡化版的 Repository Pattern。
    /// </summary>
    public class MysqlDemoItemService : IMysqlDemoItemService
    {
        private readonly MysqlDbContext _context;

        public MysqlDemoItemService(MysqlDbContext context)
        {
            _context = context;
        }

        public async Task<List<MysqlDemoItem>> GetItemsAsync(string? keyword = null)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchPattern = $"%{keyword}%";
                // ⚠️ 深度檢查注意：使用 FromSqlInterpolated 會自動進行參數化，安全防止 SQL Injection。
                // 注意 MySQL 表名不一定需要加上雙引號，但為了安全與習慣我們使用反引號 ` 或交由 EF Core
                return await _context.MysqlDemoItems
                    .FromSqlInterpolated($"SELECT * FROM `MysqlDemoItems` WHERE `Name` LIKE {searchPattern}")
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }

            return await _context.MysqlDemoItems
                .Include(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<MysqlDemoItem?> GetItemByIdAsync(int id, bool includeCategory = false)
        {
            IQueryable<MysqlDemoItem> query = _context.MysqlDemoItems;

            if (includeCategory)
            {
                query = query.Include(i => i.Category);
            }

            return await query.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task CreateItemAsync(MysqlDemoItem item)
        {
            if (item.CreatedAt == default)
            {
                item.CreatedAt = DateTime.UtcNow;
            }
            _context.MysqlDemoItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(MysqlDemoItem item)
        {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(int id)
        {
            MysqlDemoItem? item = await _context.MysqlDemoItems.FindAsync(id);
            if (item != null)
            {
                _context.MysqlDemoItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public bool ItemExists(int id)
        {
            return _context.MysqlDemoItems.Any(e => e.Id == id);
        }

        public async Task<List<MysqlDemoItem>> GetItemsViaAdoNetAsync(string? keyword = null)
        {
            // [教學註解] 雖然我們示範的是原生 ADO.NET，但還是可以直接利用已經設定在 DbContext 內的連線字串
            // 這讓我們不用去 appsettings.json 裡面手動剖析 (parse) IConfiguration。
            string? connectionString = _context.Database.GetConnectionString();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("無法從 DbContext 取得連接字串");
            }

            List<MysqlDemoItem> resultList = new List<MysqlDemoItem>();

            // ⚠️ 深度檢查注意：必須使用 await using 包覆 IDisposable 物件 (MySqlConnection, MySqlCommand, DbDataReader)
            // 原生的 ADO.NET 操作需要開發者自行負責釋放連線。如果忘記 using，會造成 Connection Pool 被耗盡。
            await using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // [教學註解] Async First 政策：不要使用 connection.Open()，以免在流量大時阻塞執行緒 (Thread starvation)。
                await connection.OpenAsync();

                // [教學註解] 建立要送到 MySQL 執行的指令物件 (Command)
                await using (MySqlCommand command = connection.CreateCommand())
                {
                    // [教學註解] 撰寫原生 SQL 查詢，這裡示範了如何做 JOIN。
                    // ⚠️ 注意：MySQL 對於保留字和欄位名稱會習慣使用反引號 ` 包起來。
                    string sqlText = """
                        SELECT 
                            item.`Id`, 
                            item.`Name`, 
                            item.`CreatedAt`, 
                            item.`Description`, 
                            item.`CategoryId`, 
                            category.`Name` AS `CategoryName`
                        FROM `MysqlDemoItems` item
                        LEFT JOIN `MysqlDemoCategories` category ON item.`CategoryId` = category.`Id`
                    """;

                    // [教學註解] 動態加入搜尋條件 (WHERE)
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        sqlText += " WHERE item.`Name` LIKE @keyword";
                        // [教學註解] ⚠️ 絕對禁止字串拼接！必須使用 Parameter 參數化查詢，防止 SQL Injection (隱碼攻擊)
                        command.Parameters.Add(new MySqlParameter("@keyword", $"%{keyword}%"));
                    }

                    sqlText += " ORDER BY item.`CreatedAt` DESC";
                    command.CommandText = sqlText;

                    // [教學註解] ExecuteReaderAsync 會回傳一個 DataReader，這是一種流式 (Streaming) 讀取方式。
                    // 它不會一次把幾百萬筆資料塞爆記憶體，而是透過 ReadAsync() 逐筆向資料庫要資料。
                    await using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // [教學註解] ReadAsync() 會逐筆將資料拉到應用程式記憶體中。
                        while (await reader.ReadAsync())
                        {
                            MysqlDemoItem item = new MysqlDemoItem
                            {
                                // [教學註解] 透過索引值取出對應的欄位，這是最快的。
                                // 如果要用欄位名稱取值，可以使用 reader.GetOrdinal("Id") 取得索引。
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                CreatedAt = reader.GetDateTime(2),
                                // [教學註解] 原生讀取資料時，針對可能為 NULL 的欄位，必須先呼叫 IsDBNull 進行檢查。
                                // 否則呼叫 GetString 或 GetInt32 時會引發 SqlNullValueException！
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CategoryId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                            };

                            // [教學註解] 如果有對應的 CategoryName，就手動建構關聯物件 (Navigation Property)
                            if (!reader.IsDBNull(5))
                            {
                                item.Category = new MysqlDemoCategory { Name = reader.GetString(5) };
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
            // 在 MySQL 中呼叫 Procedure 通常使用 CALL
            await _context.Database.ExecuteSqlInterpolatedAsync($"CALL SP_UPDATE_ITEM_DESCRIPTION({id}, {newDescription})");
        }
    }
}

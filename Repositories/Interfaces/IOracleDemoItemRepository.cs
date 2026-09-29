using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Models;

namespace DotNetMvcWeb.Repositories.Interfaces
{
    /// <summary>
    /// [教學註解] 倉儲層介面 (Repository Interface)
    /// 專注於 OracleDemoItem 的資料存取契約，包含 EF Core 查詢、原生 ADO.NET 與預存程序呼叫。
    /// </summary>
    public interface IOracleDemoItemRepository
    {
        Task<List<OracleDemoItem>> GetItemsAsync(string? keyword = null);
        Task<OracleDemoItem?> GetItemByIdAsync(int id, bool includeCategory = false);
        Task AddItemAsync(OracleDemoItem item);
        Task UpdateItemAsync(OracleDemoItem item);
        Task DeleteItemAsync(int id);
        bool ItemExists(int id);
        Task<List<OracleDemoItem>> GetItemsViaAdoNetAsync(string? keyword = null);
        Task UpdateItemDescriptionViaProcAsync(int id, string newDescription);
    }
}

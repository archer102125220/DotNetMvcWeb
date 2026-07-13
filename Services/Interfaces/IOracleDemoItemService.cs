using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Models;

namespace DotNetMvcWeb.Services.Interfaces
{
    /// <summary>
    /// [教學註解] 服務層介面 (Service Interface)
    /// 透過介面定義功能，能讓 Controller 與具體的實作解耦 (Decoupling)。
    /// 未來在撰寫單元測試時，可以很輕易地 Mock 這個介面，而不需要真的連線到資料庫。
    /// </summary>
    public interface IOracleDemoItemService
    {
        Task<List<OracleDemoItem>> GetItemsAsync(string? keyword = null);
        Task<OracleDemoItem?> GetItemByIdAsync(int id, bool includeCategory = false);
        Task CreateItemAsync(OracleDemoItem item);
        Task UpdateItemAsync(OracleDemoItem item);
        Task DeleteItemAsync(int id);
        bool ItemExists(int id);
        Task<List<OracleDemoItem>> GetItemsViaAdoNetAsync(string? keyword = null);
        Task UpdateItemDescriptionViaProcAsync(int id, string newDescription);
    }
}

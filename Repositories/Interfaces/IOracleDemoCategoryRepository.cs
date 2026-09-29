using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetMvcWeb.Models;

namespace DotNetMvcWeb.Repositories.Interfaces
{
    /// <summary>
    /// [教學註解] 倉儲層介面 (Repository Interface)
    /// 專注於 OracleDemoCategory 的資料存取契約，將資料庫細節與商業邏輯層 (Service) 隔離。
    /// </summary>
    public interface IOracleDemoCategoryRepository
    {
        Task<List<OracleDemoCategory>> GetCategoriesAsync();
        Task<OracleDemoCategory?> GetCategoryByIdAsync(int id);
        Task AddCategoryAsync(OracleDemoCategory category);
        Task UpdateCategoryAsync(OracleDemoCategory category);
        Task DeleteCategoryAsync(int id);
        bool CategoryExists(int id);
    }
}

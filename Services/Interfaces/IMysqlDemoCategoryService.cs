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
    public interface IMysqlDemoCategoryService
    {
        Task<List<MysqlDemoCategory>> GetCategoriesAsync();
        Task<MysqlDemoCategory?> GetCategoryByIdAsync(int id);
        Task CreateCategoryAsync(MysqlDemoCategory category);
        Task UpdateCategoryAsync(MysqlDemoCategory category);
        Task DeleteCategoryAsync(int id);
        bool CategoryExists(int id);
    }
}
